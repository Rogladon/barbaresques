using System;
using UnityEngine;

namespace AnimBakery.Cook {
	public class BakeryFactory {
		private readonly SkinnedMeshRenderer _skinnedMeshRenderer;
		private readonly Animation _animation;
		private readonly GameObject _prototype;

		public BakeryFactory(GameObject p, SkinnedMeshRenderer smr) {
			_prototype = p;
			_skinnedMeshRenderer = smr;
			if (!p.TryGetComponent<Animation>(out _animation)) {
				_animation = p.AddComponent<Animation>();
			}

			if (_animation == null) {
				throw new ArgumentException("Animation couldn't be null at same time");
			}
		}

		public IBakery Create() {
			if (_skinnedMeshRenderer != null && _animation != null) {
				return new AnimationBakery(_skinnedMeshRenderer, _animation);
			}

			throw new SystemException("Not valid state");
		}
	}
}