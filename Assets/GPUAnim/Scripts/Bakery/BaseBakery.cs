using System;
using System.Collections.Generic;
using AnimBakery.Cook.Model;
using UnityEngine;

namespace AnimBakery.Cook {
	public abstract class BaseBakery : IBakery {
		private const int MATRIX_ROWS_COUNT = 3;

		protected abstract List<Matrix4x4[,]> SampleAnimationClips(float frameRate,
			List<Clip> animationClips,
			out int numberOfKeyFrames,
			out int numberOfBones);

		public BakedMeshData BakeClips(List<Clip> clips, float frameRate = 30f) {
			OnBeginBakeClips();

			var bakedDataBuilder = BakedMeshData.Builder(1)
				.SetMaterial(CreateMaterial())
				.SetMesh(CreateMesh())
				.SetFrameRate(frameRate);

			var sampledBoneMatrices = SampleAnimationClips(frameRate,
														   clips,
														   out var numberOfKeyFrames,
														   out var numberOfBones);


			var size = ((int)Math.Sqrt(numberOfBones * numberOfKeyFrames * MATRIX_ROWS_COUNT)).NextPowerOfTwo();
			var texture = new Texture2D(size, size, TextureFormat.RGBAFloat, false) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};
			var textureColor = new Color[texture.width * texture.height];
			bakedDataBuilder.SetTexture(0, texture);
			bakedDataBuilder.SetBonesCount(numberOfBones);

			var clipOffset = 0;
			for (var clipIndex = 0; clipIndex < sampledBoneMatrices.Count; clipIndex++) {
				var framesCount = sampledBoneMatrices[clipIndex].GetLength(0);
				for (var keyframeIndex = 0; keyframeIndex < framesCount; keyframeIndex++) {
					var frameOffset = keyframeIndex * numberOfBones * MATRIX_ROWS_COUNT;
					for (var boneIndex = 0; boneIndex < numberOfBones; boneIndex++) {
						var index = clipOffset + frameOffset + boneIndex * MATRIX_ROWS_COUNT;
						var matrix = sampledBoneMatrices[clipIndex][keyframeIndex, boneIndex];

						if ((Vector4)textureColor[index + 0] != Vector4.zero) Debug.LogError($"Index {index + 0} not empty");
						if ((Vector4)textureColor[index + 1] != Vector4.zero) Debug.LogError($"Index {index + 1} not empty");
						if ((Vector4)textureColor[index + 2] != Vector4.zero) Debug.LogError($"Index {index + 2} not empty");

						textureColor[index + 0] = matrix.GetRow(0);
						textureColor[index + 1] = matrix.GetRow(1);
						textureColor[index + 2] = matrix.GetRow(2);
					}
				}

				var clip = clips[clipIndex].clip;
				var name = clips[clipIndex].name;
				var start = clipOffset;
				var end = clipOffset + (framesCount - 1) * MATRIX_ROWS_COUNT;

				var clipData = AnimationClipData.Create(clip,
														name,
														start,
														end,
														framesCount);

				bakedDataBuilder.AddClip(clipData);

				clipOffset += framesCount * numberOfBones * MATRIX_ROWS_COUNT;
			}

			texture.SetPixels(textureColor);
			texture.Apply(false, false);

			clipOffset = 0;
			for (var clipIndex = 0; clipIndex < sampledBoneMatrices.Count; clipIndex++) {
				var framesCount = sampledBoneMatrices[clipIndex].GetLength(0);
				for (var keyframeIndex = 0; keyframeIndex < framesCount; keyframeIndex++) {
					var frameOffset = keyframeIndex * numberOfBones * MATRIX_ROWS_COUNT;
					for (var boneIndex = 0; boneIndex < numberOfBones; boneIndex++) {
						var index = clipOffset + frameOffset + boneIndex * MATRIX_ROWS_COUNT;
						var matrix = sampledBoneMatrices[clipIndex][keyframeIndex, boneIndex];

						var color0 = textureColor[index];
						var index2D0 = To2D(index, texture.width);
						var pixel0 = texture.GetPixel(index2D0.x, index2D0.y);
						var row0 = (Color)matrix.GetRow(0);
						index++;

						var color1 = textureColor[index];
						var index2D1 = To2D(index, texture.width);
						var pixel1 = texture.GetPixel(index2D1.x, index2D1.y);
						var row1 = (Color)matrix.GetRow(1);
						index++;

						var color2 = textureColor[index];
						var index2D2 = To2D(index, texture.width);
						var pixel2 = texture.GetPixel(index2D2.x, index2D2.y);
						var row2 = (Color)matrix.GetRow(2);

						if (!Verify(pixel0, row0, color0, index2D0, clipIndex, keyframeIndex, boneIndex)) break;
						if (!Verify(pixel1, row1, color1, index2D1, clipIndex, keyframeIndex, boneIndex)) break;
						if (!Verify(pixel2, row2, color2, index2D2, clipIndex, keyframeIndex, boneIndex)) break;
					}
				}

				clipOffset += numberOfBones * framesCount * MATRIX_ROWS_COUNT;
			}

			var data = bakedDataBuilder.Build();

			OnEndBakeClips();
			return data;
		}

		protected virtual void OnBeginBakeClips() {}

		protected abstract Mesh CreateMesh();

		protected abstract Material CreateMaterial();

		protected virtual void OnEndBakeClips() {}

		public static Mesh CreateMesh(Mesh originalMesh) {
			var newMesh = originalMesh.Copy();
			var boneWeights = originalMesh.boneWeights;

			var boneIds = new List<Vector4>(originalMesh.vertexCount);
			var boneInfluences = new List<Vector4>(originalMesh.vertexCount);

			for (var i = 0; i < originalMesh.vertexCount; i++) {
				var bw = boneWeights[i];
				var bonesWeightsSorted = new List<Tuple<int, float>>(4)
				{
					Tuple.Create(bw.boneIndex0, bw.weight0),
					Tuple.Create(bw.boneIndex1, bw.weight1),
					Tuple.Create(bw.boneIndex2, bw.weight2),
					Tuple.Create(bw.boneIndex3, bw.weight3)
				};
				bonesWeightsSorted.Sort((b1, b2) => b1.Item2 < b2.Item2 ? 1 : -1);

				boneIds.Add(new Vector4 {
					x = bonesWeightsSorted[0].Item1,
					y = bonesWeightsSorted[1].Item1,
					z = bonesWeightsSorted[2].Item1,
					w = bonesWeightsSorted[3].Item1
				});

				boneInfluences.Add(new Vector4 {
					x = bonesWeightsSorted[0].Item2,
					y = bonesWeightsSorted[1].Item2,
					z = bonesWeightsSorted[2].Item2,
					w = bonesWeightsSorted[3].Item2
				});
			}

			newMesh.SetUVs(1, boneIds);
			newMesh.SetUVs(2, boneInfluences);

			return newMesh;
		}

		private static bool Verify(Color pixel, Color row, Color color, Vector2Int index2D,
			int clipIndex, int keyframeIndex, int boneIndex) {
			if (pixel != row && row != color) {
				Debug.LogError("Error at (" + clipIndex + ", " + keyframeIndex + ", " + boneIndex + ")" +
							   " expected " + row.ToString() +
							   " Texture(" + index2D.ToString() +
							   " but got " + pixel.ToString() +
							   " in color array " + color.ToString());
				return false;
			}
			return true;
		}

		public static Vector2Int To2D(int index, int width) {
			var y = index / width;
			var x = index % width;
			return new Vector2Int(x, y);
		}
	}
}