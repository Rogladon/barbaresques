using Unity.Entities;

namespace Barbaresques.Battle {
	public struct BattleGameStartedEvent : IComponentData, IEventData {
		bool dummy;
	}

	[UpdateInGroup(typeof(BattleGameSystemGroup))]
	public class BattleGameSystem : ComponentSystemGroup {
		protected override void OnCreate() {
			base.OnCreate();

			EntityArchetype ea = EntityManager.CreateArchetype(typeof(Event), typeof(BattleGameStartedEvent));
			EntityManager.CreateEntity(ea);
		}

		// TODO:
	}
}