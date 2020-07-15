using Unity.Entities;

namespace Barbaresques.Battle {
	public interface IEventData : IComponentData {}

	[GenerateAuthoringComponent]
	public struct Event : IComponentData {
	}
}