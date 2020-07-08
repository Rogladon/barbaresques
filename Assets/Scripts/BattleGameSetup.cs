using System.Collections;
using System.Collections.Generic;
using Barbaresques.Battle;
using UnityEngine;

namespace Barbaresques {
	public class BattleGameSetup : MonoBehaviour {
		public BattleGame game { get; private set; }

		public Mesh warriorAppearanceMesh;
		public Material warriorAppearanceMaterial;
		public Material warriorAppearanceMaterialA;
		public Material warriorAppearanceMaterialB;

		void Start() {
			game = new BattleGame();
			game.warriorAppearanceMesh = warriorAppearanceMesh;
			game.warriorAppearanceMaterial = warriorAppearanceMaterial;
			game.warriorAppearanceMaterialA = warriorAppearanceMaterialA;
			game.warriorAppearanceMaterialB = warriorAppearanceMaterialB;
			game.Initialize();
		}
	}
}
