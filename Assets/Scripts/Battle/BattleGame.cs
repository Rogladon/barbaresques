using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Physics;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;

namespace Barbaresques.Battle {
	public class BattleGame {
		public static BattleGame instance { get; private set; } = null;

		public World world;
		public EntityManager entities => world.EntityManager;

		public EntityArchetype archetypeRealm;
		public EntityArchetype archetypeWarrior;
		public EntityArchetype archetypeWarriorAppearance;

		public Mesh warriorAppearanceMesh;
		public Material warriorAppearanceMaterial;
		public Material warriorAppearanceMaterialA;
		public Material warriorAppearanceMaterialB;

		public void Initialize() {
			Debug.Log("Initializing BattleGame");

			world = World.DefaultGameObjectInjectionWorld;

			archetypeRealm = entities.CreateArchetype(typeof(Realm));

			archetypeWarrior = entities.CreateArchetype(new ComponentType[] {
				typeof(OwnedByRealm),
				typeof(Health),
				typeof(MaxHealth),

				typeof(Translation),
				typeof(Rotation),
				typeof(LocalToWorld),
			});

			archetypeWarriorAppearance = entities.CreateArchetype(new ComponentType[] {
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

			int counts = 368; // просто так

			NativeArray<Entity> warriors = new NativeArray<Entity>(counts, Allocator.Temp);
			entities.CreateEntity(archetypeWarrior, warriors);
			NativeArray<Entity> appearances = new NativeArray<Entity>(counts, Allocator.Temp);
			entities.CreateEntity(archetypeWarriorAppearance, appearances);

			for (int i = 0; i < warriors.Length; i++) {
				entities.SetName(warriors[i], $"Warrior #{i}");
				ConfigureWarrior(warriors[i], (i & 1) == 0 ? realmA : realmB, (i & 1) == 0);

				entities.SetName(appearances[i], $"Warrior appearance #{i}");
				ConfigureWarriorAppearance(appearances[i], warriors[i], (i & 1) == 0 ? warriorAppearanceMaterialA : warriorAppearanceMaterialB);
			}

			appearances.Dispose();
			warriors.Dispose();

			instance = this;
		}

		void ConfigureWarrior(Entity warrior, Entity owner, bool otherSide) {
			entities.SetComponentData(warrior, new OwnedByRealm() { owner = owner });
			entities.SetComponentData(warrior, new Health() { value = 95 });
			entities.SetComponentData(warrior, new MaxHealth() { value = 100 });

			entities.SetComponentData(warrior, new Translation() {
				Value = new float3(
					(otherSide ? 1 : -1) * UnityEngine.Random.Range(1.0f, 32.0f),
					0,
					UnityEngine.Random.Range(-32.0f, 32.0f)),
			});
		}

		void ConfigureWarriorAppearance(Entity appearance, Entity warrior, Material material) {
			entities.SetComponentData(appearance, new Parent() { Value = warrior });
			entities.SetComponentData(appearance, new RenderBounds() { Value = warriorAppearanceMesh.bounds.ToAABB() });
			entities.SetSharedComponentData(appearance, new RenderMesh() {
				mesh = warriorAppearanceMesh,
				material = material,
			});
		}
	}
}
