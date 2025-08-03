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

public class LocalizedTextField : VisualElement
{
    private CustomAutoCompleteTextField keyField;
    private Label translationPreview;
    private string[] textKeyOptions;
    private SerializedProperty keyProperty;

    public LocalizedTextField(SerializedProperty property, VisualElement container)
    {
        if (property == null || property.propertyType != SerializedPropertyType.Generic)
            return;

        keyProperty = property.FindPropertyRelative("Key");
        if (keyProperty == null || keyProperty.propertyType != SerializedPropertyType.String)
            return;

        // Load options once
        if (textKeyOptions == null || textKeyOptions.Length == 0)
            textKeyOptions = SimpleTranslations.GetTranslationKeys();

        style.flexDirection = FlexDirection.Column;
        style.marginBottom = 6;

        // Create input field
        keyField = new CustomAutoCompleteTextField(keyProperty, textKeyOptions, container, UpdateTranslationPreview);

        // Preview label
        translationPreview = new Label
        {
            style =
            {
                maxWidth = 500,
                marginTop = 4,
                whiteSpace = WhiteSpace.Normal
            }
        };

        UpdateTranslationPreview(keyField.value);

        Add(keyField);
        Add(translationPreview);
    }

    private void UpdateTranslationPreview(string key)
    {
        translationPreview.text = SimpleTranslations.GetText(key);
    }
}

public class CustomAutoCompleteTextField : VisualElement
{
    private readonly TextField inputField;
    private readonly ListView dropdownList;
    private readonly string[] allOptions;
    private readonly SerializedProperty boundProperty;

    private readonly VisualElement dropdownContainer;

    public string value => inputField.value;

    public CustomAutoCompleteTextField(SerializedProperty property, string[] options, VisualElement container, Action<string> onValueChange, string label = "Key")
    {
        boundProperty = property;
        allOptions = options;

        style.flexDirection = FlexDirection.Column;
        style.marginBottom = 6;

        inputField = new TextField(label);
        inputField.value = property.stringValue;

        inputField.RegisterValueChangedCallback(evt =>
        {
            property.stringValue = evt.newValue;
            property.serializedObject.ApplyModifiedProperties();
            FilterOptions(evt.newValue);
            onValueChange(evt.newValue);
        });

        inputField.RegisterCallback<FocusOutEvent>(_ => HideDropdown());
        inputField.RegisterCallback<KeyDownEvent>(OnKeyDown);

        Add(inputField);

        // Dropdown container
        dropdownContainer = new VisualElement
        {
            style =
            {
                position = Position.Absolute,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                borderTopLeftRadius = 4,
                borderTopRightRadius = 4,
                borderBottomLeftRadius = 4,
                borderBottomRightRadius = 4,
                borderBottomWidth = 1,
                borderTopWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                marginTop = -2,
                maxHeight = 150,
                overflow = Overflow.Visible,
            }
        };

        dropdownList = new ListView
        {
            selectionType = SelectionType.Single,
            showBorder = true,
            showAlternatingRowBackgrounds = AlternatingRowBackground.None,
            style =
            {
                flexGrow = 1,
                backgroundColor = Color.gray
            },
            makeItem = () => new Label(),
            bindItem = (e, i) =>
            {
                (e as Label).text = filtered[i];
            }
        };

        dropdownList.onSelectionChange += objects =>
        {
            var selected = objects.FirstOrDefault() as string;
            if (!string.IsNullOrEmpty(selected))
            {
                inputField.value = selected;
                HideDropdown();
            }
        };
        
        dropdownContainer.Add(dropdownList);
        container.parent.Add(dropdownContainer);
        HideDropdown();
    }

    private string[] filtered = Array.Empty<string>();

    private void FilterOptions(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            HideDropdown();
            return;
        }

        filtered = allOptions
            .Where(o => o.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
            .Take(10)
            .ToArray();

        if (filtered.Length == 0)
        {
            HideDropdown();
            return;
        }

        dropdownList.itemsSource = filtered;
        dropdownList.RefreshItems();
        ShowDropdown();
    }

    private void ShowDropdown()
    {
        dropdownContainer.BringToFront();
        dropdownContainer.style.display = DisplayStyle.Flex;
    }

    private void HideDropdown()
    {
        dropdownContainer.style.display = DisplayStyle.None;
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Escape)
        {
            HideDropdown();
            evt.StopPropagation();
        }
    }
}

#endif