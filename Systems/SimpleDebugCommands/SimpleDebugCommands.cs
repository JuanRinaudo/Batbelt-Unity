using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR || DEBUG_BUILD
[AttributeUsage(AttributeTargets.Method)]
public class SimpleDebugCommandAttribute : Attribute  
{
    public string category;

    public SimpleDebugCommandAttribute(string category = SimpleDebugCommands.DefaultCategory)
    {
        this.category = category;
    }
}  

public class SimpleDebugCommands : MonoBehaviour
{
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void LoadDebuger()
    {
        if(Instance == null)
        {
            GameObject instance = Instantiate(Resources.Load<GameObject>("Batbelt/DebugCommands"));
            DontDestroyOnLoad(instance);

            Instance = instance.GetComponent<SimpleDebugCommands>();
        }
    }

    public const string DefaultCategory = "Default";

    public static SimpleDebugCommands Instance;

    [Header(AttributeConstants.HeaderReferences)]
    public RectTransform mainContainer;
    public ScrollRect commandsScrollRect;
    public RectTransform categoryViewport;
    public RectTransform categoryButtonsContent;
    public TextMeshProUGUI debugText;

    private float debugInputTimer;
    private MethodInfo[] debugMethods;

    private string[] debugLines = new string[8];
    private int debugIndex = 0;

    private Dictionary<string, RectTransform> categoryContainer;

    private void Awake()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<MethodInfo> allDebugMethods = new List<MethodInfo>();
        for(int i = 0; i < assemblies.Length; ++i)
        {
            try {
                var assemblyMethods = assemblies[i].GetTypes()
                            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                            .Where(m => m.GetCustomAttributes(typeof(SimpleDebugCommandAttribute), false).Length > 0)
                            .ToArray();
                allDebugMethods.AddRange(assemblyMethods);
            }
            catch {
                
            }
        }

        debugMethods = allDebugMethods.ToArray();

        categoryContainer = new Dictionary<string, RectTransform>();
        
        for(int i = 0; i < debugMethods.Length; ++i)
        {
            var attribute = debugMethods[i].GetCustomAttribute<SimpleDebugCommandAttribute>();

            RectTransform container = GetCategoryParent(attribute.category);

            GameObject buttonInstance = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCommandButton"), container, true);
            Button button = buttonInstance.GetComponent<Button>();
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            int index = i;
            button.onClick.AddListener(() => { debugMethods[index].Invoke(this, null); });
            text.text = debugMethods[index].Name;
        }

        foreach(RectTransform containerRectTransform in categoryContainer.Values)
        {
            containerRectTransform.sizeDelta = new Vector2(0, 110f * containerRectTransform.childCount);
            containerRectTransform.localScale = Vector3.one;
        }

        for(int i = 0; i < debugLines.Length; ++i)
        {
            debugLines[i] = "";
        }
        Application.logMessageReceived += LogMessage;

        mainContainer.gameObject.SetActive(false);
        SelectCategory(DefaultCategory);
    }

    private void SelectCategory(string categoryName)
    {
        foreach(RectTransform containerRectTransform in categoryContainer.Values)
        {
            containerRectTransform.gameObject.SetActive(false);
        }

        RectTransform categoryRectTransform = GetCategoryParent(categoryName);
        categoryRectTransform.gameObject.SetActive(true);
        commandsScrollRect.content = categoryRectTransform;
    }

    private RectTransform GetCategoryParent(string categoryName)
    {
        RectTransform categoryRectTransform;
        if(!categoryContainer.TryGetValue(categoryName, out categoryRectTransform))
        {
            var categoryInstance = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCategoryContent"), categoryViewport, true);
            categoryButtonsContent.sizeDelta = new Vector2((categoryContainer.Count + 1) * 320f, 0);
            categoryRectTransform = categoryInstance.GetComponent<RectTransform>();

            Button categoryButton = Instantiate(Resources.Load<GameObject>("Batbelt/SimpleCommandCategoryButton"), categoryButtonsContent, true).GetComponent<Button>();
            categoryButton.GetComponentInChildren<TextMeshProUGUI>().text = categoryName;
            RectTransform categoryButtonRectTransform = categoryButton.GetComponent<RectTransform>();
            categoryButtonRectTransform.anchoredPosition = new Vector2(categoryContainer.Count * 320f, 0);
            categoryButtonRectTransform.localScale = Vector3.one;
            string savedCategoryName = categoryName;
            categoryButton.onClick.AddListener(() => {
                SelectCategory(savedCategoryName);
            });

            categoryContainer.Add(categoryName, categoryRectTransform);
        }
        return categoryRectTransform;
    }
    
    private void Update()
    {
        bool touchInputToggle = false;
        if(Input.touchCount == 2 && debugInputTimer >= 0)
        {
            debugInputTimer += Time.deltaTime;
            if(debugInputTimer > 0.5f)
            {
                touchInputToggle = true;
                debugInputTimer = -1;
            }
        }
        else
        {
            debugInputTimer = 0;
        }

        if(Input.GetKeyDown(KeyCode.F1) || touchInputToggle)
        {
            ToggleDebugCommandsWindow();
        }
    }

    public void ToggleDebugCommandsWindow()
    {
        mainContainer.gameObject.SetActive(!mainContainer.gameObject.activeSelf);
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        string colorHex = "#000000";
        switch(type)
        {
            case LogType.Log: { colorHex = "#000000"; } break;
            case LogType.Warning: { colorHex = "#AAAA00"; } break;
            case LogType.Error: { colorHex = "#AA0000"; } break;
            case LogType.Exception: { colorHex = "#AA0000"; } break;
            case LogType.Assert: { colorHex = "#0000AA"; } break;
        }

        debugLines[debugIndex] = $"<color={colorHex}>{message}</color>";

        debugText.text = "";
        for(int i = 0; i < debugLines.Length; ++i)
        {
            debugText.text += debugLines[(i + debugIndex) % (debugLines.Length - 1)] + "\n";
        }

        debugIndex = (debugIndex + 1) % debugLines.Length;
    }

}
#endif