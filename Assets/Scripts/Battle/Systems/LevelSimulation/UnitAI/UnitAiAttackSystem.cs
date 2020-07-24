using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup)), UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiAttackSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var delta = Time.DeltaTime;

			Entities.WithName("attack")
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateIdle>()
				.WithNone<Attacking>()
				.ForEach((int entityInQueryIndex, Entity e, in CrowdMemberSystemState crowdMemberSystemState) => {
					ecb.AddComponent<Attacking>(entityInQueryIndex, e, new Attacking() { burst = 0 });
				}).ScheduleParallel();

			Entities.WithName("aim")
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateIdle>()
				.WithNone<Walking>()
				.ForEach((int entityInQueryIndex, Entity e, ref Rotation rotation, ref Attacking attacking, in CrowdMemberSystemState crowdMemberSystemState, in Translation translation) => {
					// В холостую не палим
					if (crowdMemberSystemState.prey != Entity.Null) {
						var aimDirection = crowdMemberSystemState.preyPosition - translation.Value;

						// Целимся
						var targetRotation = quaternion.LookRotationSafe(aimDirection, new float3(0, 1, 0));
						rotation.Value = slerp(rotation.Value, targetRotation, delta * 2);

						// Стреляем тока прицелившись
						if (degrees(acos(dot(normalize(forward(rotation.Value)), normalize(aimDirection)))) < 10.0f) {
							attacking.burst = 5;
						}
					}
				}).ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}