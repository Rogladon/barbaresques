using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitSystemGroup)), UpdateAfter(typeof(HealthSystem))]
	public class DieSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			Entities.ForEach((Entity e, int entityInQueryIndex, in Health h) => {
				if (h.value <= 0) {
					ecb.DestroyEntity(entityInQueryIndex, e);
				}
			}).ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}