using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Barbaresques.Battle {
	public class BattleHudController : MonoBehaviour {
		void Start() {
			StartCoroutine(TryInitialize());
		}

		void Initialize() {
			Debug.Log("Initializing HUD");
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
