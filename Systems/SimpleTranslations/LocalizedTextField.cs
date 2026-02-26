#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalizedTextField : VisualElement
{
    public CustomAutoCompleteTextField KeyField;
    public Label TranslationPreview;
    public string[] TextKeyOptions;
    public SerializedProperty KeyProperty;

    public LocalizedTextField(SerializedProperty property, VisualElement container)
    {
        if (property == null || property.propertyType != SerializedPropertyType.Generic)
            return;

        KeyProperty = property.FindPropertyRelative("Key");
        if (KeyProperty == null || KeyProperty.propertyType != SerializedPropertyType.String)
            return;

        // Load options once
        if (TextKeyOptions == null || TextKeyOptions.Length == 0)
            TextKeyOptions = SimpleTranslations.GetTranslationKeys();

        style.flexDirection = FlexDirection.Column;
        style.marginBottom = 6;

        // Create input field
        KeyField = new CustomAutoCompleteTextField(KeyProperty, TextKeyOptions, container, UpdateTranslationPreview);
        
        // Preview label
        TranslationPreview = new Label
        {
            style =
            {
                maxWidth = 500,
                marginTop = 4,
                whiteSpace = WhiteSpace.Normal
            }
        };

        UpdateTranslationPreview(KeyField.Value);

        Add(KeyField);
        Add(TranslationPreview);
    }

    void UpdateTranslationPreview(string key)
    {
        TranslationPreview.text = SimpleTranslations.SilentGetText(key);
    }

    public void ClearValue()
    {
        KeyField.ClearValue();
    }

    public void RegisterValueChangeCallback(Action callback)
    {
        KeyField.InputField.RegisterValueChangedCallback(_ => callback?.Invoke());
    }
}

public class CustomAutoCompleteTextField : VisualElement
{
    public TextField InputField;
    public ListView DropdownList;
    public string[] AllOptions;
    public SerializedProperty BoundProperty;

    public VisualElement DropdownContainer;

    public string Value => InputField.value;

    public CustomAutoCompleteTextField(SerializedProperty property, string[] options, VisualElement container, Action<string> onValueChange, string label = "Key")
    {
        BoundProperty = property;
        AllOptions = options;

        style.flexDirection = FlexDirection.Column;
        style.marginBottom = 6;

        InputField = new TextField(label);
        InputField.value = property.stringValue;

        InputField.RegisterValueChangedCallback(evt =>
        {
            property.stringValue = evt.newValue;
            property.serializedObject.ApplyModifiedProperties();
            FilterOptions(evt.newValue);
            onValueChange(evt.newValue);
        });

        InputField.RegisterCallback<FocusOutEvent>(_ => HideDropdown());
        InputField.RegisterCallback<KeyDownEvent>(OnKeyDown);

        Add(InputField);

        // Dropdown container
        DropdownContainer = new VisualElement
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

        DropdownList = new ListView
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

        DropdownList.onSelectionChange += objects =>
        {
            var selected = objects.FirstOrDefault() as string;
            if (!string.IsNullOrEmpty(selected))
            {
                InputField.value = selected;
                HideDropdown();
            }
        };
        
        DropdownContainer.Add(DropdownList);
        container.parent.Add(DropdownContainer);
        HideDropdown();
    }

    string[] filtered = Array.Empty<string>();

    void FilterOptions(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            HideDropdown();
            return;
        }

        filtered = AllOptions
            .Where(o => o.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
            .Take(10)
            .ToArray();

        if (filtered.Length == 0)
        {
            HideDropdown();
            return;
        }

        DropdownList.itemsSource = filtered;
        DropdownList.RefreshItems();
        ShowDropdown();
    }

    void ShowDropdown()
    {
        DropdownContainer.BringToFront();
        DropdownContainer.style.display = DisplayStyle.Flex;
    }

    void HideDropdown()
    {
        DropdownContainer.style.display = DisplayStyle.None;
    }

    void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Escape)
        {
            HideDropdown();
            evt.StopPropagation();
        }
    }

    public void ClearValue()
    {
        InputField.value = string.Empty;
    }
}
#endif