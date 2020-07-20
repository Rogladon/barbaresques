using Unity.Entities;

namespace Barbaresques.Battle {
	public struct Attacking : IComponentData {
		public Entity target;
	}
}
