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
        betterToggle.LabelSyncFullColor = EditorGUILayout.Toggle("Label Sync Full Color", betterToggle.LabelSyncFullColor);
        
        betterToggle.Icon = (Image)EditorGUILayout.ObjectField("Icon", betterToggle.Icon, typeof(Image), true);
        betterToggle.IconSyncFullColor = EditorGUILayout.Toggle("Icon Sync Full Color", betterToggle.IconSyncFullColor);
        
        EditorGUILayout.Space(2);
        betterToggle.Overlay = (Image)EditorGUILayout.ObjectField("Overlay", betterToggle.Overlay, typeof(Image), true);
        betterToggle.OverlayOnColor = EditorGUILayout.ColorField("Overlay On Color", betterToggle.OverlayOnColor);
        betterToggle.OverlayOffColor = EditorGUILayout.ColorField("Overlay Off Color", betterToggle.OverlayOffColor);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(betterToggle, "BetterToggle Change");
            EditorUtility.SetDirty(betterToggle);
        }
    }
}
#endif