using Unity.Entities;

namespace Barbaresques.Battle {
	[ParameterComponent]
	public struct MaxHealth : IComponentData {
		public int value;
	}
}
