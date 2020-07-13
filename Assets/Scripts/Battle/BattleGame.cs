using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Physics;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;
using Collider = Unity.Physics.Collider;

namespace Barbaresques.Battle {
	public class BattleGame {
		public static BattleGame instance { get; private set; } = null;

		public World world;
		public EntityManager entities => world.EntityManager;

		public EntityArchetype archetypeRealm;
		public EntityArchetype archetypeCrowd;
		public EntityArchetype archetypeUnit;
		public EntityArchetype archetypeUnitAppearance;

		public Mesh warriorAppearanceMesh;
		public Material warriorAppearanceMaterial;
		public Material warriorAppearanceMaterialA;
		public Material warriorAppearanceMaterialB;

		public void Initialize() {
			Debug.Log("Initializing BattleGame");

			world = World.DefaultGameObjectInjectionWorld;

			archetypeRealm = entities.CreateArchetype(typeof(Realm));

			archetypeCrowd = entities.CreateArchetype(new ComponentType[] {
				typeof(Crowd),
				typeof(OwnedByRealm),
			});

			archetypeUnit = entities.CreateArchetype(new ComponentType[] {
				typeof(OwnedByRealm),
				typeof(CrowdMember),

				typeof(UnitAi),

				typeof(Health),
				typeof(MaxHealth),

				typeof(Speed),

				typeof(Translation),
				typeof(Rotation),
				typeof(LocalToWorld),

				typeof(PhysicsCollider),
				// typeof(PhysicsMass),
				// typeof(PhysicsVelocity),
				// typeof(PhysicsDamping),
			});

			archetypeUnitAppearance = entities.CreateArchetype(new ComponentType[] {
				typeof(Translation),
				typeof(Parent),
				typeof(LocalToParent),
				typeof(LocalToWorld),
				typeof(RenderBounds),
				typeof(RenderMesh),
			});

			// //
			// // Realms
			// //
			// NativeArray<Entity> realms = new NativeArray<Entity>(2, Allocator.Temp);
			// entities.CreateEntity(archetypeRealm, realms);

			// for (int i = 0; i < realms.Length; i++) {
			// 	entities.SetName(realms[i], $"Realm {i}");
			// 	entities.SetComponentData(realms[i], new Realm() { color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f)) });
			// }

			// //
			// // Crowds
			// //
			// NativeArray<Entity> crowds = new NativeArray<Entity>(realms.Length, Allocator.Temp);
			// entities.CreateEntity(archetypeCrowd, crowds);

			// for (int i = 0; i < crowds.Length; i++) {
			// 	entities.SetName(crowds[i], $"Crowd #{i}");
			// 	entities.SetComponentData(crowds[i], new Crowd() { targetLocation = new float3(UnityEngine.Random.Range(-16.0f, 16.0f), 0, UnityEngine.Random.Range(-16.0f, 16.0f)) });
			// 	entities.SetComponentData(crowds[i], new OwnedByRealm() { owner = realms[i] });
			// }

			// //
			// // Units
			// //
			// int counts = 32; // просто так

			// NativeArray<Entity> units = new NativeArray<Entity>(counts, Allocator.Temp);
			// entities.CreateEntity(archetypeUnit, units);
			// NativeArray<Entity> unitAppearances = new NativeArray<Entity>(counts, Allocator.Temp);
			// entities.CreateEntity(archetypeUnitAppearance, unitAppearances);

			// BlobAssetReference<Collider> unitCollider = CylinderCollider.Create(new CylinderGeometry() {
			// 	Orientation = quaternion.identity,
			// 	SideCount = 8,
			// 	Radius = 0.5f,
			// });

			// for (int i = 0; i < units.Length; i++) {
			// 	entities.SetName(units[i], $"Unit #{i}");
			// 	ConfigureUnit(units[i], (i & 1) == 0 ? realms[0] : realms[1], (i & 1) == 0, unitCollider, crowds[i % crowds.Length]);

			// 	entities.SetName(unitAppearances[i], $"Unit appearance #{i}");
			// 	ConfigureUnitAppearance(unitAppearances[i], units[i], (i & 1) == 0 ? warriorAppearanceMaterialA : warriorAppearanceMaterialB);
			// }

			// unitAppearances.Dispose();
			// units.Dispose();
			// crowds.Dispose();
			// realms.Dispose();

			instance = this;
		}

		void ConfigureUnit(Entity unit, Entity owner, bool otherSide, BlobAssetReference<Collider> collider, Entity crowd) {
			entities.SetComponentData(unit, new OwnedByRealm() { owner = owner });
			entities.SetComponentData(unit, new CrowdMember() { crowd = crowd });

			entities.SetComponentData(unit, new Health() { value = 95 });
			entities.SetComponentData(unit, new MaxHealth() { value = 100 });
			entities.SetComponentData(unit, new Speed() { value = 3.0f });

			entities.SetComponentData(unit, new Translation() {
				Value = new float3(
					(otherSide ? 1 : -1) * UnityEngine.Random.Range(1.0f, 16.0f),
					0,
					UnityEngine.Random.Range(-16.0f, 16.0f)),
			});

			entities.SetComponentData(unit, new PhysicsCollider() { Value = collider });
		}

		void ConfigureUnitAppearance(Entity appearance, Entity unit, Material material) {
			entities.SetComponentData(appearance, new Parent() { Value = unit });
			entities.SetComponentData(appearance, new Translation() { Value = new float3(0, 1f, 0) });
			entities.SetComponentData(appearance, new RenderBounds() { Value = warriorAppearanceMesh.bounds.ToAABB() });
			entities.SetSharedComponentData(appearance, new RenderMesh() {
				mesh = warriorAppearanceMesh,
				material = material,
			});
		}
	}
}
