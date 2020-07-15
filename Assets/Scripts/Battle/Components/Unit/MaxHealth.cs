using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct MaxHealth : IComponentData {
		public int value;
	}
}
