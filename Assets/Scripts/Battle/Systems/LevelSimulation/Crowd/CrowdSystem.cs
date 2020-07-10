using Unity.Entities;
using Unity.Collections;

namespace Barbaresques.Battle {
	public struct CrowdSystemState : ISystemStateComponentData {
		public int membersCount;
	}

	[UpdateInGroup(typeof(CrowdSystemGroup))]
	public class CrowdSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			Entities.WithName("Crowd_init")
				.WithNone<CrowdSystemState>()
				.WithAll<Crowd>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.AddComponent(entityInQueryIndex, entity, new CrowdSystemState() { membersCount = 0 });
				})
				.ScheduleParallel();

			Entities.WithName("Crowd_deinit")
				.WithAll<CrowdSystemState>()
				.WithNone<Crowd>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.RemoveComponent<CrowdSystemState>(entityInQueryIndex, entity);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
