using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

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

			Entities.WithName("UnitAi_idle_setTask")
				.WithNativeDisableParallelForRestriction(randoms)
				.WithNone<UnitAiStateSwitched>()
				.WithAll<UnitAiStateIdle>()
				.WithNone<Walking>()
				.ForEach((int nativeThreadIndex, int entityInQueryIndex, Entity e, in Translation translation) => {
					if (nativeThreadIndex >= randoms.Length)
						return;
					var random = randoms[nativeThreadIndex];

					ecb.AddComponent(entityInQueryIndex, e, new Walking(translation.Value + random.NextFloat3(new float3(-5.0f, 0, -5.0f), new float3(5.0f, 0, 5.0f))));

					randoms[nativeThreadIndex] = random; // возвращаем рандомайзер обратно
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
