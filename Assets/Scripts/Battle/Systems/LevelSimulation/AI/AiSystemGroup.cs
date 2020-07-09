using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(LevelSimulationSystemGroup)), UpdateAfter(typeof(UnitSystemGroup))]
	public class AiSystemGroup : ComponentSystemGroup {
	}
}

