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

		private readonly uint[] indirectArgs = { 0, 0, 0, 0, 0 };
		private readonly AnimComponent config;
		public BakedData[] dataBase;

		private ComputeBuffer[] argsBuffer;
		private ComputeBuffer[] textureCoordinatesBuffer;
		private ComputeBuffer[] objectRotationsBuffer;
		private ComputeBuffer[] objectPositionsBuffer;
		private float[] times;
		int count = -1;

		public GPUAnimDrawer(BakedData[] bakery, AnimComponent config, List<Clip> clips) {
			this.dataBase = bakery;
			this.config = config;
			InitBuffers();
		}

		private void InitBuffers() {
			Dispose();
			count = dataBase.Length;
			argsBuffer = new ComputeBuffer[dataBase.Length];
			objectRotationsBuffer = new ComputeBuffer[dataBase.Length];
			objectPositionsBuffer = new ComputeBuffer[dataBase.Length];
			textureCoordinatesBuffer = new ComputeBuffer[dataBase.Length];
			for (int i = 0; i < count; i++) {
				argsBuffer[i] = new ComputeBuffer(1, indirectArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

				objectRotationsBuffer[i] = new ComputeBuffer(1, sizeof(float) * 4);
				objectPositionsBuffer[i] = new ComputeBuffer(1, sizeof(float) * 4);
				textureCoordinatesBuffer[i] = new ComputeBuffer(1, sizeof(float));
			}
			times = new float[count];

			textureCoordinates = new NativeList<float>(1, Allocator.Persistent);
			objectPositions = new NativeList<float4>(1, Allocator.Persistent);
			objectRotations = new NativeList<quaternion>(1, Allocator.Persistent);
		}

		NativeList<float> textureCoordinates;
		NativeList<float4> objectPositions;
		NativeList<quaternion> objectRotations;

		public void Draw(float deltaTime, float3 position, quaternion rotation, float scale = 1) {
			for (int i = 0; i < dataBase.Length; i++) {
				Profiler.BeginSample("Prepare shader dataBase[i]");

				textureCoordinates.Clear();
				objectPositions.Clear();
				objectRotations.Clear();

				var x = position.x;
				var y = position.y;
				var z = position.z;

				var clip = dataBase[i][config.animationId];
				var dt = deltaTime + deltaTime * (config.addAnimationDifference ? Random.Range(-0.5f, 0.5f) : 0);

				times[i] += dt * config.timeMultiplier;
				if (times[i] > clip.ClipLength) times[i] %= clip.ClipLength;

				var normalizedTime = config.animated ? times[i] / clip.ClipLength : config.normalizedTime;
				var frameIndex = (int)((clip.FramesCount - 1) * normalizedTime);

				textureCoordinates.Add(clip.Start + frameIndex * dataBase[i].BonesCount * 3.0f);
				objectPositions.Add(new float4(x, y, z, scale));
				objectRotations.Add(rotation);

				Profiler.EndSample();

				Profiler.BeginSample("Shader set dataBase[i]");

				objectRotationsBuffer[i].SetData((NativeArray<quaternion>)objectRotations, 0, 0, 1);
				objectPositionsBuffer[i].SetData((NativeArray<float4>)objectPositions, 0, 0, 1);
				textureCoordinatesBuffer[i].SetData((NativeArray<float>)textureCoordinates, 0, 0, 1);

				dataBase[i].Material.SetBuffer(TextureCoordinatesBufferProperty, textureCoordinatesBuffer[i]);
				dataBase[i].Material.SetBuffer(ObjectPositionsBufferProperty, objectPositionsBuffer[i]);
				dataBase[i].Material.SetBuffer(ObjectRotationsBufferProperty, objectRotationsBuffer[i]);

				dataBase[i].Material.SetVector(AnimationTextureSizeProperty, new Vector2(dataBase[i].Texture.width, dataBase[i].Texture.height));
				dataBase[i].Material.SetTexture(AnimationTextureProperty, dataBase[i].Texture);

				Profiler.EndSample();

				indirectArgs[0] = dataBase[i].Mesh.GetIndexCount(0);
				indirectArgs[1] = (uint)1;
				argsBuffer[i].SetData(indirectArgs);

				Graphics.DrawMeshInstancedIndirect(dataBase[i].Mesh,
					0,
					dataBase[i].Material,
					new Bounds(Vector3.zero, 1000 * Vector3.one),
					argsBuffer[i],
					0,
					new MaterialPropertyBlock());

			}
		}

		public void Dispose() {
			if (textureCoordinates.IsCreated) textureCoordinates.Dispose();
			if (objectPositions.IsCreated) objectPositions.Dispose();
			if (objectRotations.IsCreated) objectRotations.Dispose();

			if (count <= 0) return;
			for (int i = 0; i < count; i++) {
				argsBuffer[i]?.Dispose();
				objectPositionsBuffer[i]?.Dispose();
				objectRotationsBuffer[i]?.Dispose();
				textureCoordinatesBuffer[i]?.Dispose();
			}

		}
	}
}