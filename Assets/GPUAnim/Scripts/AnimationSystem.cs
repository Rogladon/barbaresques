#define USE_SAFE_JOBS

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using AnimBakery.Cook.Model;
using UnityEngine.Profiling;

namespace AnimBakery {
	public struct AnimationState : ISystemStateComponentData {
		public float time;
	}

	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class AnimationSystem : SystemBase {
			private class AnimatedMeshRenderer : IDisposable {
			private static readonly int TextureCoordinatesBufferProperty = Shader.PropertyToID("textureCoordinatesBuffer");
			private static readonly int ObjectPositionsBufferProperty = Shader.PropertyToID("objectPositionsBuffer");
			private static readonly int ObjectRotationsBufferProperty = Shader.PropertyToID("objectRotationsBuffer");

			private static readonly int AnimationTextureSizeProperty = Shader.PropertyToID("animationTextureSize");
			private static readonly int AnimationTextureProperty = Shader.PropertyToID("animationTexture");

			private readonly uint[] _indirectArgs = { 0, 0, 0, 0, 0 };

			private ComputeBuffer[] _argsBuffer;
			private ComputeBuffer[] _textureCoordinatesBuffer;
			private ComputeBuffer[] _objectRotationsBuffer;
			private ComputeBuffer[] _objectPositionsBuffer;

			List<float> _textureCoordinates;
			List<float4> _objectPositions;
			List<quaternion> _objectRotations;
			MaterialPropertyBlock _mpb;

			private AnimationData _data;

			private bool buffersInitialized = false;
			private uint _instancesCount = 0;

			private BakedMeshData[] _bakedMeshes => _data.baked;
			private int _meshesCount => _bakedMeshes.Length;

			private readonly static int maxInstances = 1024;

			public AnimatedMeshRenderer(AnimationData data) {
				_data = data;
			}

			public void Prepare() {
				if (!buffersInitialized) {
					_InitBuffers();
				}

				_instancesCount = 0;
				_textureCoordinates.Clear();
				_objectPositions.Clear();
				_objectRotations.Clear();
			}

			public void AddInstance(float deltaTime, ref AnimationState ass, in Translation translation, in Rotation rotation, in AnimationConfig config) {
				float scale = 1;
				_instancesCount++;
				var pos = translation.Value;
				var rot = rotation.Value;
				var dt = deltaTime + deltaTime * (config.addAnimationDifference ? UnityEngine.Random.Range(-0.5f, 0.5f) : 0);
				ass.time += dt * config.timeMultiplier;
				for (int i = 0; i < _meshesCount; i++) {
					var clip = _bakedMeshes[i][config.animationId];
					var t = ass.time;
					if (t > clip.ClipLength) {
						t %= clip.ClipLength;
					}

					Profiler.BeginSample("Prepare instances");

					var normalizedTime = config.animated ? t / clip.ClipLength : config.normalizedTime;
					var frameIndex = (int)((clip.FramesCount - 1) * normalizedTime);

					_textureCoordinates.Add((clip.Start + frameIndex * _bakedMeshes[i].BonesCount * 3.0f));
					_objectPositions.Add(new float4(pos, scale));
					_objectRotations.Add(rot);

					Profiler.EndSample();
				}
			}

			public void Draw() {
				for (int i = 0; i < _bakedMeshes.Length; i++) {
					Profiler.BeginSample("Set buffers");

					_objectRotationsBuffer[i].SetData(_objectRotations, 0, 0, _objectRotations.Count);
					_objectPositionsBuffer[i].SetData(_objectPositions, 0, 0, _objectPositions.Count);
					_textureCoordinatesBuffer[i].SetData(_textureCoordinates, 0, 0, _textureCoordinates.Count);

					_bakedMeshes[i].Material.SetBuffer(TextureCoordinatesBufferProperty, _textureCoordinatesBuffer[i]);
					_bakedMeshes[i].Material.SetBuffer(ObjectPositionsBufferProperty, _objectPositionsBuffer[i]);
					_bakedMeshes[i].Material.SetBuffer(ObjectRotationsBufferProperty, _objectRotationsBuffer[i]);

					Profiler.EndSample();

					int submeshIndex = 0;

					_indirectArgs[0] = _bakedMeshes[i].Mesh.GetIndexCount(submeshIndex);
					_indirectArgs[1] = _instancesCount;
					_indirectArgs[2] = _bakedMeshes[i].Mesh.GetIndexStart(submeshIndex);
					_indirectArgs[3] = _bakedMeshes[i].Mesh.GetBaseVertex(submeshIndex);
					_argsBuffer[i].SetData(_indirectArgs);

					Profiler.BeginSample(nameof(Graphics.DrawMeshInstancedIndirect));

					Graphics.DrawMeshInstancedIndirect(_bakedMeshes[i].Mesh,
						0,
						_bakedMeshes[i].Material,
						new Bounds(Vector3.zero, 1000 * Vector3.one),
						_argsBuffer[i],
						0,
						_mpb);

					Profiler.EndSample();
				}
			}


			private void _InitBuffers() {
				buffersInitialized = true;

				_objectPositions = new List<float4>();
				_objectRotations = new List<quaternion>();
				_textureCoordinates = new List<float>();

				_argsBuffer = new ComputeBuffer[_meshesCount];
				_objectRotationsBuffer = new ComputeBuffer[_meshesCount];
				_objectPositionsBuffer = new ComputeBuffer[_meshesCount];
				_textureCoordinatesBuffer = new ComputeBuffer[_meshesCount];
				for (int i = 0; i < _meshesCount; i++) {
					_argsBuffer[i] = new ComputeBuffer(maxInstances, _indirectArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

					_objectRotationsBuffer[i] = new ComputeBuffer(maxInstances, sizeof(float) * 4);
					_objectPositionsBuffer[i] = new ComputeBuffer(maxInstances, sizeof(float) * 4);
					_textureCoordinatesBuffer[i] = new ComputeBuffer(maxInstances, sizeof(float));

					_bakedMeshes[i].Material.SetVector(AnimationTextureSizeProperty, new Vector2(_bakedMeshes[i].Texture.width, _bakedMeshes[i].Texture.height));
					_bakedMeshes[i].Material.SetTexture(AnimationTextureProperty, _bakedMeshes[i].Texture);
				}

				_mpb = new MaterialPropertyBlock();
			}

			public void Dispose() {
				for (int i = 0; i < _meshesCount; i++) {
					_argsBuffer[i]?.Dispose();
					_objectPositionsBuffer[i]?.Dispose();
					_objectRotationsBuffer[i]?.Dispose();
					_textureCoordinatesBuffer[i]?.Dispose();
				}
			}
		}

		private Dictionary<AnimationData, AnimatedMeshRenderer> _renderers = new Dictionary<AnimationData, AnimatedMeshRenderer>();

		protected override void OnUpdate() {
			var delta = Time.DeltaTime;

			Entities
				.WithAll<AnimationConfig>()
				.WithNone<AnimationState>()
				.WithStructuralChanges()
				.ForEach((Entity e, in AnimationData data) => {
					EntityManager.AddComponentData(e, new AnimationState {
						time = 0
					});
				}).Run();

			Entities
				.WithNone<AnimationConfig>()
				.WithAll<AnimationState>()
				.WithStructuralChanges()
				.ForEach((Entity e, in AnimationState ass) => {
					EntityManager.RemoveComponent<AnimationState>(e);
				}).Run();

			List<AnimationData> uniqueAnimData = new List<AnimationData>();
			EntityManager.GetAllUniqueSharedComponentData(uniqueAnimData);

			AnimatedMeshRenderer renderer = null;
			foreach (var animData in uniqueAnimData) {
				bool configured = false;

				Entities.WithName("Animation")
					.WithSharedComponentFilter(animData)
					.WithoutBurst()
					.ForEach((ref AnimationState ass, in Translation translation, in Rotation rotation, in AnimationConfig config) => {
						if (!configured) {
							if (_renderers.TryGetValue(animData, out AnimatedMeshRenderer amr)) {
								renderer = amr;
							} else {
								renderer = new AnimatedMeshRenderer(animData);
								_renderers[animData] = renderer;
							}

							renderer.Prepare();

							configured = true;
						}
						renderer.AddInstance(delta, ref ass, translation, rotation, config);
					})
					.Run();

				if (configured) {
					renderer.Draw();
				} else if (_renderers.TryGetValue(animData, out AnimatedMeshRenderer amr)) {
					amr.Dispose();
					_renderers.Remove(animData);
				}
			}
		}

		protected override void OnDestroy() {
			foreach (var v in _renderers.Values) {
				v.Dispose();
			}
			_renderers = null;
		}
	}
}
