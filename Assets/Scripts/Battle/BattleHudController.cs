using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace Barbaresques.Battle {
	public class BattleHudController : MonoBehaviour {
		void Start() {
			StartCoroutine(TryInitialize());

			World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventSystem>().AddEventHandler((BattleGameStartedEvent bgse) => {
				Debug.Log("bgse 0");
			});
		}

		void Initialize() {
			Debug.Log("Initializing HUD");
			EventSystem eventSystem = BattleGame.instance.world.GetOrCreateSystem<EventSystem>();
			eventSystem.AddEventHandler((BattleGameStartedEvent ev) => {
				Debug.Log("bgse 1");
			});
			eventSystem.AddEventHandler((NewCrowdEvent ev) => {
				Debug.Log("newcrowd");
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
