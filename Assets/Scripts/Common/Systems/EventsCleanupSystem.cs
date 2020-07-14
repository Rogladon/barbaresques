using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class EventsCleanupSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}
		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			var delta = Time.DeltaTime;

			Entities.WithName("events_cleanup")
				.WithAll<Event>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.DestroyEntity(entityInQueryIndex, e);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}

