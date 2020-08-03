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
		private BakedData[] _dataBase;

		private ComputeBuffer[] _argsBuffer;
		private ComputeBuffer[] _textureCoordinatesBuffer;
		private ComputeBuffer[] _objectRotationsBuffer;
		private ComputeBuffer[] _objectPositionsBuffer;
		private float[] _times;
		private int _count = -1;

		public GPUAnimDrawer(BakedData[] bakery, AnimComponent config, List<Clip> clips) {
			this._dataBase = bakery;
			this._config = config;
			_InitBuffers();
		}

		private void _InitBuffers() {
			Dispose();
			_count = _dataBase.Length;
			_argsBuffer = new ComputeBuffer[_dataBase.Length];
			_objectRotationsBuffer = new ComputeBuffer[_dataBase.Length];
			_objectPositionsBuffer = new ComputeBuffer[_dataBase.Length];
			_textureCoordinatesBuffer = new ComputeBuffer[_dataBase.Length];
			for (int i = 0; i < _count; i++) {
				_argsBuffer[i] = new ComputeBuffer(1, _indirectArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

				_objectRotationsBuffer[i] = new ComputeBuffer(1, sizeof(float) * 4);
				_objectPositionsBuffer[i] = new ComputeBuffer(1, sizeof(float) * 4);
				_textureCoordinatesBuffer[i] = new ComputeBuffer(1, sizeof(float));
			}
			_times = new float[_count];

			_textureCoordinates = new NativeList<float>(1, Allocator.Persistent);
			_objectPositions = new NativeList<float4>(1, Allocator.Persistent);
			_objectRotations = new NativeList<quaternion>(1, Allocator.Persistent);
		}

		NativeList<float> _textureCoordinates;
		NativeList<float4> _objectPositions;
		NativeList<quaternion> _objectRotations;

		public void Draw(float deltaTime, float3 position, quaternion rotation, float scale = 1) {
			for (int i = 0; i < _dataBase.Length; i++) {
				Profiler.BeginSample("Prepare shader dataBase[i]");

				_textureCoordinates.Clear();
				_objectPositions.Clear();
				_objectRotations.Clear();

				var x = position.x;
				var y = position.y;
				var z = position.z;

				var clip = _dataBase[i][_config.animationId];
				var dt = deltaTime + deltaTime * (_config.addAnimationDifference ? Random.Range(-0.5f, 0.5f) : 0);

				_times[i] += dt * _config.timeMultiplier;
				if (_times[i] > clip.ClipLength) _times[i] %= clip.ClipLength;

				var normalizedTime = _config.animated ? _times[i] / clip.ClipLength : _config.normalizedTime;
				var frameIndex = (int)((clip.FramesCount - 1) * normalizedTime);

				_textureCoordinates.Add(clip.Start + frameIndex * _dataBase[i].BonesCount * 3.0f);
				_objectPositions.Add(new float4(x, y, z, scale));
				_objectRotations.Add(rotation);

				Profiler.EndSample();

				Profiler.BeginSample("Shader set dataBase[i]");

				_objectRotationsBuffer[i].SetData((NativeArray<quaternion>)_objectRotations, 0, 0, 1);
				_objectPositionsBuffer[i].SetData((NativeArray<float4>)_objectPositions, 0, 0, 1);
				_textureCoordinatesBuffer[i].SetData((NativeArray<float>)_textureCoordinates, 0, 0, 1);

				_dataBase[i].Material.SetBuffer(TextureCoordinatesBufferProperty, _textureCoordinatesBuffer[i]);
				_dataBase[i].Material.SetBuffer(ObjectPositionsBufferProperty, _objectPositionsBuffer[i]);
				_dataBase[i].Material.SetBuffer(ObjectRotationsBufferProperty, _objectRotationsBuffer[i]);

				_dataBase[i].Material.SetVector(AnimationTextureSizeProperty, new Vector2(_dataBase[i].Texture.width, _dataBase[i].Texture.height));
				_dataBase[i].Material.SetTexture(AnimationTextureProperty, _dataBase[i].Texture);

				Profiler.EndSample();

				_indirectArgs[0] = _dataBase[i].Mesh.GetIndexCount(0);
				_indirectArgs[1] = (uint)1;
				_argsBuffer[i].SetData(_indirectArgs);

				Graphics.DrawMeshInstancedIndirect(_dataBase[i].Mesh,
					0,
					_dataBase[i].Material,
					new Bounds(Vector3.zero, 1000 * Vector3.one),
					_argsBuffer[i],
					0,
					new MaterialPropertyBlock());

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