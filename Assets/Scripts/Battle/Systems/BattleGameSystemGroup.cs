using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true), UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
	public class BattleGameSystemGroup : ComponentSystemGroup {
	}
}
