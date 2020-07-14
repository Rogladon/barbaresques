using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct RotationConstraint : IComponentData {
		public bool3 axes;
	}
}

