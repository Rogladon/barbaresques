using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class GlobalMapRenderer : MonoBehaviour {
		public GlobalMap _map;

		private float _downscaleFactor = 128.0f;

		[Header("Components")]
		[SerializeField]
		private MeshRenderer _meshRenderer;

		private Material _material;

		private static readonly string FIELD_PROVINCE_COLORS = "_provs";

		void Start() {
			if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();

			transform.localScale = new Vector3(_map.mapConfig.texture.width / _downscaleFactor, _map.mapConfig.texture.height / _downscaleFactor, 1);

			_material = _meshRenderer.material;
			_SetupMaterial();
		}

		void _SetupMaterial() {
			_material.mainTexture = _map.mapConfig.texture;
			_material.SetTexture("_provinces", _map.mapConfig.provincesMap);

			// if (_material.GetInt("_maxProvs") <= _globalMap.maxInternalProvinceId) {
			// 	Debug.LogError($"Shader allows only {_material.GetInt("_maxProvs")} provinces. Got {_globalMap.maxInternalProvinceId}");
			// }

			Vector4[] colors = new Vector4[_map.maxInternalProvinceId + 1];
			foreach (var p in _map.provinces) {
				Debug.Log($"{p.internalId} {colors.Length} {_map.maxInternalProvinceId}");
				colors[p.internalId] = p.id.ToVector4Color();
			}
			_material.SetVectorArray(FIELD_PROVINCE_COLORS, colors);
		}
	}
}