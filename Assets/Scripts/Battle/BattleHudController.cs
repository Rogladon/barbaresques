﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UI;

namespace Barbaresques.Battle {
	public class BattleHudController : MonoBehaviour {
#region Prefabs & Components
#pragma warning disable 649
		[Header("Prefabs")]
		[SerializeField]
		private GameObject _crowdButtonPrefab;

		[Header("Components")]
		[SerializeField]
		private Transform _crowdsDomain;

		private Dictionary<Entity, GameObject> _crowdsButtons;
#pragma warning restore 649
#endregion

		private Entity currentRealm;

		private static World world => World.DefaultGameObjectInjectionWorld;
		private static EntityManager entityManager => world.EntityManager;

		void Start() {
			_crowdsButtons = new Dictionary<Entity, GameObject>();

			World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventSystem>().AddEventHandler((BattleGameStartedEvent bgse) => {
				Debug.Log("bgse 0");
			});

			StartCoroutine(TryInitialize());
		}

		void Initialize() {
			Debug.Log("Initializing HUD");
			EventSystem eventSystem = BattleGame.instance.world.GetOrCreateSystem<EventSystem>();
			eventSystem.AddEventHandler((BattleGameStartedEvent ev) => {
				Debug.Log("bgse 1");
			});
			eventSystem.AddEventHandler((NewCrowdEvent ev) => {
				Debug.Log("New crowd");

				GameObject go = Instantiate(_crowdButtonPrefab, _crowdsDomain);
				go.name = $"Crowd {ev.crowd}";

				var text = go.GetComponent<Text>();
				text.text = ev.crowd.ToString();
				text.color = entityManager.GetComponentData<Realm>(entityManager.GetComponentData<OwnedByRealm>(ev.crowd).owner).color;

				_crowdsButtons[ev.crowd] = go.gameObject;
			});
			eventSystem.AddEventHandler((CrowdDestroyedEvent ev) => {
				Debug.Log("Crowd destroyed");
				if (_crowdsButtons.TryGetValue(ev.crowd, out GameObject go)) {
					Destroy(go);
					_crowdsButtons.Remove(ev.crowd);
				}
			});
		}

		IEnumerator TryInitialize() {
			while (true) {
				if (BattleGame.instance != null) {
					Initialize();
					break;
				} else {
					yield return null;
				}
			}
		}

		public void Click(Transform clickArea) {
			Debug.Log("Click");
			foreach (var hit in Physics.RaycastAll(Camera.main.ViewportPointToRay(Input.mousePosition))) {
				if (hit.transform == clickArea.transform) {
					Debug.Log(hit.point);
					break;
				}
			}
		}
	}
}
