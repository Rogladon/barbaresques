using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct Health : IComponentData {
		public int value;
	}
}
