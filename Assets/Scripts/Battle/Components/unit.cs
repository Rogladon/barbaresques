using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public struct Health : IComponentData {
		public int value;
	}

	public struct MaxHealth : IComponentData {
		public int value;
	}

	public struct Died : IComponentData {

	}

	public struct Speed : IComponentData {
		public float value;
	}

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
