using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace AnimBakery.Cook.Model {
	public struct BakedData {
		private Texture2D[] _textures;

		private float _frameRate;
		private int _bonesCount;
		private List<AnimationClipData> _animations;
		private Dictionary<string, AnimationClipData> _animationsDictionary;
		private Mesh _mesh;
		private Material _material;
		public Material Material => _material;
		public Texture2D Texture => _textures[0];
		public Mesh Mesh => _mesh;

		public float FrameRate => _frameRate;
		public int BonesCount => _bonesCount;
		public int Count => _animations.Count;
		public AnimationClipData this[int index] => _animations[index];
		public AnimationClipData this[string index] => _animationsDictionary[index];

		public Texture2D GetTexture(int index) => index < _textures.Length ? _textures[index] : null;

		public static BakedData Copy(BakedData data, Material mat) {
			BakedData b;
			b._textures = data._textures;
			b._mesh = data._mesh;
			b._material = mat;
			b._frameRate = data._frameRate;
			b._bonesCount = data._bonesCount;
			b._animations = data._animations;

			b._animationsDictionary = data._animationsDictionary;
			return b;
		}

		private BakedData(Texture2D[] textures,
						  Mesh mesh,
						  Material material,
						  float frameRate,
						  int bonesCount,
						  List<AnimationClipData> animations) {
			this._textures = textures;
			this._mesh = mesh;
			this._material = material;
			this._frameRate = frameRate;
			this._bonesCount = bonesCount;
			this._animations = animations;

			this._animationsDictionary = new Dictionary<string, AnimationClipData>();
			foreach (var clipData in animations)
				_animationsDictionary[clipData.Name] = clipData;
		}



		public static BakedDataBuilder Bulder(uint textCapasity = 1) {
			return new BakedDataBuilder(textCapasity);
		}

		public class BakedDataBuilder {
			private Texture2D[] textures;
			private Mesh mesh;
			private Material material;
			private float frameRate = 30;
			private int bonesCount = -1;
			private List<AnimationClipData> animations = new List<AnimationClipData>();

			internal BakedDataBuilder(uint textCapasity) {
				textures = new Texture2D[textCapasity];
			}

			public BakedDataBuilder SetTexture(uint id, Texture2D texture) {
				textures[id] = texture;
				return this;
			}

			public BakedDataBuilder SetMesh(Mesh m) {
				mesh = m;
				return this;
			}

			public BakedDataBuilder SetMaterial(Material m) {
				material = m;
				return this;
			}

			public BakedDataBuilder SetFrameRate(float fr) {
				frameRate = fr;
				return this;
			}

			public BakedDataBuilder SetBonesCount(int bc) {
				bonesCount = bc;
				return this;
			}

			public BakedDataBuilder AddClip(AnimationClipData clipData) {
				animations.Add(clipData);
				return this;
			}

			public BakedData Build() {
				if (bonesCount == -1) throw new System.NullReferenceException("Bones count shouldn't be -1");
				if (mesh == null) throw new System.NullReferenceException("Mesh shouldn't be null");
				if (material == null) throw new System.NullReferenceException("Material shouldn't be null");
				for (var index = 0; index < textures.Length; ++index)
					if (textures[index] == null)
						throw new System.NullReferenceException($"Texture {index} shouldn't be null");


				return new BakedData(textures, mesh, material, frameRate, bonesCount, animations);
			}


		}
	}
}