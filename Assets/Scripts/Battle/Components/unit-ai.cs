using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct UnitAi : IComponentData {
	}

	public enum UnitActionTypes : byte {
		NULL,
		[AssociatedComponent(typeof(UnitFollowCrowdAction))]
		FOLLOW_CROWD,
		[AssociatedComponent(typeof(UnitRetreatAction))]
		RETREAT,
		[AssociatedComponent(typeof(UnitAttackAction))]
		ATTACK,
		[AssociatedComponent(typeof(UnitIdleAction))]
		IDLE,
	}

	public struct UnitAiDecision : ISystemStateComponentData {
		public UnitActionTypes currentAction;
		public bool justChanged;
	}
	public struct UnitJustChangedDecision : IComponentData {}

	public struct UnitFollowCrowdAction : IComponentData {}
	public struct UnitAttackAction : IComponentData {}
	public struct UnitIdleAction : IComponentData {
		public float motionColddown;
	}
	public struct UnitRetreatAction : IComponentData {}

	public struct UnitFollowCrowdScore : IComponentData { public float score; }
	public struct UnitAttackScore : IComponentData { public float score; }
	public struct UnitIdleScore : IComponentData { public float score; }
	public struct UnitRetreatScore : IComponentData { public float score; }
}
