using Unity.Entities;
using Unity.Transforms;

namespace Barbaresques.Battle {
	public struct UnitAiState : ISystemStateComponentData {

	}

	[UpdateInGroup(typeof(AiSystemGroup))]
	public class UnitAiSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			Entities
				.WithAll<UnitAi>()
				.WithNone<UnitAiState>()
				.ForEach((Entity e, int entityInQueryIndex) => {
					ecb.AddComponent(entityInQueryIndex, e, new UnitAiState() {});
				})
				.ScheduleParallel();

			Entities
				.ForEach((ref Translation t, ref UnitAiState aiState, in UnitAi ai, in OwnedByRealm obr) => {

				})
				.Schedule();

			Entities
				.WithNone<UnitAi>()
				.WithAll<UnitAiState>()
				.ForEach((Entity e, int entityInQueryIndex) => {
					ecb.RemoveComponent<UnitAiState>(entityInQueryIndex, e);
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
