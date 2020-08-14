using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Barbarian.Animations.Cook {
	public interface IBakery {
		BakedMeshData BakeClips(List<Clip> clips, float frameRate = 30f);
	}
}
