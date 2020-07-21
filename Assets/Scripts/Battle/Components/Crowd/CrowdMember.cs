using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	/// <summary>
	/// Политика поведения члена толпы в толпе
	/// </summary>
	public enum CrowdMemberBehavingPolicy : byte {
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
		RETREAT,
	}

	[GenerateAuthoringComponent]
	public struct CrowdMember : IComponentData {
		public Entity crowd;
		public CrowdMemberBehavingPolicy behavingPolicy;
		public float3 targetLocation;
	}
}

