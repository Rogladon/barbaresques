using System.Collections;
using System.Collections.Generic;
using Barbaresques.Battle;
using UnityEngine;

namespace Barbaresques {
	public class BattleGameSetup : MonoBehaviour {
		public BattleGame game { get; private set; }

		public Mesh warriorAppearanceMesh;
		public Material warriorAppearanceMaterial;

		void Start() {
			game = new BattleGame();
			game.warriorAppearanceMesh = warriorAppearanceMesh;
			game.warriorAppearanceMaterial = warriorAppearanceMaterial;
			game.Initialize();
		}
	}
}
