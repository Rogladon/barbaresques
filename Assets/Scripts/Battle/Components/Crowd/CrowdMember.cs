using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct CrowdMember : IComponentData {
		public Entity crowd;
	}
}

