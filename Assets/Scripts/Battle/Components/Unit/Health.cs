using Unity.Entities;

namespace Barbaresques.Battle {
	[StateComponent]
	public struct Health : IComponentData {
		public int value;
	}
}
