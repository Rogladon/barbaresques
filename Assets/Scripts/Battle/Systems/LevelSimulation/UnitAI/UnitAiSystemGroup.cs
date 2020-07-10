using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(LevelSimulationSystemGroup)), UpdateBefore(typeof(UnitSystemGroup)), UpdateAfter(typeof(CrowdSystemGroup))]
	public class UnitAiSystemGroup : ComponentSystemGroup {
	}
}
