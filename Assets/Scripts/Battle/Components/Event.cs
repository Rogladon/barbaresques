using Unity.Entities;

namespace Barbaresques.Battle {
	public interface IEvent {}

	[GenerateAuthoringComponent]
	public struct Event : IComponentData {
	}
}