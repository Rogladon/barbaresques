using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Debug = UnityEngine.Debug;

namespace Barbaresques.Battle {
	/// <summary>
	/// Синхронизация состояния толпы с ИИ юнита
	/// </summary>
	[UpdateInGroup(typeof(UnitAiSystemGroup), OrderFirst = true), UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiCrowdSyncSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			// Работа с толпами
			Entities.WithName("sync")
				.WithAll<UnitAi>()
				.WithNone<UnitAiStateGoTo, UnitAiStateSwitch>()
				.ForEach((int entityInQueryIndex, Entity e, in UnitAiState ai, in CrowdMember crowdMember) => {
					switch (crowdMember.behavingPolicy) {
					case CrowdMemberBehavingPolicy.IDLE:
						if (ai.state != UnitAiStates.IDLE) {
							ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() { previousState = ai.state, newState = UnitAiStates.IDLE });
						}
						break;
					case CrowdMemberBehavingPolicy.FOLLOW:
						if (ai.state != UnitAiStates.GO_TO) {
							ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() { previousState = ai.state, newState = UnitAiStates.GO_TO });
						}
						break;
					default:
						break;
					}
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}