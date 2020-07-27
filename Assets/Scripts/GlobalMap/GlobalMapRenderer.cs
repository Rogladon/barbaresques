using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class GlobalMapRenderer : MonoBehaviour {
		public Texture _map;
		public Texture _provinces;

		private float _downscaleFactor = 128.0f;

		[Header("Components")]
		[SerializeField]
		private MeshRenderer _meshRenderer;

		void Start() {
			if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();

			transform.localScale = new Vector3(_map.width / _downscaleFactor, _map.height / _downscaleFactor, 1);

			_meshRenderer.material.mainTexture = _map;
			_meshRenderer.material.SetTexture("_provinces", _provinces);
		}
	}
}