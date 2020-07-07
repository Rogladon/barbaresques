using UnityEngine;
using Unity.Entities;

namespace Barbaresques {
	public struct Realm : IComponentData {
		public Color color;
	}

	public struct OwnedByRealm : IComponentData {
		public Entity owner;
	}
}