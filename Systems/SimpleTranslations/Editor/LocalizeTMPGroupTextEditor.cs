#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using EditorGUILayoutExtras = UnityEditor.AutoCompleteTextField.EditorGUILayout;

[CustomEditor(typeof(LocalizeTMPGroupText))]
public class LocalizeTMPGroupTextEditor : Editor
{

    private string[] textKeyOptions;

    public override void OnInspectorGUI()
    {
        if (textKeyOptions == null)
        {
            textKeyOptions = SimpleTranslations.GetTranslationKeys();
        }

        LocalizeTMPGroupText localizeTextGroup = (LocalizeTMPGroupText)target;

        EditorGUI.BeginChangeCheck();

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 20;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Space(8);
        EditorGUILayout.LabelField("LOCALIZE GROUP TEXT", titleStyle);
        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(EditorStyles.textArea);
        GUILayout.Space(6);
        GUI.backgroundColor = Color.white;

        if (localizeTextGroup.textGroup == null)
        {
            localizeTextGroup.textGroup = new TextMeshProUGUI[0];
        }

        GUI.contentColor = Color.white;
        if (GUILayout.Button("Assign all child texts") && (localizeTextGroup.textGroup.Length == 0 || EditorUtility.DisplayDialog("Replace all texts", "Are you sure you want to replace all texts?", "Yes", "No")))
        {
            localizeTextGroup.textGroup = localizeTextGroup.GetComponentsInChildren<TextMeshProUGUI>(true);
            System.Array.Resize(ref localizeTextGroup.textGroupKeys, localizeTextGroup.textGroup.Length);
            System.Array.Resize(ref localizeTextGroup.textGroupModifiers, localizeTextGroup.textGroup.Length);
            for (int textIndex = 0; textIndex < localizeTextGroup.textGroupKeys.Length; ++textIndex)
            {
                localizeTextGroup.textGroupKeys[textIndex] = "";
            }
        }
        GUILayout.Space(4);

        EditorGUILayout.BeginVertical(EditorStyles.textArea);
        GUILayout.Space(6);
        for (int textIndex = 0; textIndex < localizeTextGroup.textGroupKeys.Length; ++textIndex)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = GUI.backgroundColor = localizeTextGroup.textGroup[textIndex] != null ? Color.white : Color.grey;
            localizeTextGroup.textGroup[textIndex] = (TextMeshProUGUI)EditorGUILayout.ObjectField("Text", localizeTextGroup.textGroup[textIndex], typeof(TextMeshProUGUI), true);
            GUILayout.Space(2);
            GUI.contentColor = GUI.backgroundColor = Color.white;
            if (GUILayout.Button("-", GUILayout.Width(32)))
            {
                localizeTextGroup.textGroup = localizeTextGroup.textGroup.Where((value, index) => index != textIndex).ToArray();
                localizeTextGroup.textGroupKeys = localizeTextGroup.textGroupKeys.Where((value, index) => index != textIndex).ToArray();
                break;
            }
            EditorGUILayout.EndHorizontal();

            GUI.contentColor = GUI.backgroundColor = localizeTextGroup.textGroupKeys[textIndex] != "" ? Color.white : Color.grey;
            localizeTextGroup.textGroupKeys[textIndex] = EditorGUILayoutExtras.AutoCompleteTextField("Text key", localizeTextGroup.textGroupKeys[textIndex], textKeyOptions, "Text key here");
            GUILayout.Space(6);
        }

        GUI.contentColor = GUI.backgroundColor = Color.white;
        if (GUILayout.Button("+", GUILayout.Width(32)))
        {
            System.Array.Resize(ref localizeTextGroup.textGroup, localizeTextGroup.textGroup.Length + 1);
            System.Array.Resize(ref localizeTextGroup.textGroupKeys, localizeTextGroup.textGroupKeys.Length + 1);
            localizeTextGroup.textGroupKeys[localizeTextGroup.textGroupKeys.Length - 1] = "";
        }
        GUILayout.Space(6);

        EditorGUILayout.EndVertical();

        GUILayout.Space(4);

        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(localizeTextGroup);
            EditorSceneManager.MarkSceneDirty(localizeTextGroup.gameObject.scene);
        }
    }

}
#endif