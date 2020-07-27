using Unity.Entities;

namespace Barbaresques.Battle {
	[UpdateInGroup(typeof(UnitAiSystemGroup)), UpdateAfter(typeof(UnitAiActionSelectionSystem))]
	public class UnitAiActionsSystemGroup : ComponentSystemGroup {
	}
}