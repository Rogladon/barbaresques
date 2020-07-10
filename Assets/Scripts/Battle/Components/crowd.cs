using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public struct Crowd : IComponentData {
		public float3 targetLocation;
	}

	public struct CrowdMember : IComponentData {
		public Entity crowd;
	}
}
