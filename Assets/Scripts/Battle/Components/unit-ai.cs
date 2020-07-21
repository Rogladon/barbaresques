using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct UnitAi : IComponentData {
	}

	public enum UnitAiStates {
		[AssociatedComponent(typeof(UnitAiStateIdle))]
		IDLE,
		[AssociatedComponent(typeof(UnitAiStateGoTo))]
		GO_TO,
		[AssociatedComponent(typeof(UnitAiStateRetreat))]
		RETREAT,
	}

	public struct UnitAiState : ISystemStateComponentData {
		public UnitAiStates state;
	}

	public struct UnitAiStateSwitch : ISystemStateComponentData {
		public UnitAiStates previousState;
		public UnitAiStates newState;

		/// <summary>
		/// Просто инициализировать ИИ для юнита?
		/// </summary>
		public bool initialization => previousState == newState;
	}

	public struct UnitAiStateIdle : ISystemStateComponentData {}

	public struct UnitAiStateGoTo : ISystemStateComponentData {}

	public struct UnitAiStateRetreat : ISystemStateComponentData {}
}
