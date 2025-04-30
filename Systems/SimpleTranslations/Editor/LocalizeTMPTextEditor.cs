#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RotaryHeart.Lib.AutoComplete;

[CustomEditor(typeof(LocalizeTMPText))]
public class LocalizeTMPTextEditor : Editor {

    private string[] textKeyOptions;

    public override void OnInspectorGUI()
    {
        if(textKeyOptions == null)
        {
            textKeyOptions = SimpleTranslations.GetTranslationKeys();
        }

        LocalizeTMPText localizeText = (LocalizeTMPText)target;

        EditorGUI.BeginChangeCheck();

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 20;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Space(8);
        EditorGUILayout.LabelField("LOCALIZE TEXT", titleStyle);
        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(EditorStyles.textArea);
        GUILayout.Space(6);
        localizeText.textToLocalize = (TextMeshProUGUI)EditorGUILayout.ObjectField("Text", localizeText.textToLocalize, typeof(TextMeshProUGUI), true);
        if(GUILayout.Button("Assign current text"))
        {
            localizeText.textToLocalize = localizeText.GetComponent<TextMeshProUGUI>();
        }
        GUILayout.Space(4);
        GUI.contentColor = GUI.backgroundColor = localizeText.textKey != "" ? Color.white : Color.grey;
        localizeText.textKey = AutoCompleteTextField.EditorGUILayout.AutoCompleteTextField("Text key", localizeText.textKey, textKeyOptions, "Text key here");
        GUILayout.Space(4);
        GUI.contentColor = GUI.backgroundColor = localizeText.modifier != TextModifier.NONE ? Color.white : Color.grey;
        localizeText.modifier = (TextModifier)EditorGUILayout.EnumPopup("Font modifier", localizeText.modifier);
        GUILayout.Space(6);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(localizeText);
            EditorSceneManager.MarkSceneDirty(localizeText.gameObject.scene);
        }
    }

}
#endif