using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	public class TransformConstraintSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}
		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var delta = Time.DeltaTime;

			Entities.WithName("rotation")
				.ForEach((ref Rotation rotation, in RotationConstraint constraint) => {
					rotation.Value = new quaternion(
						constraint.axes[0] ? 0 : rotation.Value.value[0],
						constraint.axes[1] ? 0 : rotation.Value.value[1],
						constraint.axes[2] ? 0 : rotation.Value.value[2],
						rotation.Value.value.w
					);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
