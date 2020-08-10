using Unity.Entities;

namespace Barbaresques.Battle {
	[StateComponent]
	public struct Attacking : IComponentData {
		public int burst;
	}
}
