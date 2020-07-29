using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	[System.Serializable]
	public struct ProvinceId : System.IEquatable<ProvinceId> {
		public static readonly ProvinceId NULL = new ProvinceId() { r = 0, g = 0, b = 0 };

		public byte r, g, b;

		public static ProvinceId FromColor(Color c) {
			return new ProvinceId() {
				r = (byte)(c.r * 255),
				g = (byte)(c.g * 255),
				b = (byte)(c.b * 255),
			};
		}

		public static ProvinceId FromColor(Color32 c) {
			return new ProvinceId() {
				r = c.r,
				g = c.g,
				b = c.b,
			};
		}

		public bool Equals(ProvinceId other) => this == other;

		public override bool Equals(object obj) => obj is ProvinceId && this == (ProvinceId)obj;

		public override int GetHashCode() => ((int)r << 16) + ((int)g << 8) + (int)b;

		public override string ToString() => $"province#{r.ToString("X2")}{g.ToString("X2")}{b.ToString("X2")}";

		public static bool operator==(ProvinceId a, ProvinceId b) => a.r == b.r && a.g == b.g && a.b == b.b;
		public static bool operator!=(ProvinceId a, ProvinceId b) => a.r != b.r || a.g != b.g || a.b != b.b;
	}

	public class Province : MonoBehaviour {
		public RealmSocket owner;
		public ProvinceId id;
		public int internalId;

		public List<Province> neighboring = new List<Province>();
	}
}
