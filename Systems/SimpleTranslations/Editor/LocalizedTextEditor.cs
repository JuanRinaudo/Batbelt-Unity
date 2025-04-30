#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using RotaryHeart.Lib.AutoComplete;

[CustomPropertyDrawer(typeof(LocalizedText))]
public class LocalizedTextEditor : PropertyDrawer
{

    private string _key;
    private string[] textKeyOptions;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * (SimpleTranslations.GetText(_key).Split("\n").Length - 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(textKeyOptions == null)
        {
            textKeyOptions = SimpleTranslations.GetTranslationKeys();
        }

        var keyProperty = property.FindPropertyRelative("Key");

        EditorGUI.BeginChangeCheck();

        GUI.contentColor = GUI.backgroundColor = keyProperty.stringValue != "" ? Color.white : Color.grey;
        keyProperty.stringValue = AutoCompleteTextField.EditorGUILayout.AutoCompleteTextField(keyProperty.stringValue, keyProperty.stringValue, textKeyOptions, "Text key here", options: GUILayout.Width(400));
        _key = keyProperty.stringValue;
        EditorGUILayout.LabelField(SimpleTranslations.GetText(keyProperty.stringValue));
        // GUILayout.Space(4);
        // GUI.contentColor = GUI.backgroundColor = localizeText.modifier != TextModifier.NONE ? Color.white : Color.grey;
        // localizeText.modifier = (TextModifier)EditorGUILayout.EnumPopup("Font modifier", localizeText.modifier);
        GUI.backgroundColor = Color.white;
        
        if (EditorGUI.EndChangeCheck())
        {
            // EditorUtility.SetDirty(target);
        }
    }
}
#endif