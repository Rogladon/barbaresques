using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct Speed : IComponentData {
		public float value;
	}
}
