using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class CrowdSocket : MonoBehaviour, IMoneyAgent {
		public int unitsCount;
		public UnitType unitType;

		public int MoneyAgent() {
			return -1 * unitType.maintenancePerTurn * unitsCount;
		}
	}
}
