using Unity.Entities;

namespace Barbaresques.Battle {


	[UpdateInGroup(typeof(UnitAiSystemGroup), OrderFirst = true)]
	public class UnitAiSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			Entities.WithName("init")
				.WithNone<UnitAiDecision>()
				.ForEach((int entityInQueryIndex, Entity e, in UnitAi ai) => {
					ecb.AddComponent<UnitAiDecision>(entityInQueryIndex, e);
					ecb.AddComponent<UnitFollowCrowdScore>(entityInQueryIndex, e);
					ecb.AddComponent<UnitAttackScore>(entityInQueryIndex, e);
					ecb.AddComponent<UnitIdleScore>(entityInQueryIndex, e);
					ecb.AddComponent<UnitRetreatScore>(entityInQueryIndex, e);
				})
				.ScheduleParallel();

			Entities.WithName("deinit")
				.WithNone<UnitAi>()
				.WithAll<UnitAiDecision>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.RemoveComponent<UnitAiDecision>(entityInQueryIndex, e);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
