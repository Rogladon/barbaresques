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

			var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab,
					setting);

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
					Value = quaternion.AxisAngle(new float3(0, 1, 0), transform.rotation.eulerAngles.y)
				});
			}

			entities.Dispose();
		}

		private NativeArray<float3> SquarePositions() {
			NativeArray<float3> na = new NativeArray<float3>(count, Allocator.Temp);
			int width = (int)math.round(math.sqrt(count) + 0.49f);
			int x = 0;
			int y = -(int)math.round(width / 2 - 0.49f);

			for (int i = 0; i < na.Length; i++) {
				na[i] = transform.position + (transform.right * y - transform.forward * x) * intervalBetweenUnits;
				y++;
				if (y >= math.round(width / 2 + 0.5f)) {
					x++;
					y = -(int)math.round(width / 2 - 0.49f);
				}
			}
			return na;
		}

		private NativeArray<float3> TrianglePositions() {
			NativeArray<float3> na = new NativeArray<float3>(count, Allocator.Temp);
			int width = 1;
			int x = 0;
			int y = 0;

			for (int i = 0; i < na.Length; i++) {
				na[i] = transform.position + (transform.right * y - transform.forward * x) * intervalBetweenUnits;
				y++;
				if (y > (width - 1) / 2) {
					x++;
					width += 2;
					y = -(width - 1) / 2;

				}
			}
			return na;
		}

		private void OnDestroy() {
			setting.BlobAssetStore.Dispose();
		}
	}
}
