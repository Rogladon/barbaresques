using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup))]
	[UpdateAfter(typeof(UnitAiActionSelectionSystem))]
	public class UnitAiFollowCrowdActionSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			Entities.WithAll<UnitAi, UnitAiDecision>()
				.ForEach((int entityInQueryIndex, Entity e, ref UnitFollowCrowdAction action, in Walking walking, in CrowdMember crowdMember) => {
					ecb.SetComponent(entityInQueryIndex, e, new Walking() {
						target = crowdMember.targetLocation,
						speedFactor = 1,
					});
				}).ScheduleParallel();

			Entities.WithAll<UnitAi, UnitAiDecision>()
				.WithNone<Walking>()
				.ForEach((int entityInQueryIndex, Entity e, in UnitFollowCrowdAction action, in CrowdMember crowdMember) => {
					ecb.AddComponent(entityInQueryIndex, e, new Walking() {
						target = crowdMember.targetLocation,
						speedFactor = 1,
					});
				}).ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}