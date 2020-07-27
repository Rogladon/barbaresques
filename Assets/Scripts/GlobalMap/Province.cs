using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barbaresques.GlobalMap {
	public class Province : MonoBehaviour {
		public RealmSocket owner;

		public List<Province> neighboring = new List<Province>();
	}
}
