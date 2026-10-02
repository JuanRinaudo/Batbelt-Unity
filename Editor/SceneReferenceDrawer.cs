#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var nameProp = property.FindPropertyRelative("SceneName");
        var scenes = EditorBuildSettings.scenes;
        var sceneNames = new string[scenes.Length];
        int currentIndex = -1;

        for (int i = 0; i < scenes.Length; i++)
        {
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            if (sceneNames[i] == nameProp.stringValue) currentIndex = i;
        }

        EditorGUI.BeginProperty(position, label, property);
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);
        if (newIndex >= 0 && newIndex != currentIndex)
            nameProp.stringValue = sceneNames[newIndex];
        EditorGUI.EndProperty();
    }
}
#endif