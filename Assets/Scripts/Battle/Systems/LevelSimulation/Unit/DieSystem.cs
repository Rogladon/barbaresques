using Unity.Entities;

namespace Barbaresques.Battle {
	public struct UnitDiedEvent : IComponentData, IEventData {
		public Entity unit;
	}

	[UpdateInGroup(typeof(UnitSystemGroup)), UpdateAfter(typeof(HealthSystem))]
	public class DieSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		private EntityArchetype _archetypeUnitDiedEvent;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			_archetypeUnitDiedEvent = World.EntityManager.CreateArchetype(typeof(Event), typeof(UnitDiedEvent));
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			var archetypeUnitDiedEvent = _archetypeUnitDiedEvent;

			Entities.ForEach((Entity e, int entityInQueryIndex, in Health h) => {
				if (h.value <= 0) {
					ecb.AddComponent<Died>(entityInQueryIndex, e);

					var ev = ecb.CreateEntity(entityInQueryIndex, archetypeUnitDiedEvent);
					ecb.SetComponent(entityInQueryIndex, ev, new UnitDiedEvent() { unit = e });
				}
			}).ScheduleParallel();

			Entities
				.WithAll<Died>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.RemoveComponent<RotationConstraint>(entityInQueryIndex, e);
				}).ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}