using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Debug = UnityEngine.Debug;

namespace Barbaresques.Battle {
	/// <summary>
	/// Синхронизация состояния толпы с ИИ юнита
	/// </summary>
	[UpdateInGroup(typeof(UnitAiSystemGroup)),UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiCrowdSyncSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			// Работа с толпами
			Entities.WithName("crowdish_job")
				.WithAll<UnitAi, CrowdMember>()
				.WithNone<UnitAiStateFollowCrowd, UnitAiStateSwitch>()
				.ForEach((int entityInQueryIndex, Entity e, in UnitAiState ai) => {
					ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() { previousState = ai.state, newState = UnitAiStates.FOLLOW_CROWD });
				})
				.ScheduleParallel(); 

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}