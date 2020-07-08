using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Barbaresques.Battle {
	public class BattleHudController : MonoBehaviour {
		[SerializeField]
		private GameObject clickArea;

		void Start() {
			StartCoroutine(TryInitialize());
		}

		void Initialize() {
			var et = clickArea.AddComponent<EventTrigger>();
			var clickEntry = new EventTrigger.Entry();
			clickEntry.eventID = EventTriggerType.PointerUp;
			clickEntry.callback.AddListener((data) => Click());
			et.triggers.Add(clickEntry);
		}

		IEnumerator TryInitialize() {
			if (BattleGame.instance != null) {
				Initialize();
			} else {
				yield return null;
			}
		}

		void Click() {
		}
	}
}
