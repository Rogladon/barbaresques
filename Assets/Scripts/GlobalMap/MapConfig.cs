using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	[CreateAssetMenu(fileName = "New GlobalMapConfig", menuName = "Global Map config", order = 51)]
	public class MapConfig : ScriptableObject {
		public Texture2D texture;
		public Texture2D provincesMap;

		public HashSet<(ProvinceId, ProvinceId)> Neighbors() {
			var pixels = provincesMap.GetPixels32();
			HashSet<ProvinceId> provinces = new HashSet<ProvinceId>();
			HashSet<(ProvinceId, ProvinceId)> neighbors = new HashSet<(ProvinceId, ProvinceId)>();

			for (int y = 1; y < provincesMap.height; y++) {
				for (int x = 1; x < provincesMap.width; x++) {
					ProvinceId a = ProvinceId.FromColor(_GetPixelFromPixels(pixels, provincesMap.width, provincesMap.height, x - 1, y - 1));
					if (a == ProvinceId.NULL) continue;
					provinces.Add(a);

					ProvinceId right = ProvinceId.FromColor(_GetPixelFromPixels(pixels, provincesMap.width, provincesMap.height, x, y - 1));
					if (right != ProvinceId.NULL) {
						provinces.Add(right);
						neighbors.Add((a, right));
					}

					ProvinceId bottom = ProvinceId.FromColor(_GetPixelFromPixels(pixels, provincesMap.width, provincesMap.height, x - 1, y));
					if (bottom != ProvinceId.NULL) {
						provinces.Add(bottom);
						neighbors.Add((a, bottom));
					}
				}
			}
			// Debug.Log($"Got {provinces.Count} provinces");
			// foreach (ProvinceId pid in provinces) {
			// 	Debug.Log($"{pid.ToString()} {System.Convert.ToString(pid.GetHashCode(), toBase: 2), 32}");
			// }
			return neighbors;
		}

		private static Color32 _GetPixelFromPixels(Color32[] pixels, int w, int h, int x, int y) {
			return pixels[x + y * w];
		}
	}
}
