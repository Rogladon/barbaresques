using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct MaxMoral : IComponentData {
		public float value;
	}
}
