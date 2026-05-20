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

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if (_textKeyOptions == null || _textKeyOptions.Length == 0)
        {
            var keysTask = SimpleTranslations.GetTranslationKeys();
            keysTask.Wait();
            _textKeyOptions = keysTask.Result;
        }

        var keyProperty = property.FindPropertyRelative("Key");
        var container = new VisualElement();

        var previewLabel = new Label(SimpleTranslations.GetText(keyProperty.stringValue));
        previewLabel.style.color = new Color(0.7f, 0.7f, 0.7f);

        var keyField = new CustomAutoCompleteTextField(
            keyProperty,
            _textKeyOptions,
            container,
            newValue => {
                var freshProperty = property.FindPropertyRelative("Key");
                freshProperty.stringValue = newValue;
                freshProperty.serializedObject.ApplyModifiedProperties();
                previewLabel.text = SimpleTranslations.GetText(newValue);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            },
            property.name
        );

        container.Add(keyField);
        container.Add(previewLabel);

        return container;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        if (_textKeyOptions == null || _textKeyOptions.Length == 0)
        {
            var keysTask = SimpleTranslations.GetTranslationKeys();
            keysTask.Wait();
            _textKeyOptions = keysTask.Result;
        }

        var keyProperty = property.FindPropertyRelative("Key");

        EditorGUI.BeginChangeCheck();
        
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        position.height = EditorGUIUtility.singleLineHeight;

        var originalContentColor = GUI.contentColor;
        GUI.contentColor = GUI.backgroundColor = keyProperty.stringValue != "" ? Color.white : Color.grey;
        keyProperty.stringValue = AutoCompleteTextField.EditorGUI.AutoCompleteTextField(position, "", keyProperty.stringValue, _textKeyOptions, true);
        
        position.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(position, SimpleTranslations.GetText(keyProperty.stringValue));
        GUI.backgroundColor = Color.white;
        
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        
        GUI.contentColor = originalContentColor;
        
        EditorGUI.EndProperty();
    }
}
#endif