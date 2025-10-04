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
        
        BetterButton betterButton = (BetterButton)target;
        betterButton.Label = (TextMeshProUGUI)EditorGUILayout.ObjectField("Label", betterButton.Label, typeof(TextMeshProUGUI), true);
        betterButton.Icon = (Image)EditorGUILayout.ObjectField("Icon", betterButton.Icon, typeof(Image), true);
    }
}
#endif