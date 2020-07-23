using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Debug = UnityEngine.Debug;
using quaternion = Unity.Mathematics.quaternion;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitSystemGroup))]
	public class WalkingSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var delta = Time.DeltaTime;

			Entities.WithName("walk")
				.ForEach((int entityInQueryIndex, Entity e, ref Translation translation, ref Rotation rotation, in Walking walking, in Speed speed) => {
					var diff = walking.target - translation.Value;
					var len = length(diff);

					if (len > 0.25f) {
						var targetRotation = quaternion.AxisAngle(new float3(0, 1, 0), -atan2(diff.z, diff.x) + atan2(1, 0));
						rotation.Value = slerp(rotation.Value, targetRotation, delta * 2);

						var currentSpeed = min(len, math.clamp(speed.value * walking.speedFactor * delta, 0.0f, 1.0f));

						translation.Value += forward(rotation.Value) * currentSpeed;
					} else {
						ecb.RemoveComponent<Walking>(entityInQueryIndex, e);
					}
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
