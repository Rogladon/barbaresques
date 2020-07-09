using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitSystemGroup))]
	public class HealthSystem : SystemBase {
		protected override void OnUpdate() {
			Entities
				.WithNone<Died>()
				.ForEach((ref Health h, in MaxHealth m) => {
				if (h.value > m.value) {
					h.value = m.value;
				}
			}).ScheduleParallel();
		}
	}
}
