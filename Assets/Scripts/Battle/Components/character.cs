using Unity.Entities;

namespace Barbaresques.Battle {
	public struct Health : IComponentData {
		public int value;
	}

	public struct MaxHealth : IComponentData {
		public int value;
	}

	public struct Died : IComponentData {

	}
}
