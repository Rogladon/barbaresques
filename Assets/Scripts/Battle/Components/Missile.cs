using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct Missile : IComponentData {
		public float speed;
		public int damage;
	}
}