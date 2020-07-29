using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Barbaresques.GlobalMap {
	public class GlobalMapHud : MonoBehaviour {
		public Scheduler scheduler;

		public RealmSocket playerRealm;

		public Text treasuryFieldValue;
		public Transform realmsList;
		public Transform hireMenu;

		public Image realmArmsPrefab;

		void Start() {
			foreach (RealmSocket rs in scheduler.realms) {
				var arms = Instantiate(realmArmsPrefab, realmsList);
				arms.color = rs.realmComponent.color;

				UnityAction setScale = () =>
					arms.transform.localScale = scheduler.current == rs ? Vector3.one * 1.2f : Vector3.one;

				scheduler.onNextRealm.AddListener(setScale);
				setScale();
			}
			Refresh();
			hireMenu.gameObject.SetActive(false);
		}

		public void GoBattle() {
			SceneLoader.LoadScene(SceneLoader.SCENE_BATTLE);
		}

		public void NextTurn() {
			scheduler.NextTurn();
		}

		public void ClickMap() {
			// TODO:
		}

		public void Refresh() {
			treasuryFieldValue.text = playerRealm.money.ToString();
		}
	}
}