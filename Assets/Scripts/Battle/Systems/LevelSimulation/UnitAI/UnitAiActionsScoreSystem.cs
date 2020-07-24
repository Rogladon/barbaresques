using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup))]
	[UpdateAfter(typeof(UnitAiSystem))]
	public class UnitAiActionsScoreSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			Entities.WithAll<UnitAi, UnitAiDecision>()
				.ForEach((ref UnitAttackScore attackScore, in CrowdMember crowdMember, in CrowdMemberSystemState crowdMemberSystemState, in UnitAiDecision aiState) => {
					if (crowdMemberSystemState.prey != Entity.Null && (crowdMember.behavingPolicy | CrowdMemberBehavingPolicy.ALLOWED_ATTACK) == crowdMember.behavingPolicy) {
						if (aiState.currentAction == UnitActionTypes.ATTACK) {
							attackScore.score = 1.0f; // или сделать больше 1?
						} else {
							attackScore.score = ResponseCurve.Exponential(math.clamp(attackScore.score, 0.0f, 1.0f));
						}

						// TODO: отбалансить
						if (crowdMemberSystemState.preyDistance < 2.5f) {
							attackScore.score += 0.25f;
						}
					} else {
						attackScore.score = 0.0f;
					}
				}).ScheduleParallel();

			Entities.WithAll<UnitAi, UnitAiDecision>()
				.ForEach((ref UnitFollowCrowdScore score, in CrowdMember crowdMember, in CrowdMemberSystemState crowdMemberSystemState, in UnitAiDecision aiState) => {
					if ((crowdMember.behavingPolicy | CrowdMemberBehavingPolicy.FOLLOW) == crowdMember.behavingPolicy) {
						score.score = 1.0f;
					} else {
						score.score = 0; // TODO: плавное снижение
					}
				}).ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}