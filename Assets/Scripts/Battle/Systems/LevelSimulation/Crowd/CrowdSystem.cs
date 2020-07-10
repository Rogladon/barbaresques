using Unity.Entities;
using Unity.Collections;

namespace Barbaresques.Battle {
	public struct CrowdSystemState : ISystemStateComponentData {
		public int members;
	}

	[UpdateInGroup(typeof(CrowdSystemGroup))]
	public class CrowdSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		// public NativeList<Entity> crowds { get; private set; }

		// private EntityQuery validCrowdsQuery = default;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			// crowds = new NativeList<Entity>(Allocator.Persistent);
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			Entities.WithName("Crowd_init")
				.WithNone<CrowdSystemState>()
				.WithAll<Crowd>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.AddComponent(entityInQueryIndex, entity, new CrowdSystemState() { members = 0 });
				})
				.ScheduleParallel();

			// Entities.WithAll<Crowd, CrowdSystemState>()
			// 	.WithStoreEntityQueryInField(ref validCrowdsQuery)
			// 	.ForEach((Entity e) => crowds.Add(e))
			// 	.WithoutBurst()
			// 	.Schedule();

			Entities.WithName("Crowd_deinit")
				.WithAll<CrowdSystemState>()
				.WithNone<Crowd>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.RemoveComponent<CrowdSystemState>(entityInQueryIndex, entity);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}

		protected override void OnDestroy() {
			// crowds.Dispose();

			base.OnDestroy();
		}
	}
}
