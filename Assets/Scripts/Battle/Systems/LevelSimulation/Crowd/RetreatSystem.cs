using Unity.Entities;

namespace Barbaresques.Battle {
	public struct CrowdRetreatsEvent : IComponentData, IEventData {
		public Entity crowd;
	}

	[UpdateInGroup(typeof(CrowdSystemGroup)), UpdateAfter(typeof(MoralSystem))]
	public class RetreatSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		private EntityArchetype _archetypeCrowdRetreatsEvent;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			_archetypeCrowdRetreatsEvent = World.EntityManager.CreateArchetype(typeof(Event), typeof(CrowdRetreatsEvent));
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var archetypeCrowdRetreatsEvent = _archetypeCrowdRetreatsEvent;

			Entities.ForEach((Entity e, int entityInQueryIndex, in Moral m) => {
				if (m.value <= 0) {
					var ev = ecb.CreateEntity(entityInQueryIndex, archetypeCrowdRetreatsEvent);
					ecb.SetComponent(entityInQueryIndex, ev, new CrowdRetreatsEvent() { crowd = e });
				}
			}).ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}