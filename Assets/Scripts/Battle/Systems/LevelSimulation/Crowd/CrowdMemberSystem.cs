using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(CrowdSystemGroup)), UpdateAfter(typeof(CrowdSystem))]
	public class CrowdMemberSystem : SystemBase {
		protected override void OnUpdate() {
		}
	}
}