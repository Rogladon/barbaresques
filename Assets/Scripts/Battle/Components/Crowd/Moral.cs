using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct Moral : IComponentData {
		public float value;
	}
}
