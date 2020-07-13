using Unity.Entities;
using Unity.Mathematics;

namespace Barbaresques.Battle {
	[GenerateAuthoringComponent]
	public struct SpawnCrowd : IComponentData {
		public Entity crowdMemberPrefab;
		public Entity owner;
		public int count;
	}
}
