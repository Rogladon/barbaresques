using Unity.Entities;

namespace Barbaresques {
	public struct OwnedByRealm : IComponentData {
		public Entity owner;
	}
}