#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using EditorGUILayoutExtras = UnityEditor.AutoCompleteTextField.EditorGUILayout;

[CustomEditor(typeof(LocalizeTMP3DText))]
public class LocalizeTMP3DTextEditor : Editor {

    private string[] textKeyOptions;

    public override void OnInspectorGUI()
    {
        if(textKeyOptions == null)
        {
            textKeyOptions = SimpleTranslations.GetTranslationKeys();
        }

        LocalizeTMP3DText localizeText = (LocalizeTMP3DText)target;

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
        localizeText.textToLocalize = (TextMeshPro)EditorGUILayout.ObjectField("Text", localizeText.textToLocalize, typeof(TextMeshPro), true);
        if(GUILayout.Button("Assign current text"))
        {
            localizeText.textToLocalize = localizeText.GetComponent<TextMeshPro>();
        }
        GUILayout.Space(4);
        GUI.contentColor = GUI.backgroundColor = localizeText.textKey != "" ? Color.white : Color.grey;
        localizeText.textKey = EditorGUILayoutExtras.AutoCompleteTextField("Text key", localizeText.textKey, textKeyOptions, "Text key here");
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