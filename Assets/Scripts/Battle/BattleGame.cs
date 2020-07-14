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
				typeof(Unit),

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
			});

			archetypeUnitAppearance = entities.CreateArchetype(new ComponentType[] {
				typeof(Translation),
				typeof(Parent),
				typeof(LocalToParent),
				typeof(LocalToWorld),
				typeof(RenderBounds),
				typeof(RenderMesh),
			});

			instance = this;
		}
	}
}
