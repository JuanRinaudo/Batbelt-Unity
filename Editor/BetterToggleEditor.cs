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
        
        BetterToggle betterToggle = (BetterToggle)target;
        betterToggle.Label = (TextMeshProUGUI)EditorGUILayout.ObjectField("Label", betterToggle.Label, typeof(TextMeshProUGUI), true);
        betterToggle.Icon = (Image)EditorGUILayout.ObjectField("Icon", betterToggle.Icon, typeof(Image), true);
    }
}
#endif