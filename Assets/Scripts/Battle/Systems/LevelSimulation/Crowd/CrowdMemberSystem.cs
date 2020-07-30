using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Barbaresques.Battle {
	public struct CrowdMemberSystemState : ISystemStateComponentData {
		public float3 lastCrowdsTargetPosition;
		public Entity prey;
		public float3 preyPosition;
		public float preyDistance;
		public Entity _crowd;
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
		private EntityQuery _crowdPopulationDiffQuery;
		private EntityQuery _calculateDistancesJobQuery;

		[System.Serializable]
		public struct DistanceBetweenEntities : IComparable<DistanceBetweenEntities> {
			public Entity a;
			public Entity b;
			public float3 positionA;
			public float3 positionB;
			public float distance;

			public int CompareTo(DistanceBetweenEntities other) => distance.CompareTo(other.distance);
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
					var crowdA = crowdMembers[a].crowd;
					var positionA = translations[a].Value;
					var entityA = entities[a];

					for (int b = a; b < chunk.Count; b++) {
						if (a == b) continue;
						var crowdB = crowdMembers[b].crowd;
						if (crowdA == crowdB) continue;

						var positionB = translations[b].Value;
						var entityB = entities[b];
						distancesBuffer.AddNoResize(new DistanceBetweenEntities() {
							a = entityA, b = entityB,
							positionA = positionA,
							positionB = positionB,
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

			_calculateDistancesJobQuery = GetEntityQuery(new EntityQueryDesc() {
				All = new ComponentType[] {
					ComponentType.ReadOnly<Translation>(),
					ComponentType.ReadOnly<CrowdMember>(),
					ComponentType.ReadOnly<CrowdMemberSystemState>(),
				},
				None = new ComponentType[] {
					typeof(Died),
				},
			});
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
			var randoms = _randomSystem.randoms;

			//
			// Сводки по толпам
			//
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

			//
			// Обработка новых и задестроенных
			//
			var syncEcb = _endSimulationEcbSystem.CreateCommandBuffer();

			NativeHashMap<Entity, int> crowdPopulationDiffs = new NativeHashMap<Entity, int>(_crowdPopulationDiffQuery.CalculateEntityCount(), Allocator.TempJob);

			JobHandle init = Entities.WithName(nameof(init))
				.WithNone<CrowdMemberSystemState>()
				.WithAll<CrowdMember>()
				.ForEach((Entity entity, in CrowdMember crowdMember) => {
					syncEcb.AddComponent(entity, new CrowdMemberSystemState() { _crowd = crowdMember.crowd });
					if (crowdPopulationDiffs.ContainsKey(crowdMember.crowd)) {
						crowdPopulationDiffs[crowdMember.crowd] = crowdPopulationDiffs[crowdMember.crowd] + 1;
					} else {
						crowdPopulationDiffs[crowdMember.crowd] = 1;
					}
				})
				.Schedule(Dependency);

			JobHandle cleanup = Entities.WithName(nameof(cleanup))
				.WithAll<CrowdMemberSystemState>()
				.WithNone<CrowdMember>()
				.ForEach((Entity entity, in CrowdMemberSystemState systemState) => {
					if (crowdPopulationDiffs.ContainsKey(systemState._crowd)) {
						crowdPopulationDiffs[systemState._crowd] = crowdPopulationDiffs[systemState._crowd] - 1;
					} else {
						crowdPopulationDiffs[systemState._crowd] = -1;
					}
					syncEcb.RemoveComponent<CrowdMemberSystemState>(entity);
				})
				.Schedule(init);

			// Оно вообще уместно тут?
			JobHandle applyCrowdPopulationsChanges = Entities.WithName(nameof(applyCrowdPopulationsChanges))
				.WithStoreEntityQueryInField(ref _crowdPopulationDiffQuery)
				.WithReadOnly(crowdPopulationDiffs)
				.WithAll<Crowd>()
				.ForEach((Entity crowd, ref CrowdSystemState crowdSystemState) => {
					if (crowdPopulationDiffs.TryGetValue(crowd, out int diff)) {
						crowdSystemState.membersCount += diff;
					}
				})
				.ScheduleParallel(cleanup);

			//
			// Проведение политики толпы в жизнь её членов
			//
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
								if (math.length(crowdMemberSystemState.lastCrowdsTargetPosition - crowd.position) > 0.1f) {
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
				.ScheduleParallel(JobHandle.CombineDependencies(cleanup, collectCrowdsSummaries));

			//
			// Таргетирование
			//

			var distances = new NativeList<DistanceBetweenEntities>(
				(int)((math.pow(_calculateDistancesJobQuery.CalculateEntityCount(), 2) - 1) / 2.0f),
				Allocator.TempJob);

			var calculateHostileDistances = new CalculateHostileDistancesJob() {
				translationTypeHandle = GetComponentTypeHandle<Translation>(),
				crowdMemberTypeHandle = GetComponentTypeHandle<CrowdMember>(),
				entityTypeHandle = GetEntityTypeHandle(),
				distancesBuffer = distances.AsParallelWriter(),
			}.ScheduleParallel(_calculateDistancesJobQuery, updatePolicy);

			var sortDistances = Job.WithCode(() => distances.Sort()).Schedule(calculateHostileDistances);

			JobHandle assignPreys = Entities.WithName(nameof(assignPreys))
				.WithReadOnly(distances)
				.WithAll<CrowdMember>()
				.ForEach((Entity e, ref CrowdMemberSystemState state) => {
					// Находим ближайшего вражеского юнита -- он и жертва
					for (int i = 0; i < distances.Length; i++) {
						var d = distances[i];
						if (d.a == e) {
							state.preyDistance = d.distance;
							state.prey = d.b;
							state.preyPosition = d.positionB;
							break;
						} else if (d.b == e) {
							state.preyDistance = d.distance;
							state.prey = d.a;
							state.preyPosition = d.positionA;
							break;
						}
					}
				}).ScheduleParallel(sortDistances);

			Dependency = JobHandle.CombineDependencies(updatePolicy, assignPreys, applyCrowdPopulationsChanges);

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

			CompleteDependency();
			crowdPopulationDiffs.Dispose();
			crowdsSummaries.Dispose();
			distances.Dispose();
		}
	}
}
