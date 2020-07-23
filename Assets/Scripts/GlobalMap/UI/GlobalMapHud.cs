using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class GlobalMapHud : MonoBehaviour {
		public void GoBattle() {
			SceneLoader.LoadScene(SceneLoader.SCENE_BATTLE);
		}
	}
}