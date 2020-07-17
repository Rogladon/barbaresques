using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public struct CrowdTargetPosition : IComponentData {
		public float3 value;
	}
}