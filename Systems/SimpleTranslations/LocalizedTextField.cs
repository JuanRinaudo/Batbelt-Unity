#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
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
        {
            var keysTask = SimpleTranslations.GetTranslationKeys();
            keysTask.Wait();
            TextKeyOptions = keysTask.Result;
        }

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
    
    bool _isFocused = false;

    public CustomAutoCompleteTextField(SerializedProperty property, string[] options, VisualElement container, Action<string> onValueChange, string label = "Key")
    {
        BoundProperty = property;
        AllOptions = options;

        style.flexDirection = FlexDirection.Column;
        style.marginBottom = 6;

        InputField = new TextField(label);
        InputField.BindProperty(property);

        InputField.RegisterCallback<FocusInEvent>(_ =>
        {
            _isFocused = true;
        });
        InputField.RegisterCallback<FocusOutEvent>(_ =>
        {
            _isFocused = false;
            HideDropdown();
        });
        InputField.RegisterCallback<KeyDownEvent>(e =>
        {
            OnKeyDown(e);
        });
        
        InputField.RegisterValueChangedCallback(evt =>
        {
            if (_isFocused)
                FilterOptions(evt.newValue);
        
            onValueChange(evt.newValue);
        });

        Add(InputField);

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
                overflow = Overflow.Visible,
            }
        };
        
        var styleAsset = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SimpleVN/StoryGraph/CustomEditors/Styles/NodeEditorStyle.uss");
        DropdownContainer.styleSheets.Add(styleAsset); 

        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");
        DropdownContainer.style.unityFont = font;

        DropdownList = new ListView
        {
            selectionType = SelectionType.Single,
            showBorder = true,
            fixedItemHeight = 30,
            showAlternatingRowBackgrounds = AlternatingRowBackground.All,
            style =
            {
                flexGrow = 1,
                backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.8f),
                overflow = Overflow.Hidden,
                
                borderLeftWidth = 2,
                borderRightWidth = 2,
                borderTopWidth = 2,
                borderBottomWidth = 2,

                borderLeftColor = Color.gray,
                borderRightColor = Color.gray,
                borderTopColor = Color.gray,
                borderBottomColor = Color.gray,

                borderTopLeftRadius = 2,
                borderTopRightRadius = 2,
                borderBottomLeftRadius = 2,
                borderBottomRightRadius = 2,
            },
            makeItem = () => 
            {
                var label = new Label { };
                return label;
            },
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
        
        RegisterCallback<AttachToPanelEvent>(evt =>
        {
            evt.destinationPanel.visualTree.contentContainer.Add(DropdownContainer);
            DropdownContainer.name = "DROPDOWN_CONTAINER";
            DropdownContainer.Add(DropdownList);
            HideDropdown();
        });

        RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            DropdownContainer.RemoveFromHierarchy();
        });
        
        DropdownList.RegisterCallback<AttachToPanelEvent>(evt => 
        {
            var scroller = DropdownList.Q<Scroller>();
            if (scroller != null)
            {
                scroller.style.width = 12;
                // Target the dragger (the part you click and pull)
                var dragger = scroller.Q<VisualElement>("unity-dragger");
                if (dragger != null) dragger.style.backgroundColor = Color.gray;
            }
        });
    }

    string[] filtered = Array.Empty<string>();

    void FilterOptions(string input)
    {
        filtered = AllOptions
            .Where(o => o.Contains(input, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        DropdownList.itemsSource = filtered;
        if (filtered.Length > 0)
        { 
            float itemHeight = DropdownList.fixedItemHeight;
            float totalHeight = Math.Min(filtered.Length * itemHeight, 250); 
            DropdownContainer.style.height = totalHeight;
            
            var worldBound = InputField.worldBound;
            DropdownContainer.style.left = worldBound.x;
            DropdownContainer.style.top = worldBound.yMax;
            DropdownContainer.style.width = worldBound.width;
            // DropdownContainer.style.backgroundColor = Color.clear;

            DropdownContainer.style.display = DisplayStyle.Flex;
            DropdownList.RefreshItems();
        }
        else
        {
            HideDropdown();
        }
    }

    void ShowDropdown()
    {
        DropdownContainer.BringToFront();
        DropdownContainer.style.display = DisplayStyle.Flex;
    }

    public void HideDropdown()
    {
        DropdownContainer.style.display = DisplayStyle.None;
    }

    void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Escape)
        {
            _isFocused = false;
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