using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using AnimBakery.Cook;
using AnimBakery.Cook.Model;
using Unity.Transforms;
namespace AnimBakery {
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
				// frameRate = frameRate,
			});
			dstManager.AddSharedComponentData(entity, init);
		}

		private BakedMeshData[] Create(GameObject prototype) {
			var srms = prototype.GetComponentsInChildren<SkinnedMeshRenderer>();
			BakedMeshData[] bakery = new BakedMeshData[srms.Length];
			for(int i = 0; i < srms.Length; i++) {
				bakery[i] = new BakeryFactory(prototype, srms[i]).Create().BakeClips(clips, frameRate);
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