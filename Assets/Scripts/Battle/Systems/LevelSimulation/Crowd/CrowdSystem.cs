using Unity.Entities;
using Unity.Collections;

namespace Barbaresques.Battle {
	public struct CrowdSystemState : ISystemStateComponentData {
		public int membersCount;
	}

	public struct NewCrowdEvent : IComponentData, IEventData {
		public Entity crowd;
	}
	public struct CrowdDestroyedEvent : IComponentData, IEventData {
		public Entity crowd;
	}

	[UpdateInGroup(typeof(CrowdSystemGroup))]
	public class CrowdSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		private EntityArchetype _archetypeNewCrowdEvent;
		private EntityArchetype _archetypeCrowdDestroyedEvent;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			_archetypeNewCrowdEvent = World.EntityManager.CreateArchetype(typeof(Event), typeof(NewCrowdEvent));
			_archetypeCrowdDestroyedEvent = World.EntityManager.CreateArchetype(typeof(Event), typeof(CrowdDestroyedEvent));
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var archetypeNewCrowdEvent = _archetypeNewCrowdEvent;
			var archetypeCrowdDestroyedEvent = _archetypeCrowdDestroyedEvent;

			Entities.WithName("init")
				.WithNone<CrowdSystemState>()
				.WithAll<Crowd>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.AddComponent(entityInQueryIndex, entity, new CrowdSystemState() { membersCount = 0 });
					var ev = ecb.CreateEntity(entityInQueryIndex, archetypeNewCrowdEvent);
					ecb.SetComponent(entityInQueryIndex, ev, new NewCrowdEvent() { crowd = entity });
				})
				.ScheduleParallel();

			Entities.WithName("deinit")
				.WithAll<CrowdSystemState>()
				.WithNone<Crowd>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.RemoveComponent<CrowdSystemState>(entityInQueryIndex, entity);
					var ev = ecb.CreateEntity(entityInQueryIndex, archetypeCrowdDestroyedEvent);
					ecb.SetComponent(entityInQueryIndex, ev, new CrowdDestroyedEvent() { crowd = entity });
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
