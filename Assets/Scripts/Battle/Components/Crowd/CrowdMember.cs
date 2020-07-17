using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	/// <summary>
	/// Политика поведения члена толпы в толпе
	/// </summary>
	public enum CrowdMemberPolicy : byte {
		// TODO: вообще как флаговый enum сделать

		/// <summary>
		/// Занимается своими делами
		/// </summary>
		IDLE = 0,
		/// <summary>
		/// Следует к targetLocation
		/// </summary>
		FOLLOW,
		// TODO: ограничения на определённые действия
	}

	[GenerateAuthoringComponent]
	public struct CrowdMember : IComponentData {
		public Entity crowd;
		public CrowdMemberPolicy policy;
		public float3 targetLocation;
	}
}

