using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(LevelSimulationSystemGroup)), UpdateBefore(typeof(UnitSystemGroup))]
	public class CrowdSystemGroup : ComponentSystemGroup {
	}
}
