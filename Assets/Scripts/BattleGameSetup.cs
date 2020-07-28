using System.Collections;
using System.Collections.Generic;
using Barbaresques.Battle;
using UnityEngine;
using Unity.Entities;

namespace Barbaresques {
	public class BattleGameSetup : MonoBehaviour {
		public BattleGame game { get; private set; }

		public EntityManager em;

		[System.Serializable]
		public struct Spawn {
			public SpawnCrowdBehaviour spawnCrowd;
			[SerializeField]
			public Entity entity;
			public int realmId;
			public Color realmColor;
			public bool playerControlled;
		}

		public List<Spawn> spawns = new List<Spawn>();

		void Awake() {
			game = new BattleGame();
			game.Initialize();
			em = game.world.EntityManager;

			Dictionary<int, Entity> createdRealms = new Dictionary<int, Entity>();
			foreach(var realmConfig in spawns) {
				if (!createdRealms.ContainsKey(realmConfig.realmId)) {
					Entity realm = em.CreateEntity(game.archetypeRealm);
					em.SetComponentData(realm, new Realm { color = realmConfig.realmColor });
					if (realmConfig.playerControlled) {
						em.AddComponent<PlayerControlled>(realm);
					}
					createdRealms.Add(realmConfig.realmId, realm);
				}
				realmConfig.spawnCrowd.Init(createdRealms[realmConfig.realmId]);
			}
			
		}
	}
}
