using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public struct Walking : IComponentData {
		public float3 target;
		public float speedFactor;
		public float targetRadius;
		public float stopAfterSecsInRadius;

		public static readonly float SPEED_FACTOR_ZERO = -1.0f;

		public Walking(float3 target, float speedFactor = 1.0f, float targetRadius = 0.1f, float stopAfterSecsInRadius = 0.0f) {
			this.target = target;
			this.speedFactor = speedFactor;
			this.targetRadius = targetRadius;
			this.stopAfterSecsInRadius = stopAfterSecsInRadius;
		}
	}
}
