using AnimBakery.Cook.Model;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnimBakery.Cook {
	public interface IBakery {
		BakedData BakeClips(List<Clip> clips, float frameRate = 30f);
	}
}
