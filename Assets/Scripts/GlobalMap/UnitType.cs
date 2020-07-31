using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	[CreateAssetMenu(fileName = "New UnitType", menuName = "Unit Type", order = 51)]
	public class UnitType : ScriptableObject {
		public int maintenancePerTurn;
		public GameObject prefab;
	}
}
