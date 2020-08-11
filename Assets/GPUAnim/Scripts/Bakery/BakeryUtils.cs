using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimBakery.Cook {
	// тупое говно тупого говна
	// Больше ютилити классов богу хаоса 
	[System.Obsolete]
	public static class BakeryUtils {
		public static int NextPowerOfTwo(this int v) {
			v--;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v++;
			return v;
		}

		public static Mesh Copy(this Mesh originalMesh) {
			var newMesh = new Mesh();
			var vertices = originalMesh.vertices;

			newMesh.vertices = vertices;
			newMesh.triangles = originalMesh.triangles;
			newMesh.normals = originalMesh.normals;
			newMesh.uv = originalMesh.uv;
			newMesh.tangents = originalMesh.tangents;
			newMesh.name = originalMesh.name;
			newMesh.bindposes = originalMesh.bindposes;
			newMesh.bounds = originalMesh.bounds;
			return newMesh;
		}

		public static List<AnimationClip> GetAllAnimationClips(this Animation animation) {
			if (animation == null) return null;

			var animationClips = new List<AnimationClip>();
			foreach (AnimationState state in animation) {
				animationClips.Add(state.clip);
			}
			return animationClips;
		}

		public static Vector2Int To2D(int index, int width) {
			var y = index / width;
			var x = index % width;
			return new Vector2Int(x, y);
		}
	}
}