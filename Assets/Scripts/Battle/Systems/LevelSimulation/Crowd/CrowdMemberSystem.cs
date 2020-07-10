using Unity.Entities;

namespace Barbaresques.Battle {
	public struct CrowdMemberSystemState : ISystemStateComponentData {
	}

	[UpdateInGroup(typeof(CrowdSystemGroup)), UpdateAfter(typeof(CrowdSystem))]
	public class CrowdMemberSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			Entities.WithName("CrowdMember_init")
				.WithNone<CrowdMemberSystemState>()
				.WithAll<CrowdMember>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.AddComponent<CrowdMemberSystemState>(entityInQueryIndex, entity);
				})
				.ScheduleParallel();

			Entities.WithName("CrowdMember_deinit")
				.WithAll<CrowdMemberSystemState>()
				.WithNone<CrowdMember>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.RemoveComponent<CrowdMemberSystemState>(entityInQueryIndex, entity);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}