using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiActionsSystemGroup))]
	public class UnitAiIdleActionSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		private RandomSystem _randomSystem;

		private static readonly float SPEED_FACTOR = 0.25f;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_randomSystem = World.GetOrCreateSystem<RandomSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
			var randoms = _randomSystem.randoms;

			var delta = Time.DeltaTime;

			Entities
				.WithAll<UnitAi, UnitAiDecision, UnitIdleAction>()
				.WithAll<UnitJustChangedDecision, Walking>()
				.ForEach((int entityInQueryIndex, Entity e) => {
					ecb.RemoveComponent<Walking>(entityInQueryIndex, e);
				}).ScheduleParallel();

			Entities.WithAll<UnitAi, UnitAiDecision, UnitIdleAction>()
				.WithNone<Walking, CrowdMember>()
				.ForEach((int nativeThreadIndex, int entityInQueryIndex, Entity e, ref UnitIdleAction action, in Translation translation) => {
					if (action.motionColddown < 0) {
						// HACK: Всё из-за того, что по какой-то неведомой причине nativeThreadIndex > Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount
						// Хотя должно быть наоборот
						if (nativeThreadIndex >= randoms.Length)
							return;

						var random = randoms[nativeThreadIndex];

						ecb.AddComponent(entityInQueryIndex, e, new Walking() {
							target = translation.Value + random.NextFloat3(new float3(-5.0f, 0, -5.0f), new float3(5.0f, 0, 5.0f)),
							speedFactor = SPEED_FACTOR,
						});

						action.motionColddown = random.NextFloat(3f);

						randoms[nativeThreadIndex] = random; // возвращаем рандомайзер обратно
					} else {
						action.motionColddown -= delta;
					}
				}).ScheduleParallel();

			Entities.WithAll<UnitAi, UnitAiDecision, UnitIdleAction>()
				.WithNone<Walking>()
				.ForEach((int nativeThreadIndex, int entityInQueryIndex, Entity e, ref UnitIdleAction action, in Translation translation, in CrowdMember crowdMember) => {
					if (action.motionColddown < 0) {
						// HACK: Всё из-за того, что по какой-то неведомой причине nativeThreadIndex > Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount
						// Хотя должно быть наоборот
						if (nativeThreadIndex >= randoms.Length)
							return;

						var random = randoms[nativeThreadIndex];

						ecb.AddComponent(entityInQueryIndex, e, new Walking() {
							target = crowdMember.targetLocation + random.NextFloat3(new float3(-10.0f, 0, -10.0f), new float3(10.0f, 0, 10.0f)),
							speedFactor = SPEED_FACTOR,
						});

						action.motionColddown = random.NextFloat(3f);

						randoms[nativeThreadIndex] = random; // возвращаем рандомайзер обратно
					} else {
						action.motionColddown -= delta;
					}
				}).ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}