using System;
 
using UnityEngine;
 
// ---------------
//  String => Int
// ---------------
[Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> {}
 
// ---------------
//  GameObject => Float
// ---------------
[Serializable]
public class GameObjectFloatDictionary : SerializableDictionary<GameObject, float> {}

// ---------------
//  int => GameObject
// ---------------
[Serializable]
public class IntGameObjectDictionary : SerializableDictionary<int, GameObject> { }

// ---------------
//  int => Sprite
// ---------------
[Serializable]
public class IntSpriteDictionary : SerializableDictionary<int, Sprite> { }


// ---------------
//  int => Color
// ---------------
[Serializable]
public class IntColorDictionary : SerializableDictionary<int, Color> { }


// ---------------
//  string => SceneReference
// ---------------
[Serializable]
public class StringSceneReferenceDictionary : SerializableDictionary<string, SceneReference> { }


// ---------------
//  int => SceneReference
// ---------------
[Serializable]
public class IntSceneReferenceDictionary : SerializableDictionary<int, SceneReference> { }
 
// ---------------
//  string => GameObject
// ---------------
[Serializable]
public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> {}

// ---------------
//  string => Sprite
// ---------------
[Serializable]
public class StringSpriteDictionary : SerializableDictionary<string, Sprite> { }
