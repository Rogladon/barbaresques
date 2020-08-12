using System;
using Unity.Entities;
using AnimBakery.Cook.Model;

namespace AnimBakery {
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
