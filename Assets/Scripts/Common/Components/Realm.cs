using UnityEngine;
using Unity.Entities;

namespace Barbaresques {
	[GenerateAuthoringComponent]
	public struct Realm : IComponentData {
		public Color color;
	}
}
