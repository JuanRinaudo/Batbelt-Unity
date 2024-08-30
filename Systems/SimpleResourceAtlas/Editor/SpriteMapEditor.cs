#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteMap))]
public class SpriteMapEditor : Editor {

	private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
		SpriteMap spriteMap = (SpriteMap)target;

		foreach(string mapKey in spriteMap.mapping.Keys) {
			MappedSprite spriteData = spriteMap.mapping[mapKey];

			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.LabelField(mapKey);
			EditorGUILayout.LabelField(spriteData.path);
			EditorGUILayout.LabelField(spriteData.index.ToString());

			EditorGUILayout.EndHorizontal();
		}
    }

}
#endif