using UnityEngine;

namespace Barbaresques {
	public class MainMenuController : MonoBehaviour {
		public void GoPlay() {
			SceneLoader.LoadScene(SceneLoader.SCENE_GLOBAL_MAP);
		}
	}
}
