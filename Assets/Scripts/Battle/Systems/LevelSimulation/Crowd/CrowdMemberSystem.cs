using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	public struct CrowdMemberSystemState : ISystemStateComponentData {
		public float3 lastCrowdsTargetPosition;
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

		private EntityQuery _crowdsQuery;

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
			var randoms = _randomSystem.randoms;

			NativeHashMap<Entity, float3> crowdsTargets = new NativeHashMap<Entity, float3>(_crowdsQuery.CalculateEntityCount(), Allocator.TempJob);
			JobHandle collectCrowdsTargets = Entities.WithName(nameof(collectCrowdsTargets))
				.WithStoreEntityQueryInField(ref _crowdsQuery)
				.WithAll<Crowd>()
				.ForEach((Entity e, in CrowdTargetPosition targetPosition) => {
					crowdsTargets[e] = targetPosition.value;
				})
				.Schedule(Dependency);

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
				.WithReadOnly(crowdsTargets)
				.ForEach((int nativeThreadIndex, ref CrowdMember crowdMember, ref CrowdMemberSystemState crowdMemberSystemState) => {
					if (crowdsTargets.TryGetValue(crowdMember.crowd, out float3 target)) {
						bool acquireNewTargetLocation = false;
						if (crowdMember.behavingPolicy == CrowdMemberBehavingPolicy.FOLLOW) {
							// Если целевая точка изменилась, выдаём её заново
							if (math.length(crowdMemberSystemState.lastCrowdsTargetPosition - target) < 0.01f) {
								crowdMemberSystemState.lastCrowdsTargetPosition = target;
								acquireNewTargetLocation = true;
								// TODO: слать событие?
							}
						} else {
							crowdMember.behavingPolicy = CrowdMemberBehavingPolicy.FOLLOW;

							crowdMemberSystemState.lastCrowdsTargetPosition = target;
							acquireNewTargetLocation = true;
						}

						if (acquireNewTargetLocation) {
							// TODO: сделать нормальная раздачу точек, пока рандомом
							var random = randoms[nativeThreadIndex];
							crowdMember.targetLocation = target + random.NextFloat3(new float3(-5.0f, 0, -5.0f), new float3(5.0f, 0, 5.0f));
							randoms[nativeThreadIndex] = random;
						}
					} else {
						crowdMember.behavingPolicy = CrowdMemberBehavingPolicy.IDLE;
					}
				})
				.ScheduleParallel(collectCrowdsTargets);

			Dependency = JobHandle.CombineDependencies(init, cleanup, updatePolicy);

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

			CompleteDependency();
			crowdsTargets.Dispose();
		}
	}
}
