using Unity.Mathematics;
using UnityEngine;

namespace Barbaresques {
	public static class Barbaresques {
		public static AABB ToAABB(this Bounds self) => new AABB() {
			Center = self.center,
			Extents = self.extents,
		};
	}
}
