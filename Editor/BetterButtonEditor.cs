#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RotaryHeart.Lib.AutoComplete;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(BetterButton))]
public class BetterButtonEditor : ButtonEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        
        BetterButton betterButton = (BetterButton)target;
        betterButton.Label = (TextMeshProUGUI)EditorGUILayout.ObjectField("Label", betterButton.Label, typeof(TextMeshProUGUI), true);
        betterButton.TransitionLabelColor = EditorGUILayout.Toggle("Transition Label Color", betterButton.TransitionLabelColor);
        EditorGUILayout.Space(4);
        betterButton.Icon = (Image)EditorGUILayout.ObjectField("Icon", betterButton.Icon, typeof(Image), true);
        betterButton.TransitionIconColor = EditorGUILayout.Toggle("Transition Icon Color", betterButton.TransitionIconColor);
        EditorGUILayout.Space(4);
        betterButton.SelectedHighlight = (Image)EditorGUILayout.ObjectField("Selected Highlight", betterButton.SelectedHighlight, typeof(Image), true);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(betterButton, "BetterButton Change");
            EditorUtility.SetDirty(betterButton);
        }
    }
}
#endif