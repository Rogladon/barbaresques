using Barbarian.Animations;
using Unity.Collections;
using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitSystemGroup), OrderLast = true)]
	public class UnitAnimationSwitchingSystem : SystemBase {
		private static readonly FixedString32 animationWalking = "walk";
		private static readonly FixedString32 animationIdle = "idle";
		private static readonly FixedString32 animationFire = "shoot";

		protected override void OnUpdate() {
			Entities
				.WithAll<Unit, Walking>()
				.ForEach((ref AnimationConfig ac, in Walking walking) => { ac.animationId = animationWalking; ac.timeMultiplier = walking.speedFactor; })
				.ScheduleParallel();

			Entities
				.WithAll<Unit>()
				.WithNone<Walking>()
				.ForEach((ref AnimationConfig ac) => { ac.animationId = animationIdle; ac.timeMultiplier = 1; })
				.ScheduleParallel();

			Entities
				.WithAll<Unit, Attacking>()
				.ForEach((ref AnimationConfig ac) => { ac.animationId = animationFire; ac.timeMultiplier = 1; })
				.ScheduleParallel();
		}
	}
}