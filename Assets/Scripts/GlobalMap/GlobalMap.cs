using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class GlobalMap : MonoBehaviour {
		public MapConfig mapConfig;

		public int maxInternalProvinceId { get; private set; } = 0;

		private Dictionary<ProvinceId, Province> _provinces;
		public List<Province> provinces { get; private set; }

		void Awake() {
			var neighbors = mapConfig.Neighbors();

			HashSet<ProvinceId> uniqueProvinces = new HashSet<ProvinceId>();
			foreach ((ProvinceId a, ProvinceId b) n in neighbors) {
				uniqueProvinces.Add(n.a);
				uniqueProvinces.Add(n.b);
			}

			Debug.Log("Creating provinces...");

			_provinces = new Dictionary<ProvinceId, Province>();
			provinces = new List<Province>();

			maxInternalProvinceId = 0;

			foreach (var pid in uniqueProvinces) {
				var provGameObject = new GameObject(pid.ToString());
				provGameObject.transform.SetParent(transform);

				var province = provGameObject.AddComponent<Province>();
				province.id = pid;
				province.internalId = maxInternalProvinceId;
				maxInternalProvinceId++;

				_provinces[pid] = province;
				provinces.Add(province);
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
