using Unity.Entities;

namespace Barbaresques.Battle {
	public struct UnitAi : IComponentData {

	}

	public enum UnitAiStates {
		[AssociatedComponent(typeof(UnitAiStateIdle))]
		IDLE,
		[AssociatedComponent(typeof(UnitAiStateFollowCrowd))]
		FOLLOW_CROWD,
	}

	public struct UnitAiState : ISystemStateComponentData {
		public UnitAiStates state;
	}

	public struct UnitAiStateSwitched : ISystemStateComponentData {
		public UnitAiStates previousState;
		public UnitAiStates newState;

		/// <summary>
		/// Просто инициализировать ИИ для юнита?
		/// </summary>
		public bool initialization => previousState == newState;
	}

	public struct UnitAiStateIdle : ISystemStateComponentData {}

	public struct UnitAiStateFollowCrowd : ISystemStateComponentData {}
}
