using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup)), UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiFollowCrowdSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		private EntityQuery _crowdsQuery;

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			NativeHashMap<Entity, float3> crowdsTargets = new NativeHashMap<Entity, float3>(_crowdsQuery.CalculateEntityCount(), Allocator.TempJob);
			JobHandle collectCrowdsTargets = Entities.WithName($"UnitAi_followCrowd_{nameof(collectCrowdsTargets)}")
				.WithStoreEntityQueryInField(ref _crowdsQuery)
				.ForEach((Entity e, in Crowd crowd) => {
					crowdsTargets[e] = crowd.targetLocation;
				})
				.Schedule(Dependency);

			float targetRadius = 10.0f;

			JobHandle setTask = Entities.WithName($"UnitAi_followCrowd_{nameof(setTask)}")
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateFollowCrowd>()
				.WithNone<Walking>()
				.WithReadOnly(crowdsTargets)
				.ForEach((int entityInQueryIndex, Entity e, in CrowdMember crowdMember, in Translation translation) => {
					if (crowdsTargets.TryGetValue(crowdMember.crowd, out float3 target)) {
						if (math.length(translation.Value - target) > targetRadius / 2.0f) {
							ecb.AddComponent(entityInQueryIndex, e, new Walking() {
								target = GetComponent<Crowd>(crowdMember.crowd).targetLocation,
								speedFactor = 1,
								targetRadius = targetRadius,
								stopAfterSecsInRadius = 3.0f,
							});
						}
					}
				})
				.ScheduleParallel(collectCrowdsTargets);

			JobHandle updateTask = Entities.WithName($"UnitAi_followCrowd_{nameof(updateTask)}")
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateFollowCrowd>()
				.WithReadOnly(crowdsTargets)
				.ForEach((int entityInQueryIndex, Entity e, ref Walking walking, in CrowdMember crowdMember) => {
					if (crowdsTargets.TryGetValue(crowdMember.crowd, out float3 target)) {
						walking.target = GetComponent<Crowd>(crowdMember.crowd).targetLocation;
					}
				})
				.ScheduleParallel(collectCrowdsTargets);
			
			Dependency = JobHandle.CombineDependencies(setTask, updateTask);

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

			CompleteDependency();
			crowdsTargets.Dispose();
		}
	}
}

