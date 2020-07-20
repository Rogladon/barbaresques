using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup)), UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiGoToSystem : SystemBase {
		private static readonly float TARGET_LOCATION_RADIUS = 0.5f;
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			JobHandle updateTarget = Entities.WithName(nameof(updateTarget))
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateGoTo>()
				.ForEach((int entityInQueryIndex, Entity e, ref Walking walking, in CrowdMember crowdMember, in CrowdMemberSystemState crowdMemberSystemState) => {
					if (crowdMemberSystemState.distanceToTarget > TARGET_LOCATION_RADIUS) {
						walking.target = crowdMember.targetLocation;
						walking.speedFactor = 1.0f;
					} else {
						ecb.RemoveComponent<Walking>(entityInQueryIndex, e);
					}
				})
				.ScheduleParallel(Dependency);

			JobHandle setTarget = Entities.WithName(nameof(setTarget))
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateGoTo>()
				.WithNone<Walking>()
				.ForEach((int entityInQueryIndex, Entity e, in CrowdMember crowdMember, in CrowdMemberSystemState crowdMemberSystemState, in Translation translation) => {
					if (crowdMemberSystemState.distanceToTarget > TARGET_LOCATION_RADIUS) {
						ecb.AddComponent(entityInQueryIndex, e, new Walking() {
							target = crowdMember.targetLocation,
							speedFactor = 1,
						});
					}
				})
				.ScheduleParallel(updateTarget);
		
			Dependency = JobHandle.CombineDependencies(setTarget, updateTarget);

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}

