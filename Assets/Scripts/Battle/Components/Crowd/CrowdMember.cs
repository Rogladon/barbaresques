using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	/// <summary>
	/// Политика поведения члена толпы в толпе
	/// </summary>
	[Flags]
	[Serializable]
	public enum CrowdMemberBehavingPolicy : byte {
		/// <summary>
		/// Должен ли следовать к точке цели толпы
		/// </summary>
		FOLLOW = 0b1,
		/// <summary>
		/// Может ли атаковать
		/// </summary>
		ALLOWED_ATTACK = 0b10,

		_NOTHING = 0b0,
		IDLE = _NOTHING | ALLOWED_ATTACK,
		RETREAT = _NOTHING,
	}

	[GenerateAuthoringComponent]
	public struct CrowdMember : IComponentData {
		public Entity crowd;
		public CrowdMemberBehavingPolicy behavingPolicy;
		public float3 targetLocation;
	}
}

