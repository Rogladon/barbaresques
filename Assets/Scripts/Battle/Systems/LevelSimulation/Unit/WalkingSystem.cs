using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Debug = UnityEngine.Debug;

namespace Barbaresques.Battle {
	public struct WalkingSystemState : ISystemStateComponentData {
		public bool achievedLocation;
		public float timeInLocation;
	}

	[UpdateInGroup(typeof(UnitSystemGroup))]
	public class WalkingSystem : SystemBase {
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate() {
			base.OnCreate();

			_endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate() {
			var ecb = _endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

			var delta = Time.DeltaTime;

			Entities.WithName("Walking_init")
				.WithNone<WalkingSystemState>()
				.WithAll<Walking>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.AddComponent(entityInQueryIndex, entity, new WalkingSystemState() { achievedLocation = false });
				})
				.ScheduleParallel();

			Entities.WithName("Walking_walk")
				.ForEach((int entityInQueryIndex, Entity e, ref Translation translation, ref WalkingSystemState walkingSystemState, in Walking walking, in Speed speed) => {
					var diff = walking.target - translation.Value;
					var len = length(diff);

					if (len < walking.targetRadius) {
						if (walkingSystemState.achievedLocation) {
							walkingSystemState.timeInLocation += delta;
						} else {
							walkingSystemState.achievedLocation = true;
							walkingSystemState.timeInLocation = 0.0f;
						}
					} else {
						walkingSystemState.achievedLocation = false;
					}

					if (walkingSystemState.achievedLocation && walkingSystemState.timeInLocation > walking.stopAfterSecsInRadius) {
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

			Entities.WithName("Walking_deinit")
				.WithAll<WalkingSystemState>()
				.WithNone<Walking>()
				.ForEach((int entityInQueryIndex, Entity entity) => {
					ecb.RemoveComponent<WalkingSystemState>(entityInQueryIndex, entity);
				})
				.ScheduleParallel();


			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
