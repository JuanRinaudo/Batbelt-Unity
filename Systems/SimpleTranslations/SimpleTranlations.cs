using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleTranslations
{
    public static bool translationsEnabled = false;

    public static SimpleTranslations _instance;
    public static SimpleTranslations Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SimpleTranslations();
            
            return _instance;
        }
    }

    [HideInInspector] public string[] languages;
    [HideInInspector] public string currentLanguage;
    [HideInInspector] public int currentLanguageIndex;
    private Dictionary<string, int> languageColumn = new Dictionary<string, int>();
    private Dictionary<string, string> languageValues = new Dictionary<string, string>();

    public string systemLanguage;
    public string defaultLanguage = "en";

    private SimpleTranslationsConfig config;

    public static char[] charsToTrim = { ' ', '\r', '\n', '\t' };

    public static Action LanguageChanged;

    public SimpleTranslations()
    {
        if (_instance != null)
            return;

        _instance = this;

        if (!BatUtils.LoadConfig(out config))
        {
            Debug.LogError("Translation config file not found, make sure this is set up correctly");
        }

        CultureInfo cultureInfo = CultureInfo.InstalledUICulture;
        systemLanguage = cultureInfo.TwoLetterISOLanguageName;

        SetLanguage(systemLanguage);
    }
    
    public void SetLanguage(int languageIndex)
    {
        SetLanguage(languages[languageIndex]);
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;

        languageColumn.Clear();
        languageValues.Clear();
        string translationText = GetTranslationsFile();

        if(translationText != null && translationText != "")
        {
            string[] lines = translationText.Split('\n');
            languages = lines[0].Trim(charsToTrim).Split('\t');
            currentLanguageIndex = Array.IndexOf(languages, currentLanguage);
            
            for (int column = 0; column < languages.Length; ++column)
            {
                languageColumn.Add(languages[column], column);
            }

            if (!languageColumn.ContainsKey(currentLanguage))
            {
                Debug.LogError("No language found, setting default language " + defaultLanguage);
                currentLanguage = defaultLanguage;
            }

            int languageIndex = languageColumn[currentLanguage];
        
            for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
            {
                string[] lineValues = lines[lineIndex].Split('\t');
                languageValues.Add(lineValues[0], lineValues[languageIndex].Trim(charsToTrim));
            }
        }

        LanguageChanged?.Invoke();
    }

    public bool HasKey(string key)
    {
        return languageValues != null && languageValues.ContainsKey(key);
    }

    public string GetValue(string key)
    {
        if (key != null && languageValues != null && languageValues.ContainsKey(key))
        {
            return languageValues[key].Replace("\\n", "\n");
        }
        else
        {
            Debug.LogWarning($"Translation key not found: {key}");
            return null;
        }
    }

    public static T GetResource<T>(string key) where T : UnityEngine.Object
    {
        string value = Instance.GetValue(key);
        return value != null ? Resources.Load<T>(value) : default(T);
    }

    public static string GetText(string key)
    {
        string value = Instance.GetValue(key);
        return value != null ? value : "KEY NOT FOUND!";
    }

    public static string GetFormattedText(string key, params object[] args)
    {
        try {
            return string.Format(Instance.GetValue(key), args);
        }
        catch
        {
            Debug.LogError($"Formating error on id {key}");
            return "FORMAT ERROR";
        }
    }

    public static string GetTranslationsFile()
    {
        string translationText = "";

        SimpleTranslationsConfig config;
#if UNITY_EDITOR
        if(EditorApplication.isPlaying)
        {
            config = Instance.config;
        }
        else
        {
            if (!BatUtils.LoadConfig(out config, true))
            {
                Debug.LogError("Translation config file not found, make sure this is setted up correctly");
                return "";
            }
        }
#else
        config = Instance.config;
#endif

        if(config.useResources)
        {
            int formatDot = config.filename.LastIndexOf(".");
            string filename = formatDot > 0 ? config.filename.Substring(0, formatDot) : config.filename;

            TextAsset textAssets = Resources.Load<TextAsset>("Batbelt/" + filename);
            try {
                translationText = textAssets.text;
            }
            catch
            {
                Debug.LogError("Translation resource not found, check the filename");
            }
        }
        else
        {
#if UNITY_EDITOR
            SimpleTranslationsEditorConfig configEditor;
            if (BatUtils.LoadConfig(out configEditor, false))
            {
                try {
                    StreamReader reader = new StreamReader(configEditor.downloadFileFolder + "/" + config.filename);
                    translationText = reader.ReadToEnd();
                    reader.Close();
                }
                catch
                {
                    Debug.LogError("Translation file not found, check the path and if the file exists");
                }
            }
#else
            StreamReader reader = new StreamReader(BatUtils.GetUniquePersistentDataPath() + "/" + config.filename);
            translationText = reader.ReadToEnd();
            reader.Close();
#endif
        }

        return translationText;
    }

#if UNITY_EDITOR
    public static string[] GetTranslationKeys()
    {
        List<string> keys = new List<string>();
        string translationText = GetTranslationsFile();
        string[] lines = translationText.Split('\n');

        for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
        {
            string[] currentLine = lines[lineIndex].Trim(charsToTrim).Split('\t');
            keys.Add(currentLine[0]);
        }

        return keys.ToArray();
    }
#endif

}

public struct SimpleTranslationsConfig
{
    public bool inited;

    public string filename;
    public bool useResources;
}

#if UNITY_EDITOR
public struct SimpleTranslationsEditorConfig
{
    public bool inited;
    public string gDriveSheetFileId;
    public string tabId;

    public string downloadFileFolder;
}
#endif