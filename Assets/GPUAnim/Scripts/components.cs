using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System;
using Unity.Collections;
using AnimBakery.Cook.Model;

namespace AnimBakery {
	public struct AnimationConfig : IComponentData {
		public bool animated;
		public bool addAnimationDifference;
		public FixedString32 animationId;
		public float timeMultiplier;
		public float normalizedTime;
	}

	public struct AnimationData : ISharedComponentData, IEquatable<AnimationData> {
		public BakedMeshData[] baked;

		public bool Equals(AnimationData other) => baked == other.baked;
		public override int GetHashCode() => baked == null ? 0 : baked.GetHashCode();
	}

}