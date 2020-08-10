using Unity.Entities;

namespace Barbaresques.Battle {
	[ParameterComponent]
	public struct Speed : IComponentData {
		public float value;
	}
}
