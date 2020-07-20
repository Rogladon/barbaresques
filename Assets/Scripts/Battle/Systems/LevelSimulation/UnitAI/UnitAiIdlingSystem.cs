using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup)), UpdateAfter(typeof(UnitAiManagementSystem))]
	public class UnitAiIdlingSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
		private RandomSystem _randomSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			_randomSystem = World.GetOrCreateSystem<RandomSystem>();
		}

		static readonly float MAX_IDLING_RADIUS = 5.0f;
		static readonly float IDLING_SPEED_FACTOR = 0.25f;

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
			var randoms = _randomSystem.randoms;

			JobHandle fixTask = Entities.WithName(nameof(fixTask))
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateIdle>()
				.ForEach((int entityInQueryIndex, Entity e, ref Walking w, in Translation translation) => {
					w.speedFactor = IDLING_SPEED_FACTOR;

					var len = math.length(w.target - translation.Value);
					if (len < 0.2f || len > MAX_IDLING_RADIUS) {
						ecb.RemoveComponent<Walking>(entityInQueryIndex, e);
					}
				})
				.ScheduleParallel(Dependency);

			JobHandle setTask = Entities.WithName(nameof(setTask))
				.WithNativeDisableParallelForRestriction(randoms)
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateIdle>()
				.WithNone<Walking>()
				.ForEach((int nativeThreadIndex, int entityInQueryIndex, Entity e, in Translation translation) => {
					// HACK: Всё из-за того, что по какой-то неведомой причине nativeThreadIndex > Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount
					// Хотя должно быть наоборот
					if (nativeThreadIndex >= randoms.Length)
						return;

					var random = randoms[nativeThreadIndex];

					ecb.AddComponent(entityInQueryIndex, e, new Walking() {
						target = translation.Value + random.NextFloat3(new float3(-5.0f, 0, -5.0f), new float3(5.0f, 0, 5.0f)),
						speedFactor = IDLING_SPEED_FACTOR,
					});

					randoms[nativeThreadIndex] = random; // возвращаем рандомайзер обратно
				})
				.ScheduleParallel(fixTask);

			Dependency = setTask;

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
