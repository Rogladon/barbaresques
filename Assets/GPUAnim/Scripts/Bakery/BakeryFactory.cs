using System;
using UnityEngine;

namespace AnimBakery.Cook {
	public class BakeryFactory {
		private readonly SkinnedMeshRenderer skinnedMeshRenderer;
		private readonly Animation animation;
		private readonly GameObject prototype;

		public BakeryFactory(GameObject p, SkinnedMeshRenderer smr) {
			prototype = p;
			skinnedMeshRenderer = smr;
			if (!p.TryGetComponent<Animation>(out animation)) {
				animation = p.AddComponent<Animation>();
			}

			if (animation == null) {
				throw new ArgumentException("Animation couldn't be null at same time");
			}
		}

		public IBakery Create() {
			if (skinnedMeshRenderer != null && animation != null) {
				return new AnimationBakery(skinnedMeshRenderer, animation);
			}

			throw new SystemException("Not valid state");
		}
	}
}