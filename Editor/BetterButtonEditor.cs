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
        betterButton.LabelSyncFullColor = EditorGUILayout.Toggle("Label Sync Full Color", betterButton.LabelSyncFullColor);
        
        betterButton.Icon = (Image)EditorGUILayout.ObjectField("Icon", betterButton.Icon, typeof(Image), true);
        betterButton.IconSyncFullColor = EditorGUILayout.Toggle("Icon Sync Full Color", betterButton.IconSyncFullColor);
        
        betterButton.SelectedHighlight = (Image)EditorGUILayout.ObjectField("Selected Highlight", betterButton.SelectedHighlight, typeof(Image), true);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(betterButton, "BetterButton Change");
            EditorUtility.SetDirty(betterButton);
        }
    }
}
#endif