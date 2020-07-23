using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Barbaresques.Battle {
	public struct CrowdMemberSystemState : ISystemStateComponentData {
		public float3 lastCrowdsTargetPosition;
		public float distanceToTarget;
	}

	[UpdateInGroup(typeof(CrowdSystemGroup)), UpdateAfter(typeof(CrowdSystem))]
	public class CrowdMemberSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		private RandomSystem _randomSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_randomSystem = World.GetOrCreateSystem<RandomSystem>();
		}

		[System.Serializable]
		private struct _CrowdSummary {
			public float3 position;
			public bool retreating;
		}

		private EntityQuery _crowdsQuery;

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
			var randoms = _randomSystem.randoms;

			NativeHashMap<Entity, _CrowdSummary> crowdsSummaries = new NativeHashMap<Entity, _CrowdSummary>(_crowdsQuery.CalculateEntityCount(), Allocator.TempJob);
			JobHandle collectActiveCrowds = Entities.WithName(nameof(collectActiveCrowds))
				.WithStoreEntityQueryInField(ref _crowdsQuery)
				.WithAll<Crowd>()
				.WithNone<Retreating>()
				.ForEach((Entity e, in CrowdTargetPosition targetPosition) => {
					crowdsSummaries[e] = new _CrowdSummary() { position = targetPosition.value, retreating = false };
				})
				.Schedule(Dependency);
			JobHandle collectRetreatedCrowds = Entities.WithName(nameof(collectRetreatedCrowds))
				.WithStoreEntityQueryInField(ref _crowdsQuery)
				.WithAll<Crowd, Retreating>()
				.ForEach((Entity e, in CrowdTargetPosition targetPosition) => {
					crowdsSummaries[e] = new _CrowdSummary() { position = targetPosition.value, retreating = true };
				})
				.Schedule(collectActiveCrowds);

			JobHandle collectCrowdsSummaries = collectRetreatedCrowds;

			JobHandle init = Entities.WithName(nameof(init))
				.WithNone<CrowdMemberSystemState>()
				.WithAll<CrowdMember>()
				.ForEach((int entityInQueryIndex, Entity entity) => ecb.AddComponent<CrowdMemberSystemState>(entityInQueryIndex, entity))
				.ScheduleParallel(Dependency);

			JobHandle cleanup = Entities.WithName(nameof(cleanup))
				.WithAll<CrowdMemberSystemState>()
				.WithNone<CrowdMember>()
				.ForEach((int entityInQueryIndex, Entity entity) => ecb.RemoveComponent<CrowdMemberSystemState>(entityInQueryIndex, entity))
				.ScheduleParallel(init);

			JobHandle updatePolicy = Entities.WithName(nameof(updatePolicy))
				.WithReadOnly(crowdsSummaries)
				.ForEach((int nativeThreadIndex, ref CrowdMember crowdMember, ref CrowdMemberSystemState crowdMemberSystemState) => {
					if (crowdsSummaries.TryGetValue(crowdMember.crowd, out _CrowdSummary crowd)) {
						if (crowd.retreating) {
							crowdMember.behavingPolicy = CrowdMemberBehavingPolicy.RETREAT;
						} else {
							crowdMember.behavingPolicy |= CrowdMemberBehavingPolicy.ALLOWED_ATTACK;

							bool acquireNewTargetLocation = false;
							if ((crowdMember.behavingPolicy ^ CrowdMemberBehavingPolicy.FOLLOW) == CrowdMemberBehavingPolicy.FOLLOW) {
								// Если целевая точка изменилась, выдаём её заново
								if (math.length(crowdMemberSystemState.lastCrowdsTargetPosition - crowd.position) < 0.01f) {
									crowdMemberSystemState.lastCrowdsTargetPosition = crowd.position;
									acquireNewTargetLocation = true;
									// TODO: слать событие?
								}
							} else {
								crowdMember.behavingPolicy |= CrowdMemberBehavingPolicy.FOLLOW;

								crowdMemberSystemState.lastCrowdsTargetPosition = crowd.position;
								acquireNewTargetLocation = true;
							}

							if (acquireNewTargetLocation) {
								// TODO: сделать нормальная раздачу точек, пока рандомом
								var random = randoms[nativeThreadIndex];
								crowdMember.targetLocation = crowd.position + random.NextFloat3(new float3(-5.0f, 0, -5.0f), new float3(5.0f, 0, 5.0f));
								randoms[nativeThreadIndex] = random;
							}
						}
					} else {
						crowdMember.behavingPolicy = CrowdMemberBehavingPolicy.IDLE;
					}
				})
				.ScheduleParallel(collectCrowdsSummaries);


			Dependency = JobHandle.CombineDependencies(init, cleanup, updatePolicy);

			// CLEANCODE: а уместно ли тут эта джобса?
			JobHandle calculateDistanceToTarget = Entities.WithName(nameof(calculateDistanceToTarget))
				.ForEach((ref CrowdMemberSystemState c, in CrowdMember crowdMember, in Translation translation) => {
					c.distanceToTarget = math.length(crowdMember.targetLocation - translation.Value);
				}).ScheduleParallel(Dependency);

			Dependency = calculateDistanceToTarget;

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

			CompleteDependency();
			crowdsSummaries.Dispose();
		}
	}
}
