using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Barbaresques.GlobalMap {
	[CustomEditor(typeof(MapConfig))]
	public class MapConfigDrawer : Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			GUILayout.Label("");
			if (GUILayout.Button("Check neighbors")) {
				MapConfig palette = (MapConfig)serializedObject.targetObject;
				var neighbors = palette.Neighbors();
				// TODO:
			}
			GUILayout.Label("");
		}
	}
}
