using Unity.Entities;
using Unity.Physics;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true), UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
	public class BattleGameSystemGroup : ComponentSystemGroup {
	}
}
