using Unity.Burst;
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

		[System.Serializable]
		private struct _CrowdSummary {
			public float3 position;
			public bool retreating;
		}

		private EntityQuery _crowdsQuery;
		private EntityQuery _calculateDistancesJobQuery;

		[System.Serializable]
		public struct DistanceBetweenEntities {
			public Entity a;
			public Entity b;
			public float distance;
		}

		/// <summary>
		/// Подсчёт расстояний между враждебными юнитами
		/// </summary>
		[BurstCompile]
		private struct CalculateHostileDistancesJob : IJobChunk {
			[ReadOnly] public ComponentTypeHandle<Translation> translationTypeHandle;
			[ReadOnly] public ComponentTypeHandle<CrowdMember> crowdMemberTypeHandle;
			[ReadOnly] public EntityTypeHandle entityTypeHandle;

			public NativeList<DistanceBetweenEntities>.ParallelWriter distancesBuffer;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
				var translations = chunk.GetNativeArray(translationTypeHandle);
				var crowdMembers = chunk.GetNativeArray(crowdMemberTypeHandle);

				var entities = chunk.GetNativeArray(entityTypeHandle);

				for (int a = 0; a < chunk.Count; a++) {
					var positionA = translations[a].Value;
					var entityA = entities[a];
					var crowdA = crowdMembers[a].crowd;
					for (int b = 0; b < chunk.Count; b++) {
						if (a == b) continue;
						var crowdB = crowdMembers[a].crowd;
						if (crowdA == crowdB) continue;

						var positionB = translations[b].Value;
						var entityB = entities[b];
						distancesBuffer.AddNoResize(new DistanceBetweenEntities() {
							a = entityA, b = entityB,
							distance = math.length(a - b),
						});
					}
				}
			}
		}

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_randomSystem = World.GetOrCreateSystem<RandomSystem>();

			_calculateDistancesJobQuery = GetEntityQuery(new ComponentType[] {
				ComponentType.ReadOnly<Translation>(),
				ComponentType.ReadOnly<CrowdMember>(),
				ComponentType.ReadOnly<CrowdMemberSystemState>(),
			});
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
			var randoms = _randomSystem.randoms;

			// Сводки по толпам
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

			var distancesBuffer = new NativeList<DistanceBetweenEntities>(
				(int)((math.pow(_calculateDistancesJobQuery.CalculateEntityCount(), 2) - 1) / 2.0f),
				Allocator.TempJob);

			var calculateHostileDistances = new CalculateHostileDistancesJob() {
				translationTypeHandle = GetComponentTypeHandle<Translation>(),
				crowdMemberTypeHandle = GetComponentTypeHandle<CrowdMember>(),
				entityTypeHandle = GetEntityTypeHandle(),
				distancesBuffer = distancesBuffer.AsParallelWriter(),
			}.ScheduleParallel(_calculateDistancesJobQuery, init);

			JobHandle updatePolicy = Entities.WithName(nameof(updatePolicy))
				.WithReadOnly(crowdsSummaries)
				.ForEach((int nativeThreadIndex, ref CrowdMember crowdMember, ref CrowdMemberSystemState crowdMemberSystemState) => {
					if (crowdsSummaries.TryGetValue(crowdMember.crowd, out _CrowdSummary crowd)) {
						if (crowd.retreating) {
							crowdMember.behavingPolicy = CrowdMemberBehavingPolicy.RETREAT;
						} else {
							crowdMember.behavingPolicy |= CrowdMemberBehavingPolicy.ALLOWED_ATTACK;

							bool acquireNewTargetLocation = false;
							if ((crowdMember.behavingPolicy | CrowdMemberBehavingPolicy.FOLLOW) == crowdMember.behavingPolicy) {
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
				.ScheduleParallel(JobHandle.CombineDependencies(calculateHostileDistances, collectCrowdsSummaries));


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
			distancesBuffer.Dispose();
		}
	}
}
