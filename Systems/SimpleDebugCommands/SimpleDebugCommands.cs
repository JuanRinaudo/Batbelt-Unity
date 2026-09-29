using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR || DEBUG_BUILD

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class SimpleDebugCommandAttribute : Attribute  
{
    public string category;

    public SimpleDebugCommandAttribute(string category = SimpleDebugCommands.DefaultCategory)
    {
        this.category = category;
    }
}  

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class SimpleDebugIntAttribute : Attribute
{
    public string category;
    public int defaultValue;
    public int minValue;
    public int maxValue;
    public bool isInstant;

    public SimpleDebugIntAttribute(string category = SimpleDebugCommands.DefaultCategory, int defaultValue = 0, int minValue = int.MinValue, int maxValue = int.MaxValue, bool isInstant = false)
    {
        this.category = category;
        this.defaultValue = defaultValue;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.isInstant = isInstant;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class SimpleDebugFloatAttribute : Attribute
{
    public string category;
    public float defaultValue;
    public float minValue;
    public float maxValue;
    public bool isInstant;

    public SimpleDebugFloatAttribute(string category = SimpleDebugCommands.DefaultCategory, float defaultValue = 0f, float minValue = float.MinValue, float maxValue = float.MaxValue, bool isInstant = false)
    {
        this.category = category;
        this.defaultValue = defaultValue;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.isInstant = isInstant;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class SimpleDebugBoolAttribute : Attribute
{
    public string category;
    public bool defaultValue;

    public SimpleDebugBoolAttribute(string category = SimpleDebugCommands.DefaultCategory, bool defaultValue = false)
    {
        this.category = category;
        this.defaultValue = defaultValue;
    }
}

public class SimpleDebugCommands : MonoBehaviour
{
    struct DebugMemberInfo
    {
        public string Name;
        public SimpleDebugCommandAttribute CommandAttr;
        public SimpleDebugIntAttribute IntAttr;
        public SimpleDebugFloatAttribute FloatAttr;
        public SimpleDebugBoolAttribute BoolAttr;

        public MethodInfo Method;
        public PropertyInfo Property;

        public object GetValue()
        {
            if (Property != null && Property.CanRead)
            {
                return Property.GetValue(null);
            }
            return null;
        }

        public void SetValue(object value)
        {
            if (Property != null && Property.CanWrite)
            {
                Property.SetValue(null, value);
            }
            else if (Method != null)
            {
                Method.Invoke(null, new object[] { value });
            }
        }

        public void InvokeCommand()
        {
            if (Method != null)
            {
                Method.Invoke(null, null);
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void LoadDebuger()
    {
        if (Instance == null)
        {
            GameObject instance = Instantiate(Resources.Load<GameObject>("Batbelt/DebugCommands"));
            DontDestroyOnLoad(instance);

            Instance = instance.GetComponent<SimpleDebugCommands>();
        }
    }

    public const string DefaultCategory = "Default";

    public static SimpleDebugCommands Instance;
    
    public static bool IsActive => Instance != null && Instance.mainContainer.gameObject.activeSelf;
    
    static TMP_FontAsset _fontAsset;
    static Color _fontColor;
    
    [Header("References")]
    public RectTransform mainContainer;
    public ScrollRect commandsScrollRect;
    public RectTransform categoryViewport;
    public RectTransform categoryButtonsContent;
    public TextMeshProUGUI debugText;

    float _debugInputTimer;
    List<DebugMemberInfo> _debugMembers = new List<DebugMemberInfo>();

    string[] _debugLines = new string[8];
    int _debugIndex = 0;
    string _selectedCategory = DefaultCategory;
    
    Dictionary<string, RectTransform> _categoryContainer = new Dictionary<string, RectTransform>();

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Restart()
    {
        Instance = null;
    }
#endif

    public static void Init(TMP_FontAsset fontAsset, Color fontColor)
    {
        _fontAsset = fontAsset;
        _fontColor = fontColor;
    }

    void Awake()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        _debugMembers.Clear();

        for (int i = 0; i < assemblies.Length; ++i)
        {
            try 
            {
                var types = assemblies[i].GetTypes();
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    foreach (var m in methods)
                    {
                        var commandAttr = m.GetCustomAttribute<SimpleDebugCommandAttribute>();
                        var intAttr = m.GetCustomAttribute<SimpleDebugIntAttribute>();
                        var floatAttr = m.GetCustomAttribute<SimpleDebugFloatAttribute>();
                        var boolAttr = m.GetCustomAttribute<SimpleDebugBoolAttribute>();

                        if (commandAttr != null || intAttr != null || floatAttr != null || boolAttr != null)
                        {
                            _debugMembers.Add(new DebugMemberInfo
                            {
                                Name = m.Name,
                                Method = m,
                                CommandAttr = commandAttr,
                                IntAttr = intAttr,
                                FloatAttr = floatAttr,
                                BoolAttr = boolAttr
                            });
                        }
                    }

                    var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
                    foreach (var p in properties)
                    {
                        var commandAttr = p.GetCustomAttribute<SimpleDebugCommandAttribute>();
                        var intAttr = p.GetCustomAttribute<SimpleDebugIntAttribute>();
                        var floatAttr = p.GetCustomAttribute<SimpleDebugFloatAttribute>();
                        var boolAttr = p.GetCustomAttribute<SimpleDebugBoolAttribute>();

                        if (commandAttr != null || intAttr != null || floatAttr != null || boolAttr != null)
                        {
                            _debugMembers.Add(new DebugMemberInfo
                            {
                                Name = p.Name,
                                Property = p,
                                CommandAttr = commandAttr,
                                IntAttr = intAttr,
                                FloatAttr = floatAttr,
                                BoolAttr = boolAttr
                            });
                        }
                    }
                }
            }
            catch 
            {
            }
        }

        for (int i = 0; i < _debugLines.Length; ++i)
        {
            _debugLines[i] = "";
        }
        Application.logMessageReceived += LogMessage;

        mainContainer.gameObject.SetActive(false);
    }

    void RefreshUI()
    {
        string previousCategory = _selectedCategory;

        foreach (var container in _categoryContainer.Values)
        {
            if (container != null) 
                DestroyImmediate(container.gameObject);
        }
        _categoryContainer.Clear();

        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in categoryButtonsContent)
        {
            childrenToDestroy.Add(child.gameObject);
        }
        for (int i = 0; i < childrenToDestroy.Count; i++)
        {
            DestroyImmediate(childrenToDestroy[i]);
        }

        for (int i = 0; i < _debugMembers.Count; ++i)
        {
            var member = _debugMembers[i];

            if (member.BoolAttr != null)
            {
                RectTransform container = GetCategoryParent(member.BoolAttr.category);
                GameObject boolInputInstance = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCommandBoolInput"), container, true);

                Toggle toggle = boolInputInstance.GetComponentInChildren<Toggle>();
                TextMeshProUGUI labelText = boolInputInstance.GetComponentInChildren<TextMeshProUGUI>();

                if (labelText != null)
                {
                    labelText.text = member.Name;
                    labelText.font = _fontAsset;
                    labelText.color = _fontColor;
                }

                if (toggle != null)
                {
                    object currentVal = member.GetValue();
                    toggle.isOn = currentVal != null ? (bool)currentVal : member.BoolAttr.defaultValue;

                    toggle.onValueChanged.AddListener((bool isOn) =>
                    {
                        member.SetValue(isOn);
                        RefreshUI();
                    });
                }
            }
            else if (member.FloatAttr != null)
            {
                RectTransform container = GetCategoryParent(member.FloatAttr.category);
                GameObject floatInputInstance = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCommandFloatInput"), container, true);

                TMP_InputField inputField = floatInputInstance.GetComponentInChildren<TMP_InputField>();
                TextMeshProUGUI labelText = floatInputInstance.GetComponentInChildren<TextMeshProUGUI>();

                if (labelText != null)
                {
                    labelText.text = member.Name;
                    labelText.font = _fontAsset;
                    labelText.color = _fontColor;
                }

                if (inputField != null)
                {
                    inputField.textComponent.color = _fontColor;
                    inputField.fontAsset = _fontAsset;
                    inputField.contentType = TMP_InputField.ContentType.DecimalNumber;

                    object currentVal = member.GetValue();
                    float initialValue = currentVal != null ? Convert.ToSingle(currentVal) : member.FloatAttr.defaultValue;
                    inputField.text = initialValue.ToString(CultureInfo.InvariantCulture);

                    void HandleInputSubmit(string newText, bool refresh = false)
                    {
                        if (float.TryParse(newText, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                        {
                            float clampedValue = Mathf.Clamp(val, member.FloatAttr.minValue, member.FloatAttr.maxValue);

                            if (!Mathf.Approximately(clampedValue, val)) inputField.text = clampedValue.ToString(CultureInfo.InvariantCulture);

                            member.SetValue(clampedValue);
                            if (refresh)
                                RefreshUI();
                        }
                    }

                    if (member.FloatAttr.isInstant)
                    {
                        inputField.onValueChanged.AddListener((val) => HandleInputSubmit(val));
                    }
                    else
                    {
                        inputField.onEndEdit.AddListener((val) => HandleInputSubmit(val, true));
                    }
                }
            }
            else if (member.IntAttr != null)
            {
                RectTransform container = GetCategoryParent(member.IntAttr.category);
                GameObject intInputInstance = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCommandIntInput"), container, true);

                TMP_InputField inputField = intInputInstance.GetComponentInChildren<TMP_InputField>();
                TextMeshProUGUI labelText = intInputInstance.GetComponentInChildren<TextMeshProUGUI>();

                if (labelText != null)
                {
                    labelText.text = member.Name;
                    labelText.font = _fontAsset;
                    labelText.color = _fontColor;
                }

                if (inputField != null)
                {
                    inputField.textComponent.color = _fontColor;
                    inputField.fontAsset = _fontAsset;
                    inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

                    object currentVal = member.GetValue();
                    int initialValue = currentVal != null ? Convert.ToInt32(currentVal) : member.IntAttr.defaultValue;
                    inputField.text = initialValue.ToString();

                    void HandleInputSubmit(string newText, bool refresh = false)
                    {
                        if (int.TryParse(newText, out int val))
                        {
                            int clampedValue = Mathf.Clamp(val, member.IntAttr.minValue, member.IntAttr.maxValue);

                            if (clampedValue != val) inputField.text = clampedValue.ToString();

                            member.SetValue(clampedValue);
                            if(refresh)
                                RefreshUI();
                        }
                    }

                    if (member.IntAttr.isInstant)
                    {
                        inputField.onValueChanged.AddListener((val) => HandleInputSubmit(val));
                    }
                    else
                    {
                        inputField.onEndEdit.AddListener((val) => HandleInputSubmit(val, true));
                    }
                }
            }
            else if (member.CommandAttr != null)
            {
                RectTransform container = GetCategoryParent(member.CommandAttr.category);

                GameObject buttonInstance = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCommandButton"), container, true);
                Button button = buttonInstance.GetComponent<Button>();
                TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
                button.onClick.AddListener(() => 
                { 
                    member.InvokeCommand(); 
                    RefreshUI();
                });
                text.text = member.Name;
                text.font = _fontAsset;
                text.color = _fontColor;
            }
        }

        foreach (RectTransform containerRectTransform in _categoryContainer.Values)
        {
            containerRectTransform.sizeDelta = new Vector2(0, 110f * containerRectTransform.childCount);
            containerRectTransform.localScale = Vector3.one;
        }

        if (_categoryContainer.ContainsKey(previousCategory))
        {
            SelectCategory(previousCategory);
        }
        else if (_categoryContainer.ContainsKey(DefaultCategory))
        {
            SelectCategory(DefaultCategory);
        }
        else if (_categoryContainer.Count > 0)
        {
            SelectCategory(_categoryContainer.Keys.First());
        }
    }

    void SelectCategory(string categoryName)
    {
        _selectedCategory = categoryName;

        foreach (RectTransform containerRectTransform in _categoryContainer.Values)
        {
            containerRectTransform.gameObject.SetActive(false);
        }

        RectTransform categoryRectTransform = GetCategoryParent(categoryName);
        categoryRectTransform.gameObject.SetActive(true);
        commandsScrollRect.content = categoryRectTransform;
    }

    RectTransform GetCategoryParent(string categoryName)
    {
        RectTransform categoryRectTransform;
        if (!_categoryContainer.TryGetValue(categoryName, out categoryRectTransform))
        {
            var categoryInstance = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCategoryContent"), categoryViewport, true);
            categoryButtonsContent.sizeDelta = new Vector2((_categoryContainer.Count + 1) * 320f, 0);
            categoryRectTransform = categoryInstance.GetComponent<RectTransform>();

            Button categoryButton = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCommandCategoryButton"), categoryButtonsContent, true).GetComponent<Button>();
            var categoryText = categoryButton.GetComponentInChildren<TextMeshProUGUI>();
            categoryText.text = categoryName;
            categoryText.font = _fontAsset;
            categoryText.color = _fontColor;
            
            RectTransform categoryButtonRectTransform = categoryButton.GetComponent<RectTransform>();
            categoryButtonRectTransform.anchoredPosition = new Vector2(_categoryContainer.Count * 320f, 0);
            categoryButtonRectTransform.localScale = Vector3.one;
            string savedCategoryName = categoryName;
            categoryButton.onClick.AddListener(() => {
                SelectCategory(savedCategoryName);
            });

            _categoryContainer.Add(categoryName, categoryRectTransform);
        }
        return categoryRectTransform;
    }

    void Update()
    {
        bool touchInputToggle = false;
        int activeTouchCount = GetActiveTouchCount();

        if (activeTouchCount == 3 && _debugInputTimer >= 0)
        {
            _debugInputTimer += Time.deltaTime;
            if (_debugInputTimer > 0.5f)
            {
                touchInputToggle = true;
                _debugInputTimer = -1;
            }
        }
        else
        {
            _debugInputTimer = 0;
        }

        if (IsF1KeyPressedThisFrame() || touchInputToggle)
        {
            ToggleDebugCommandsWindow();
        }
    }

    int GetActiveTouchCount()
    {
        int count = 0;

#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null)
        {
            count += Touchscreen.current.touches.Count(t => t.isInProgress);
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (count == 0)
        {
            count = Input.touchCount;
        }
#endif

        return count;
    }

    bool IsF1KeyPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.F1))
        {
            return true;
        }
#endif

        return false;
    }

    public void ToggleDebugCommandsWindow()
    {
        bool willBeActive = !mainContainer.gameObject.activeSelf;
        if (willBeActive)
        {
            RefreshUI();
        }

        mainContainer.gameObject.SetActive(willBeActive);
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        string colorHex = "#000000";
        switch (type)
        {
            case LogType.Log: { colorHex = "#000000"; } break;
            case LogType.Warning: { colorHex = "#AAAA00"; } break;
            case LogType.Error: { colorHex = "#AA0000"; } break;
            case LogType.Exception: { colorHex = "#AA0000"; } break;
            case LogType.Assert: { colorHex = "#0000AA"; } break;
        }

        _debugLines[_debugIndex] = $"<color={colorHex}>{message}</color>";

        debugText.text = "";
        for (int i = 0; i < _debugLines.Length; ++i)
        {
            debugText.text += _debugLines[(i + _debugIndex) % (_debugLines.Length - 1)] + "\n";
        }

        _debugIndex = (_debugIndex + 1) % _debugLines.Length;
    }
}
#endif