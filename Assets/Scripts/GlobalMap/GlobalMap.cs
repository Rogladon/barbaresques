using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class GlobalMap : MonoBehaviour {
		public MapConfig mapConfig;

		private Dictionary<ProvinceId, Province> _provinces;

		void Awake() {
			var neighbors = mapConfig.Neighbors();

			HashSet<ProvinceId> uniqueProvinces = new HashSet<ProvinceId>();
			foreach ((ProvinceId a, ProvinceId b) n in neighbors) {
				uniqueProvinces.Add(n.a);
				uniqueProvinces.Add(n.b);
			}

			Debug.Log("Creating provinces...");

			_provinces = new Dictionary<ProvinceId, Province>();

			int idCounter = 0;

			foreach (var pid in uniqueProvinces) {
				var provGameObject = new GameObject(pid.ToString());
				provGameObject.transform.SetParent(transform);

				var province = provGameObject.AddComponent<Province>();
				province.id = pid;
				province.internalId = ++idCounter;

				_provinces[pid] = province;
			}

			Debug.Log("Connecting provinces...");
			foreach ((ProvinceId a, ProvinceId b) n in neighbors) {
				var provinceA = _provinces[n.a];
				var provinceB = _provinces[n.b];
				provinceA.neighboring.Add(provinceB);
				provinceB.neighboring.Add(provinceA);
			}
		}
	}
}
