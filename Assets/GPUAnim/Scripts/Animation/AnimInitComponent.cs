using System;
using Barbarian.Animations.Cook;
using Unity.Entities;

namespace Barbarian.Animations {
	public struct AnimInitComponent : ISharedComponentData, IEquatable<AnimInitComponent> {
		public BakedMeshData[] bakery;

		public bool Equals(AnimInitComponent other) {
			return bakery == other.bakery;
		}

		public override int GetHashCode() {
			return bakery == null ? 0 : bakery.GetHashCode();
		}
	}
}
