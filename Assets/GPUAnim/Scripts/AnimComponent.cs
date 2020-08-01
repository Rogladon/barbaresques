using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System;
namespace AnimBakery {
	public struct AnimComponent : ISharedComponentData, IEquatable<AnimComponent> {
		public int id;
		public float frameRate;
		public string animationId;
		public float timeMultiplier;
		public bool addAnimationDifference;
		public bool animated;
		public float normalizedTime;
		public Draw.GPUAnimDrawer drawer;

		public bool Equals(AnimComponent other) {
			return drawer == other.drawer;
		}
		public override int GetHashCode() {
			return drawer.GetHashCode();
		}
	}
}