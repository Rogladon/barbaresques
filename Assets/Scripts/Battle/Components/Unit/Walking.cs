using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public struct Walking : IComponentData {
		public float3 target;
		public float speedFactor;

		public static readonly float SPEED_FACTOR_ZERO = -1.0f;

		public Walking(float3 target, float speedFactor = 1.0f) {
			this.target = target;
			this.speedFactor = speedFactor;
		}
	}
}
