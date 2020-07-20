using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(CrowdSystemGroup))]
	public class MoralSystem : SystemBase {
		protected override void OnUpdate() {
			Entities
				.ForEach((ref Moral m, in MaxHealth max) => {
					if (m.value > max.value) {
						m.value = max.value;
					}
				})
				.ScheduleParallel();
		}
	}
}
