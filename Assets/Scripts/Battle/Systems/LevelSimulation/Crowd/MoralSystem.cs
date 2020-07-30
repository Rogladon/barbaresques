using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(CrowdSystemGroup)), UpdateAfter(typeof(CrowdMemberSystem))]
	public class MoralSystem : SystemBase {
		protected override void OnUpdate() {
			Entities
				.ForEach((ref Moral m, in MaxMoral max) => {
					if (m.value > max.value) {
						m.value = max.value;
					}
				})
				.ScheduleParallel();

			Entities
				.ForEach((ref Moral m, in CrowdSystemState systemState) => {
					if (systemState.membersCount < 1) {
						m.value = -10000;
					}
				})
				.ScheduleParallel();
		}
	}
}
