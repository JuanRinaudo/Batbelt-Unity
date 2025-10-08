#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RotaryHeart.Lib.AutoComplete;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(BetterToggle))]
public class BetterToggleEditor : ToggleEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        
        BetterToggle betterToggle = (BetterToggle)target;
        betterToggle.Label = (TextMeshProUGUI)EditorGUILayout.ObjectField("Label", betterToggle.Label, typeof(TextMeshProUGUI), true);
        betterToggle.TransitionLabelColor = EditorGUILayout.Toggle("Transition Label Color", betterToggle.TransitionLabelColor);
        EditorGUILayout.Space(4);
        betterToggle.Icon = (Image)EditorGUILayout.ObjectField("Icon", betterToggle.Icon, typeof(Image), true);
        betterToggle.TransitionIconColor = EditorGUILayout.Toggle("Transition Icon Color", betterToggle.TransitionIconColor);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(betterToggle, "BetterToggle Change");
            EditorUtility.SetDirty(betterToggle);
        }
    }
}
#endif