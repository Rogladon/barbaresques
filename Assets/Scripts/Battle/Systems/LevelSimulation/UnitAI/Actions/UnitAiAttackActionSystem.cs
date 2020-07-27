using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiActionsSystemGroup))]
	public class UnitAiAttackActionSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var delta = Time.DeltaTime;

			JobHandle cleanupWalking = Entities.WithName(nameof(cleanupWalking))
				.WithAll<UnitAi, UnitAiDecision, UnitAttackAction>()
				.WithAll<UnitJustChangedDecision, Walking>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.RemoveComponent<Walking>(entityInQueryIndex, e);
				})
				.ScheduleParallel(Dependency);

			JobHandle setupAttacking = Entities.WithName(nameof(setupAttacking))
				.WithAll<UnitAi, UnitAiDecision, UnitAttackAction>()
				.WithNone<Attacking>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.AddComponent<Attacking>(entityInQueryIndex, e);
				})
				.ScheduleParallel(cleanupWalking);

			JobHandle aim = Entities.WithName(nameof(aim))
				.WithAll<UnitAi, UnitAiDecision, UnitAttackAction>()
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
				})
				.ScheduleParallel(setupAttacking);

			Dependency = aim;

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}