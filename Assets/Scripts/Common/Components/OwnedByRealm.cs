using Unity.Entities;

namespace Barbaresques {
	[GenerateAuthoringComponent]
	public struct OwnedByRealm : IComponentData {
		public Entity owner;
	}
}