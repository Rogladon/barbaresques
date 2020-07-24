using Unity.Entities;
using Unity.Jobs;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup))]
	[UpdateAfter(typeof(UnitAiActionsScoreSystem))]
	public class UnitAiActionSelectionSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			JobHandle cleanupJustChangedEvent = Entities.WithName(nameof(cleanupJustChangedEvent))
				.WithAll<UnitJustChangedDecision>()
				.ForEach((int entityInQueryIndex, Entity e) => ecb.RemoveComponent<UnitJustChangedDecision>(entityInQueryIndex, e))
				.ScheduleParallel(Dependency);

			JobHandle select = Entities.WithName(nameof(select))
				.WithAll<UnitAi, UnitAiDecision>()
				.ForEach((int entityInQueryIndex, Entity e, ref UnitAiDecision decision, in UnitIdleScore idleScore, in UnitAttackScore attackScore, in UnitRetreatScore retreatScore, in UnitFollowCrowdScore followCrowdScore) => {
					float maxScore = 0.0f;
					UnitActionTypes newAction = UnitActionTypes.IDLE;

					_ChooseMostScored(retreatScore.score, UnitActionTypes.RETREAT, ref maxScore, ref newAction);
					_ChooseMostScored(idleScore.score, UnitActionTypes.IDLE, ref maxScore, ref newAction);
					_ChooseMostScored(attackScore.score, UnitActionTypes.ATTACK, ref maxScore, ref newAction);
					_ChooseMostScored(followCrowdScore.score, UnitActionTypes.FOLLOW_CROWD, ref maxScore, ref newAction);

					if (decision.currentAction != newAction) {
						if (decision.currentAction != UnitActionTypes.NULL) {
							foreach (var aca in AssociatedComponentAttribute.OfEnum(decision.currentAction)) {
								ecb.RemoveComponent(entityInQueryIndex, e, aca.type);
							}
						}
						decision.currentAction = newAction;
						switch (decision.currentAction) {
						default:
							foreach (var aca in AssociatedComponentAttribute.OfEnum(newAction)) {
								ecb.AddComponent(entityInQueryIndex, e, aca.type);
							}
							break;
						}
						decision.justChanged = true;
						ecb.AddComponent<UnitJustChangedDecision>(entityInQueryIndex, e);
					} else {
						decision.justChanged = false;
					}
				}).ScheduleParallel(cleanupJustChangedEvent);

			Dependency = select;

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}

		private static void _ChooseMostScored(float score, UnitActionTypes action, ref float maxScore, ref UnitActionTypes chosenAction) {
			if (score > maxScore) {
				maxScore = score;
				chosenAction = action;
			}
		}
	}
}