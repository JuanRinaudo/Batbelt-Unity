using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    
    public bool LoadFailed = false;

    public string systemLanguage;
    public string defaultLanguage = "en";

    private SimpleTranslationsConfig config;

    public static char[] charsToTrim = { ' ', '\r', '\n', '\t' };
    
    public static Action LanguageChanged;
    
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Restart()
    {
        _instance = null;
        LanguageChanged = delegate { };
    }
#endif

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

    public async Task SetLanguage(string language)
    {
        currentLanguage = language;

        languageColumn.Clear();
        languageValues.Clear();
        string translationText = await GetTranslationsFile();

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

    public string SilentGetValue(string key)
    {
        if (key != null && languageValues != null && languageValues.ContainsKey(key))
        {
            return languageValues[key].Replace("\\n", "\n");
        }
        
        return null;
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

    public static string SilentGetText(string key)
    {
        string value = Instance.SilentGetValue(key);
        return value != null ? value : "KEY NOT FOUND!";
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

    public static async Task<string> GetTranslationsFile()
    {
        string translationText = "";
        
#if UNITY_EDITOR
        // Use reflection to check if SimpleTranslationsFallback has a public static string field called FallbackBaseText, and if it does, use that as the base translation text
        Type fallbackType = typeof(SimpleTranslationsFallback);
        var fallbackField = fallbackType.GetField("FallbackBaseText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (fallbackField != null && fallbackField.FieldType == typeof(string))
            translationText = (string)fallbackField.GetValue(null);
#endif

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
                Debug.LogError("Translation config file not found, make sure this is set up correctly");
                return "";
            }
        }
#else
        config = Instance.config;
#endif

        try
        {
            if (config.fileMethod == SimpleTranslationsConfig.FileMethod.Resources)
            {
                var targetFilename = config.filename;
                int formatDot = targetFilename.LastIndexOf(".");
                string filename = formatDot > 0 ? targetFilename.Substring(0, formatDot) : targetFilename;

                TextAsset textAssets = Resources.Load<TextAsset>("Batbelt/" + filename);
                try
                {
                    translationText = textAssets.text;
                }
                catch
                {
                    Debug.LogWarning("Translation resource not found, check the filename");
                    throw;
                }
            }
            else if (config.fileMethod == SimpleTranslationsConfig.FileMethod.Addressables)
            {
                var targetFilename = config.filename;

                var locationsHandle = Addressables.LoadResourceLocationsAsync(targetFilename);
                await locationsHandle.Task;

                bool keyExists = locationsHandle.Status == AsyncOperationStatus.Succeeded &&
                                 locationsHandle.Result != null &&
                                 locationsHandle.Result.Count > 0; 

                Addressables.Release(locationsHandle);

                if (!keyExists)
                {
                    Debug.LogWarning($"Invalid addressable key: {targetFilename}");
                    throw new Exception($"Invalid addressable key: {targetFilename}");
                }

                var loadHandle = Addressables.LoadAssetAsync<TextAsset>(targetFilename);
                await loadHandle.Task;

                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                    translationText = loadHandle.Result.text;
                else
                    Debug.LogWarning($"Failed to load addressable: {targetFilename}");

                Addressables.Release(loadHandle);
            }
            else
            {
#if UNITY_EDITOR
                SimpleTranslationsEditorConfig configEditor;
                if (BatUtils.LoadConfig(out configEditor, false))
                {
                    try
                    {
                        var targetFilename = config.filename;
                        StreamReader reader = new StreamReader(configEditor.downloadFileFolder + "/" + targetFilename);
                        translationText = reader.ReadToEnd();
                        reader.Close();
                    }
                    catch
                    {
                        Debug.LogWarning("Translation file not found, check the path and if the file exists");
                    }
                }
#else
                var targetFilename = config.filename;
                StreamReader reader = new StreamReader(BatUtils.GetUniquePersistentDataPath() + "/" + targetFilename);
                translationText = reader.ReadToEnd();
                reader.Close();
#endif
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            throw;
        }

        return translationText;
    }

#if UNITY_EDITOR
    public static async Task<string[]> GetTranslationKeys()
    {
        List<string> keys = new List<string>();
        string translationText = await GetTranslationsFile();
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
    public enum FileMethod
    {
        Resources,
        File,
        Addressables,
        // MultipleResources, // TODO: Implement split language support
        // MultipleFiles,
        // MultipleAddressables
    }
    
    public bool inited;

    public string filename;
    public FileMethod fileMethod;
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