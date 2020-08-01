using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using AnimBakery.Cook.Model;
using System;
namespace AnimBakery {
	public struct AnimInitComponent : ISharedComponentData, IEquatable<AnimInitComponent> {
		public List<Clip> clips;
		public BakedData[] bakery;
		public AnimComponent anim;

		public bool Equals(AnimInitComponent other) {
			return bakery == other.bakery;
		}
		public override int GetHashCode() {
			return bakery.GetHashCode();
		}
	}
}
