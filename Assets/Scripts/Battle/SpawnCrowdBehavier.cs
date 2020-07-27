using System.Collections;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

public enum TypeFormationCrowd {
	square,
	triangle
}

namespace Barbaresques.Battle {
	public class SpawnCrowdBehavier : MonoBehaviour {
		public GameObject prefab;
		public int count;
		public TypeFormationCrowd typeFormationCrowd;
		public float distance;
		EntityManager em;
		private EntityArchetype _archetypeCrowd;
		private Entity owner;
		private GameObjectConversionSettings setting;
		private NativeArray<Entity> entites;

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
			em.SetComponentData(crowd, new OwnedByRealm() { owner = owner});
			em.SetComponentData(crowd, new Moral() { value = 0.8f });
			em.SetComponentData(crowd, new MaxMoral() { value = 1.0f });

			var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab,
					setting);

			entites = new NativeArray<Entity>(count, Allocator.Temp);
			em.Instantiate(entity, entites);

			NativeArray<float3> positions = new NativeArray<float3>(count, Allocator.Temp);
			switch (typeFormationCrowd) {
				case TypeFormationCrowd.square:
					positions = SquarePositions();
					break;
				case TypeFormationCrowd.triangle:
					positions = TrianglePositions();
					break;
			}

			for (int i = 0; i < entites.Length; i++) {
				em.SetName(entites[i], "Unit: " + crowd.Index + "-" + i);
				em.AddComponentData(entites[i], new CrowdMember() {
					crowd = crowd,
					behavingPolicy = CrowdMemberBehavingPolicy.IDLE,
					targetLocation = transform.position,
				});
				em.SetComponentData(entites[i], new OwnedByRealm() { owner = owner });

				em.SetComponentData(entites[i], new Translation() {
					Value =positions[i]
				});
				em.SetComponentData(entites[i], new Rotation() {
					Value = quaternion.AxisAngle(new float3(0, 1, 0), transform.rotation.eulerAngles.y)
				});
			}

			entites.Dispose();
		}

		public NativeArray<float3> SquarePositions() {

			NativeArray<float3> na = new NativeArray<float3>(count, Allocator.Temp);
			int width = (int)math.round(math.sqrt(count) + 0.49f);
			int x = 0;
			int y = -(int)math.round(width / 2 - 0.49f);

			for (int i = 0; i < na.Length; i++) {
				na[i] = transform.position + (transform.right * y - transform.forward * x) * distance;
				y++;
				if (y >= math.round(width/2+0.5f)) {
					x++;
					y = -(int)math.round(width/2-0.49f);
				}
			}
			return na;
		}

		public NativeArray<float3> TrianglePositions() {

			NativeArray<float3> na = new NativeArray<float3>(count, Allocator.Temp);
			int width = 1;
			int x = 0;
			int y = 0;

			for (int i = 0; i < na.Length; i++) {
				na[i] = transform.position + (transform.right * y - transform.forward * x) * distance;
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
