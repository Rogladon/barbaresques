using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup))]
	[UpdateAfter(typeof(UnitAiSystem))]
	public class UnitAiActionsScoreSystem : SystemBase {
		// private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			// _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		private EntityQuery _crowdsQuery;

		protected override void OnUpdate() {
			// var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

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
				.ForEach((ref UnitFollowCrowdScore followCrowdScore, in CrowdMember crowdMember, in CrowdMemberSystemState crowdMemberSystemState, in UnitAiDecision aiState, in Translation translation) => {
					if ((crowdMember.behavingPolicy | CrowdMemberBehavingPolicy.FOLLOW) == crowdMember.behavingPolicy) {
						if (math.length(crowdMember.targetLocation - translation.Value) > 10.0f) {
							followCrowdScore.score = 10.0f;
						} else {
							followCrowdScore.score = 1.0f;
						}
					} else {
						followCrowdScore.score = 1.0f - ResponseCurve.Exponential(math.clamp(1.0f - followCrowdScore.score, 0.0f, 1.0f));
					}
				}).ScheduleParallel();

			NativeArray<Entity> retreatedCrowds = new NativeArray<Entity>(_crowdsQuery.CalculateEntityCount(), Allocator.TempJob);

			JobHandle collectRetreatedCrowds = Entities.WithName(nameof(collectRetreatedCrowds))
				.WithStoreEntityQueryInField(ref _crowdsQuery)
				.WithAll<Crowd, Retreating>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					retreatedCrowds[entityInQueryIndex] = e;
				})
				.Schedule(Dependency);

			JobHandle scoreRetreat = Entities.WithName(nameof(scoreRetreat))
				.WithAll<UnitAi, UnitAiDecision>()
				.WithReadOnly(retreatedCrowds)
				.ForEach((ref UnitRetreatScore retreatScore, in CrowdMember crowdMember) => {
					if (retreatedCrowds.Contains(crowdMember.crowd)) {
						retreatScore.score = 10000.0f;
					} else {
						retreatScore.score = 0.0f;
					}
				})
				.ScheduleParallel(collectRetreatedCrowds);

			Dependency = JobHandle.CombineDependencies(Dependency, scoreRetreat);

			// _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

			CompleteDependency();
			retreatedCrowds.Dispose();
		}
	}
}