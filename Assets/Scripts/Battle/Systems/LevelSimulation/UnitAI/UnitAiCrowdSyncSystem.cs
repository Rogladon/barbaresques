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
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			// Работа с толпами
			Entities.WithName("sync")
				.WithAll<UnitAi>()
				.WithNone<UnitAiStateGoTo, UnitAiStateSwitch>()
				.ForEach((int entityInQueryIndex, Entity e, in UnitAiState ai, in CrowdMember crowdMember, in CrowdMemberSystemState crowdMemberSystemState) => {
					if ((crowdMember.behavingPolicy | CrowdMemberBehavingPolicy.FOLLOW) == crowdMember.behavingPolicy) {
						if (ai.state != UnitAiStates.GO_TO) {
							ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() { previousState = ai.state, newState = UnitAiStates.GO_TO });
						}
					} else if ((crowdMember.behavingPolicy | CrowdMemberBehavingPolicy.ALLOWED_ATTACK) == crowdMember.behavingPolicy
						&& crowdMemberSystemState.prey != Entity.Null
						&& crowdMemberSystemState.preyDistance < 5.0f) {
						if (ai.state != UnitAiStates.ATTACK) {
							ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() { previousState = ai.state, newState = UnitAiStates.ATTACK });
						}
					} else if (crowdMember.behavingPolicy == CrowdMemberBehavingPolicy.IDLE) {
						if (ai.state != UnitAiStates.IDLE) {
							ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() { previousState = ai.state, newState = UnitAiStates.IDLE });
						}
					} else if (crowdMember.behavingPolicy == CrowdMemberBehavingPolicy.RETREAT) {
						if (ai.state != UnitAiStates.RETREAT) {
							ecb.AddComponent(entityInQueryIndex, e, new UnitAiStateSwitch() { previousState = ai.state, newState = UnitAiStates.RETREAT });
						}
					}
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}