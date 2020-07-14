using Unity.Entities;
using Unity.Physics;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	public class BattleGameSystemGroup : ComponentSystemGroup {
	}
}
