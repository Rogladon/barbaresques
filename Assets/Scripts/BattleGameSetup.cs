using System.Collections;
using System.Collections.Generic;
using Barbaresques.Battle;
using UnityEngine;
using Unity.Entities;

namespace Barbaresques {
	public class BattleGameSetup : MonoBehaviour {
		public BattleGame game { get; private set; }

		public Mesh warriorAppearanceMesh;
		public Material warriorAppearanceMaterial;
		public Material warriorAppearanceMaterialA;
		public Material warriorAppearanceMaterialB;
		public EntityManager em;

		[System.Serializable]
		public struct Spawn {
			public SpawnCrowdBehavier spawnCrowd;
			[SerializeField]
			public Entity entity;
			public int idRealm;
			public Color colorRealm;
		}

		public List<Spawn> spawns = new List<Spawn>();

		void Awake() {
			game = new BattleGame();
			game.warriorAppearanceMesh = warriorAppearanceMesh;
			game.warriorAppearanceMaterial = warriorAppearanceMaterial;
			game.warriorAppearanceMaterialA = warriorAppearanceMaterialA;
			game.warriorAppearanceMaterialB = warriorAppearanceMaterialB;
			game.Initialize();
			em = game.world.EntityManager;

			Dictionary<int, Entity> idCreatedRealms = new Dictionary<int, Entity>();
			foreach(var i in spawns) {
				if (!idCreatedRealms.ContainsKey(i.idRealm)) {
					Entity realm = em.CreateEntity(game.archetypeRealm);
					em.SetComponentData(realm, new Realm { color = i.colorRealm });
					idCreatedRealms.Add(i.idRealm, realm);
				}
				i.spawnCrowd.Init(idCreatedRealms[i.idRealm]);
			}
			
		}
	}
}
