using UnityEngine;
using UnityEngine.UI;
 
using UnityEditor;

#if UNITY_EDITOR

// ---------------
//  String => Int
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryDrawer : SerializableDictionaryDrawer<string, int> {
    protected override SerializableKeyValueTemplate<string, int> GetTemplate() {
        return GetGenericTemplate<SerializableStringIntTemplate>();
    }
}
internal class SerializableStringIntTemplate : SerializableKeyValueTemplate<string, int> {}
 
// ---------------
//  GameObject => Float
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(GameObjectFloatDictionary))]
public class GameObjectFloatDictionaryDrawer : SerializableDictionaryDrawer<GameObject, float> {
    protected override SerializableKeyValueTemplate<GameObject, float> GetTemplate() {
        return GetGenericTemplate<SerializableGameObjectFloatTemplate>();
    }
}
internal class SerializableGameObjectFloatTemplate : SerializableKeyValueTemplate<GameObject, float> {}

// ---------------
//  Int => GameObject
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(IntGameObjectDictionary))]
public class IntGameObjectDictionaryDrawer : SerializableDictionaryDrawer<int, GameObject> {
	protected override SerializableKeyValueTemplate<int, GameObject> GetTemplate() {
		return GetGenericTemplate<SerializableIntGameObjectTemplate>();
	}
}
internal class SerializableIntGameObjectTemplate : SerializableKeyValueTemplate<int, GameObject> { }

// ---------------
//  int => Sprite 
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(IntSpriteDictionary))]
public class IntSpriteDictionaryDrawer : SerializableDictionaryDrawer<int, Sprite> {
	protected override SerializableKeyValueTemplate<int, Sprite> GetTemplate() {
		return GetGenericTemplate<SerializableIntSpriteTemplate>();
	}
}
internal class SerializableIntSpriteTemplate : SerializableKeyValueTemplate<int, Sprite> { }


// ---------------
//  int => Color
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(IntColorDictionary))]
public class IntColorDictionaryDrawer : SerializableDictionaryDrawer<int, Color> {
	protected override SerializableKeyValueTemplate<int, Color> GetTemplate() {
		return GetGenericTemplate<SerializableIntColorTemplate>();
	}
}
internal class SerializableIntColorTemplate : SerializableKeyValueTemplate<int, Color> { }


// ---------------
//  string => SceneReference
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StringSceneReferenceDictionary))]
public class StringSceneReferenceDictionaryDrawer : SerializableDictionaryDrawer<string, SceneReference> {
	protected override SerializableKeyValueTemplate<string, SceneReference> GetTemplate() {
		return GetGenericTemplate<SerializableStringSceneReferenceTemplate>();
	}
}
internal class SerializableStringSceneReferenceTemplate : SerializableKeyValueTemplate<string, SceneReference> { }


// ---------------
//  int => SceneReference
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(IntSceneReferenceDictionary))]
public class IntSceneReferenceDictionaryDrawer : SerializableDictionaryDrawer<int, SceneReference> {
	protected override SerializableKeyValueTemplate<int, SceneReference> GetTemplate() {
		return GetGenericTemplate<SerializableIntSceneReferenceTemplate>();
	}
}
internal class SerializableIntSceneReferenceTemplate : SerializableKeyValueTemplate<int, SceneReference> { }


// ---------------
//  string => GameObject
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StringGameObjectDictionary))]
public class StringGameObjectDictionaryDrawer : SerializableDictionaryDrawer<string, GameObject> {
	protected override SerializableKeyValueTemplate<string, GameObject> GetTemplate() {
		return GetGenericTemplate<SerializableStringGameObjectTemplate>();
	}
}
internal class SerializableStringGameObjectTemplate : SerializableKeyValueTemplate<string, GameObject> { }


// ---------------
//  string => Sprite 
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StringSpriteDictionary))]
public class StringSpriteDictionaryDrawer : SerializableDictionaryDrawer<string, Sprite> {
	protected override SerializableKeyValueTemplate<string, Sprite> GetTemplate() {
		return GetGenericTemplate<SerializableStringSpriteTemplate>();
	}
}
internal class SerializableStringSpriteTemplate : SerializableKeyValueTemplate<string, Sprite> { }

#endif
