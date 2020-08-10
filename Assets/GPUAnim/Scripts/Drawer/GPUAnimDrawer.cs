#define USE_SAFE_JOBS

using System;
using System.Collections.Generic;
using System.Linq;
using AnimBakery.Cook;
using AnimBakery.Cook.Model;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace AnimBakery.Draw {
	public class GPUAnimDrawer : IDisposable {
		private static readonly int TextureCoordinatesBufferProperty = Shader.PropertyToID("textureCoordinatesBuffer");
		private static readonly int ObjectPositionsBufferProperty = Shader.PropertyToID("objectPositionsBuffer");
		private static readonly int ObjectRotationsBufferProperty = Shader.PropertyToID("objectRotationsBuffer");

		private static readonly int AnimationTextureSizeProperty = Shader.PropertyToID("animationTextureSize");
		private static readonly int AnimationTextureProperty = Shader.PropertyToID("animationTexture");

		private readonly uint[] _indirectArgs = { 0, 0, 0, 0, 0 };
		private readonly AnimComponent _config;
		private BakedData[] _bakedClips;

		private ComputeBuffer[] _argsBuffer;
		private ComputeBuffer[] _textureCoordinatesBuffer;
		private ComputeBuffer[] _objectRotationsBuffer;
		private ComputeBuffer[] _objectPositionsBuffer;
		private float[] _times;
		private int _count = -1;

		public GPUAnimDrawer(BakedData[] bakery, AnimComponent config, List<Clip> clips) {
			this._bakedClips = bakery;
			this._config = config;
			_InitBuffers();
		}

		private void _InitBuffers() {
			Dispose();

			_textureCoordinates = new NativeArray<float>(1, Allocator.Persistent);
			_objectPositions = new NativeArray<float4>(1, Allocator.Persistent);
			_objectRotations = new NativeArray<quaternion>(1, Allocator.Persistent);

			_count = _bakedClips.Length;
			_argsBuffer = new ComputeBuffer[_bakedClips.Length];
			_objectRotationsBuffer = new ComputeBuffer[_bakedClips.Length];
			_objectPositionsBuffer = new ComputeBuffer[_bakedClips.Length];
			_textureCoordinatesBuffer = new ComputeBuffer[_bakedClips.Length];
			for (int i = 0; i < _count; i++) {
				_argsBuffer[i] = new ComputeBuffer(1, _indirectArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

				_objectRotationsBuffer[i] = new ComputeBuffer(1, sizeof(float) * 4);
				_objectPositionsBuffer[i] = new ComputeBuffer(1, sizeof(float) * 4);
				_textureCoordinatesBuffer[i] = new ComputeBuffer(1, sizeof(float));

				_bakedClips[i].Material.SetVector(AnimationTextureSizeProperty, new Vector2(_bakedClips[i].Texture.width, _bakedClips[i].Texture.height));
				_bakedClips[i].Material.SetTexture(AnimationTextureProperty, _bakedClips[i].Texture);
			}
			_times = new float[_count];

			_mpb = new MaterialPropertyBlock();
		}

		NativeArray<float> _textureCoordinates;
		NativeArray<float4> _objectPositions;
		NativeArray<quaternion> _objectRotations;
		MaterialPropertyBlock _mpb;

		public void Draw(float deltaTime, float3 position, quaternion rotation, float scale = 1) {
			for (int i = 0; i < _bakedClips.Length; i++) {
				Profiler.BeginSample("Prepare shader dataBase[i]");

				var clip = _bakedClips[i][_config.animationId];
				var dt = deltaTime + deltaTime * (_config.addAnimationDifference ? Random.Range(-0.5f, 0.5f) : 0);

				_times[i] += dt * _config.timeMultiplier;
				if (_times[i] > clip.ClipLength) _times[i] %= clip.ClipLength;

				var normalizedTime = _config.animated ? _times[i] / clip.ClipLength : _config.normalizedTime;
				var frameIndex = (int)((clip.FramesCount - 1) * normalizedTime);

				_textureCoordinates[0] = (clip.Start + frameIndex * _bakedClips[i].BonesCount * 3.0f);
				_objectPositions[0] = new float4(position, scale);
				_objectRotations[0] = rotation;

				Profiler.EndSample();

				Profiler.BeginSample("Shader set dataBase[i]");

				_objectRotationsBuffer[i].SetData(_objectRotations, 0, 0, 1);
				_objectPositionsBuffer[i].SetData(_objectPositions, 0, 0, 1);
				_textureCoordinatesBuffer[i].SetData(_textureCoordinates, 0, 0, 1);

				_bakedClips[i].Material.SetBuffer(TextureCoordinatesBufferProperty, _textureCoordinatesBuffer[i]);
				_bakedClips[i].Material.SetBuffer(ObjectPositionsBufferProperty, _objectPositionsBuffer[i]);
				_bakedClips[i].Material.SetBuffer(ObjectRotationsBufferProperty, _objectRotationsBuffer[i]);

				Profiler.EndSample();

				_indirectArgs[0] = _bakedClips[i].Mesh.GetIndexCount(0);
				_indirectArgs[1] = (uint)1;
				_argsBuffer[i].SetData(_indirectArgs);

				Graphics.DrawMeshInstancedIndirect(_bakedClips[i].Mesh,
					0,
					_bakedClips[i].Material,
					new Bounds(position, 10 * Vector3.one),
					_argsBuffer[i],
					0,
					_mpb);
			}
		}

		public void Dispose() {
			if (_textureCoordinates.IsCreated) _textureCoordinates.Dispose();
			if (_objectPositions.IsCreated) _objectPositions.Dispose();
			if (_objectRotations.IsCreated) _objectRotations.Dispose();

			if (_count <= 0) return;
			for (int i = 0; i < _count; i++) {
				_argsBuffer[i]?.Dispose();
				_objectPositionsBuffer[i]?.Dispose();
				_objectRotationsBuffer[i]?.Dispose();
				_textureCoordinatesBuffer[i]?.Dispose();
			}

		}
	}
}