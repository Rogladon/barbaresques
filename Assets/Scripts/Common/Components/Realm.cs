using System;
using UnityEngine;
using Unity.Entities;

namespace Barbaresques {
	[GenerateAuthoringComponent]
	[Serializable]
	public struct Realm : IComponentData {
		public Color color;
	}
}
