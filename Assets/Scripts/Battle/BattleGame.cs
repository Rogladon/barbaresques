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

			archetypeUnit = entities.CreateArchetype(new ComponentType[] {
				typeof(OwnedByRealm),

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

			Entity realmA = entities.CreateEntity(archetypeRealm);
			entities.SetName(realmA, "Realm A");
			entities.SetComponentData(realmA, new Realm() { color = Color.green });

			Entity realmB = entities.CreateEntity(archetypeRealm);
			entities.SetName(realmB, "Realm B");
			entities.SetComponentData(realmB, new Realm() { color = Color.yellow });

			int counts = 32; // просто так

			NativeArray<Entity> units = new NativeArray<Entity>(counts, Allocator.Temp);
			entities.CreateEntity(archetypeUnit, units);
			NativeArray<Entity> appearances = new NativeArray<Entity>(counts, Allocator.Temp);
			entities.CreateEntity(archetypeUnitAppearance, appearances);

			BlobAssetReference<Collider> unitCollider = CylinderCollider.Create(new CylinderGeometry() {
				Orientation = quaternion.identity,
				SideCount = 8,
				Radius = 0.5f,
			});

			for (int i = 0; i < units.Length; i++) {
				entities.SetName(units[i], $"Unit #{i}");
				ConfigureUnit(units[i], (i & 1) == 0 ? realmA : realmB, (i & 1) == 0, unitCollider);

				entities.SetName(appearances[i], $"Unit appearance #{i}");
				ConfigureUnitAppearance(appearances[i], units[i], (i & 1) == 0 ? warriorAppearanceMaterialA : warriorAppearanceMaterialB);
			}

			appearances.Dispose();
			units.Dispose();

			instance = this;
		}

		void ConfigureUnit(Entity unit, Entity owner, bool otherSide, BlobAssetReference<Collider> collider) {
			entities.SetComponentData(unit, new OwnedByRealm() { owner = owner });

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
