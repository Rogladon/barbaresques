using Unity.Entities;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Debug = UnityEngine.Debug;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitSystemGroup))]
	public class WalkingSystem : SystemBase {
		public static readonly double WALKING_PRECISION = 0.01f;

		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}
		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			var delta = Time.DeltaTime;

			Entities.WithName("walk")
				.ForEach((int entityInQueryIndex, Entity e, ref Translation translation, in Walking walking, in Speed speed) => {
					var diff = walking.target - translation.Value;
					var len = length(diff);
					if (len < WALKING_PRECISION) {
						ecb.RemoveComponent<Walking>(entityInQueryIndex, e);
					} else {
						var currentSpeed = speed.value;
						if (walking.speedFactor == 0) {
							// FIXME: так-то, все эти репорты можно обернуть в статический метод с [BurstDiscard], но тогда для запуска в редакторе придётся запускать без burst компиляции
							// TODO: как-то триггериться на 0
							// Debug.LogError($"{nameof(walking.speedFactor)} of entity {e} was set to 0, which must NOT be used and will be treated as 1. If you want to \"pause\" walking, use {nameof(Walking)}.{nameof(Walking.SPEED_FACTOR_ZERO)} constant");
						} else if (walking.speedFactor < 0) {
							currentSpeed = 0;
						} else if (walking.speedFactor > 1) {
							// TODO: стоит вообще предупреждать, если speedFactor > 1?
							// Debug.LogWarning($"{nameof(walking.speedFactor)} of entity {e} was set to {walking.speedFactor}, which is >= 1, which is weired. If you want to walk speed faster, then set in {nameof(Speed)} use buffs.");
						} else {
							currentSpeed *= walking.speedFactor;
						}
						translation.Value += normalize(diff) * min(len, currentSpeed * delta);
					}
				})
				.ScheduleParallel();

			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}