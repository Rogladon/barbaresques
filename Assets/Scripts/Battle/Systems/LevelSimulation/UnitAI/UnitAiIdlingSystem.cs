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

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
			var randoms = _randomSystem.randoms;

			JobHandle endTask = Entities.WithName(nameof(endTask))
				.WithNone<UnitAiStateSwitch>()
				.WithAll<UnitAiStateIdle>()
				.ForEach((int entityInQueryIndex, Entity e, in Walking w, in Translation translation) => {
					if (math.length(w.target - translation.Value) < 0.2f) {
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
					// HACK:
					if (nativeThreadIndex >= randoms.Length)
						return;

					var random = randoms[nativeThreadIndex];

					ecb.AddComponent(entityInQueryIndex, e, new Walking(translation.Value + random.NextFloat3(new float3(-5.0f, 0, -5.0f), new float3(5.0f, 0, 5.0f))) {
						speedFactor = 0.25f,
					});

					randoms[nativeThreadIndex] = random; // возвращаем рандомайзер обратно
				})
				.ScheduleParallel(endTask);

			Dependency = setTask;

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
