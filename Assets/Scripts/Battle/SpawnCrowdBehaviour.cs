using System.Collections;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Barbaresques.Battle {
	public enum CrowdFormationTypes {
		SQUARE,
		TRIANGLE,
	}

	public class SpawnCrowdBehaviour : MonoBehaviour {
		public GameObject prefab;
		public int count;
		public CrowdFormationTypes typeFormationCrowd;
		public float intervalBetweenUnits = 1.2f;
		private EntityManager em;
		private EntityArchetype _archetypeCrowd;
		private Entity owner;
		private GameObjectConversionSettings setting;
		private NativeArray<Entity> entities;

		private ICrowdFormationPositionsDistributor positionsDistributor;

		public void Init(Entity realm) {
			owner = realm;
			em = World.DefaultGameObjectInjectionWorld.EntityManager;
			_archetypeCrowd = em.CreateArchetype(new ComponentType[] {
				typeof(Crowd),
				typeof(OwnedByRealm),

				typeof(Moral),
				typeof(MaxMoral),
			});

			setting = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, new BlobAssetStore());

			var crowd = em.CreateEntity(_archetypeCrowd);
			em.SetComponentData(crowd, new OwnedByRealm() { owner = owner });
			em.SetComponentData(crowd, new Moral() { value = 0.8f });
			em.SetComponentData(crowd, new MaxMoral() { value = 1.0f });

			if (!em.HasComponent<PlayerControlled>(owner))
				em.AddComponent<CrowdAi>(crowd);

			var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, setting);

			entities = new NativeArray<Entity>(count, Allocator.Temp);
			em.Instantiate(entity, entities);

			NativeArray<float3> positions;
			switch (typeFormationCrowd) {
			case CrowdFormationTypes.SQUARE:
				positions = new SquareCrowdFormation().Distribute(transform, count, intervalBetweenUnits);
				break;
			case CrowdFormationTypes.TRIANGLE:
				positions = new TriangleCrowdFormation().Distribute(transform, count, intervalBetweenUnits);
				break;
			default:
				positions = new NativeArray<float3>(count, Allocator.Temp);
				break;
			}

			for (int i = 0; i < entities.Length; i++) {
				em.SetName(entities[i], "Unit: " + crowd.Index + "-" + i);
				em.AddComponentData(entities[i], new CrowdMember() {
					crowd = crowd,
					behavingPolicy = CrowdMemberBehavingPolicy.IDLE,
					targetLocation = transform.position,
				});
				em.SetComponentData(entities[i], new OwnedByRealm() { owner = owner });

				em.SetComponentData(entities[i], new Translation() {
					Value = positions[i]
				});
				em.SetComponentData(entities[i], new Rotation() {
					Value = transform.rotation
				});
			}

			entities.Dispose();
		}

		private void OnDestroy() {
			setting.BlobAssetStore.Dispose();
		}
	}
}
