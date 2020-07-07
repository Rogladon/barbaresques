using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using Barbaresques;

namespace Barbaresques.Battle {
	public class BattleGame {
		public World world;
		public EntityManager entities => world.EntityManager;

		public EntityArchetype archetypeRealm;
		public EntityArchetype archetypeWarrior;
		public EntityArchetype archetypeWarriorAppearance;

		public Mesh warriorAppearanceMesh;
		public Material warriorAppearanceMaterial;

		public void Initialize() {
			Debug.Log("Initializing BattleGame");

			world = World.DefaultGameObjectInjectionWorld;

			archetypeRealm = entities.CreateArchetype(typeof(Realm));

			archetypeWarrior = entities.CreateArchetype(new ComponentType[] {
				typeof(OwnedByRealm),
				typeof(Health),
				typeof(MaxHealth),

				typeof(Translation),
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

			Entity realmGreen = entities.CreateEntity(archetypeRealm);
			entities.SetName(realmGreen, "Realm green");
			entities.SetComponentData(realmGreen, new Realm() { color = Color.green });

			Entity realmRed = entities.CreateEntity(archetypeRealm);
			entities.SetName(realmRed, "Realm red");
			entities.SetComponentData(realmRed, new Realm() { color = Color.red });

			int counts = 32;

			NativeArray<Entity> warriors = new NativeArray<Entity>(counts, Allocator.Temp);
			entities.CreateEntity(archetypeWarrior, warriors);
			NativeArray<Entity> appearances = new NativeArray<Entity>(counts, Allocator.Temp);
			entities.CreateEntity(archetypeWarriorAppearance, appearances);

			for (int i = 0; i < warriors.Length; i++) {
				entities.SetName(warriors[i], $"Warrior #{i}");
				ConfigureWarrior(warriors[i], (i & 1) == 0 ? realmGreen : realmRed);

				entities.SetName(appearances[i], $"Warrior appearance #{i}");
				ConfigureWarriorAppearance(appearances[i], warriors[i]);
			}

			appearances.Dispose();
			warriors.Dispose();
		}

		void ConfigureWarrior(Entity warrior, Entity owner) {
			entities.SetComponentData(warrior, new OwnedByRealm() { owner = owner });
			entities.SetComponentData(warrior, new Health() { value = 95 });
			entities.SetComponentData(warrior, new MaxHealth() { value = 100 });

			entities.SetComponentData(warrior, new Translation() { Value = new float3(UnityEngine.Random.Range(-20.0f, 20.0f), 0, UnityEngine.Random.Range(-20.0f, 20.0f)) });
		}

		void ConfigureWarriorAppearance(Entity appearance, Entity warrior) {
			entities.SetComponentData(appearance, new Parent() { Value = warrior });
			entities.SetComponentData(appearance, new RenderBounds() { Value = warriorAppearanceMesh.bounds.ToAABB() });
			entities.SetSharedComponentData(appearance, new RenderMesh() {
				mesh = warriorAppearanceMesh,
				material = warriorAppearanceMaterial,
			});
		}
	}
}
