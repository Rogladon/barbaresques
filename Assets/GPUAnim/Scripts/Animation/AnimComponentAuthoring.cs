using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Barbarian.Animations.Cook;
using Unity.Transforms;
using Unity.Mathematics;

namespace Barbarian.Animations {
	public class AnimComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
		public float frameRate = 30;

		public float timeMultiplier = 1.0f;
		public bool addAnimationDifference;

		public bool animated = true;

		public float normalizedTime = 1.0f;
		public GameObject body;
		public List<Clip> clips;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
			// Debug.Log(gameObject.name);

			AnimInitComponent init = new AnimInitComponent {
				bakery = Create(body),
				// clips = clips,
			};

			dstManager.AddComponentData(entity, new AnimationConfig {
				timeMultiplier = timeMultiplier,
				animated = animated,
				animationId = clips[0].name,
				addAnimationDifference = addAnimationDifference,
				normalizedTime = normalizedTime,
				tint = new float4(1,1,1,1),
				// frameRate = frameRate,
			});
			dstManager.AddSharedComponentData(entity, init);
		}

		private BakedMeshData[] Create(GameObject prototype) {
			var meshRenderers = prototype.GetComponentsInChildren<SkinnedMeshRenderer>();
			BakedMeshData[] bakery = new BakedMeshData[meshRenderers.Length];
			for(int i = 0; i < meshRenderers.Length; i++) {
				bakery[i] = new BakeryFactory(prototype, meshRenderers[i]).Create().BakeClips(clips, frameRate);
			}
			return bakery;
		}
	}
	[System.Serializable]
	public struct Clip {
		public string name;
		public AnimationClip clip;
	}
}