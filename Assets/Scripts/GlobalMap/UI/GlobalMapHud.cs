using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class GlobalMapHud : MonoBehaviour {
		public Scheduler scheduler;

		public void GoBattle() {
			SceneLoader.LoadScene(SceneLoader.SCENE_BATTLE);
		}

		public void NextTurn() {
			scheduler.NextTurn();
		}

		public void ClickMap() {
			// TODO:
		}
	}
}