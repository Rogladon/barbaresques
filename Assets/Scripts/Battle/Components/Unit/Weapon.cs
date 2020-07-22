using Unity.Entities;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct Weapon : IComponentData {
		public Entity missilePrefab;
		public int shotsPerMinute;
	}
}