using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public class BattleHud : MonoBehaviour {
		#region Prefabs & Components
#pragma warning disable 649
		[Header("Prefabs")]
		[SerializeField]
		private GameObject _crowdButtonPrefab;

		[Header("Components")]
		[SerializeField]
		private Transform _crowdsDomain;
		[SerializeField]
		private CrowdBar _crowdBar;
#pragma warning restore 649
		#endregion

		private Dictionary<Entity, Button> _crowdsButtons;

		private Entity currentRealm = Entity.Null;
		private Entity currentCrowd = Entity.Null;

		private static World World => World.DefaultGameObjectInjectionWorld;
		private static EntityManager entityManager => World.EntityManager;

		void Start() {
			_crowdsButtons = new Dictionary<Entity, Button>();

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
				Debug.Log("+crowd");

				GameObject go = Instantiate(_crowdButtonPrefab, _crowdsDomain);
				go.name = $"Crowd {ev.crowd}";

				var text = go.GetComponent<Text>();
				text.text = ev.crowd.ToString();
				text.color = entityManager.GetComponentData<Realm>(entityManager.GetComponentData<OwnedByRealm>(ev.crowd).owner).color;

				var btn = go.GetComponent<Button>();
				btn.onClick.AddListener(() => {
					SelectCrowd(ev.crowd);
					foreach (Button otherBtn in _crowdsButtons.Values) {
						otherBtn.interactable = true;
					}
					btn.interactable = false;
				});

				_crowdsButtons[ev.crowd] = btn;
			});
			eventSystem.AddEventHandler((CrowdDestroyedEvent ev) => {
				Debug.Log("-crowd");
				if (_crowdsButtons.TryGetValue(ev.crowd, out Button btn)) {
					Destroy(btn.gameObject);
					_crowdsButtons.Remove(ev.crowd);
				}
			});

			EntityQuery eq = entityManager.CreateEntityQuery(new EntityQueryDesc() {
				All = new ComponentType[] { typeof(Realm), typeof(PlayerControlled) }
			});
			if (eq.CalculateEntityCount() == 1) {
				currentRealm = eq.GetSingletonEntity();
				// Debug.Log($"Current realm is {currentRealm} \"{entityManager.GetName(currentRealm)}\"");
			} else {
				Debug.LogError($"Expected exactly one entity with {nameof(Realm)} and {nameof(PlayerControlled)} components, but got {eq.CalculateEntityCount()}!");
			}
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

		void Update() {
			_crowdBar.gameObject.SetActive(currentCrowd != Entity.Null);
		}

		private void OnLevelPointSelected(float3 point) {
			if (currentCrowd != Entity.Null) {
				entityManager.AddComponentData(currentCrowd, new CrowdTargetPosition() { value = point });
			}
		}

		public void Click(Transform clickArea) {
			// TODO: приделать InputHelper и чекать одинарный/двойной клик и зум
			foreach (var hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 1000.0f)) {
				if (hit.transform == clickArea.transform) {
					// Debug.Log($"Clicked at {hit.point}");
					OnLevelPointSelected(hit.point);
					break;
				}
			}
		}

		public void SelectCrowd(Entity entity) {
			// TODO: чекать есть ли Crowd
			currentCrowd = entity;
			_crowdBar.crowdEntity = currentCrowd;
		}
	}
}
