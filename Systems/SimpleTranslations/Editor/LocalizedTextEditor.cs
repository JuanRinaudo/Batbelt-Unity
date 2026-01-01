#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using RotaryHeart.Lib.AutoComplete;
using UnityEngine.UIElements;
using DisplayStyle = UnityEngine.UIElements.DisplayStyle;
using KeyCode = UnityEngine.KeyCode;
using KeyDownEvent = UnityEngine.UIElements.KeyDownEvent;

[CustomPropertyDrawer(typeof(LocalizedText))]
public class LocalizedTextEditor : PropertyDrawer
{
    string[] _textKeyOptions;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        if(_textKeyOptions == null)
            _textKeyOptions = SimpleTranslations.GetTranslationKeys();

        var keyProperty = property.FindPropertyRelative("Key");

        EditorGUI.BeginChangeCheck();
        
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        position.height = EditorGUIUtility.singleLineHeight;

        GUI.contentColor = GUI.backgroundColor = keyProperty.stringValue != "" ? Color.white : Color.grey;
        keyProperty.stringValue = AutoCompleteTextField.EditorGUI.AutoCompleteTextField(position, "", keyProperty.stringValue, _textKeyOptions, "Text key here");
        
        position.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(position, SimpleTranslations.GetText(keyProperty.stringValue));
        GUI.backgroundColor = Color.white;
        
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        
        EditorGUI.EndProperty();
    }
}
#endif