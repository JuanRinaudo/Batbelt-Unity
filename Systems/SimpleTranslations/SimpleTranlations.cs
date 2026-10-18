using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Text;
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

    public string[] languages;
    public string currentLanguage;
    public int currentLanguageIndex;
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

            for (int column = 0; column < languages.Length; ++column)
            {
                languageColumn[languages[column]] = column;
            }

            if (!languageColumn.ContainsKey(currentLanguage))
            {
                Debug.LogError("No language found, setting default language " + defaultLanguage);
                currentLanguage = defaultLanguage;
            }

            currentLanguageIndex = Array.IndexOf(languages, currentLanguage);

            if (languageColumn.TryGetValue(currentLanguage, out int languageIndex))
            {
                for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
                {
                    if (string.IsNullOrWhiteSpace(lines[lineIndex]))
                        continue;

                    string[] lineValues = lines[lineIndex].Split('\t');
                    languageValues[lineValues[0].Trim(charsToTrim)] = languageIndex < lineValues.Length ? lineValues[languageIndex].Trim(charsToTrim) : string.Empty;
                }
            }
            else
            {
                Debug.LogError("Default language " + defaultLanguage + " not found in translations file");
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
                Debug.LogError("Translation config file not found, make sure this is set up correctly, moving too fallback");
                return TryGetFallbackText();
            }
        }
#else
        config = Instance.config;
#endif

        try
        {
#if UNITY_EDITOR
            var filePath = GetEditorTranslationsPath(config);
            if (filePath != null)
            {
                try
                {
                    translationText = File.ReadAllText(filePath);
                }
                catch
                {
                    Debug.LogWarning("Translation file not found, check the path and if the file exists");
                }
            }
#else
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
                var targetFilename = config.filename;
                StreamReader reader = new StreamReader(BatUtils.GetUniquePersistentDataPath() + "/" + targetFilename);
                translationText = reader.ReadToEnd();
                reader.Close();
            }
#endif
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            
            return TryGetFallbackText();
        }

        return translationText;
    }

    public bool ValidateTranslationsFile(string filepath, out string message)
    {
        return ValidateTranslationsFile(filepath, out message, out _);
    }

    public bool ValidateTranslationsFile(string filepath, out string message, out string warningsMessage)
    {
        StreamReader reader = new StreamReader(filepath);
        var translationText = reader.ReadToEnd();
        reader.Close();
        
        var keys = new HashSet<string>();
        string[] lines = translationText.Split('\n');

        var issuesCount = 0;
        
        StringBuilder messageBuilder = new StringBuilder();

        for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
        {
            string[] currentLine = lines[lineIndex].Trim(charsToTrim).Split('\t');
            if (keys.Contains(currentLine[0]))
            {
                issuesCount++;
                messageBuilder.AppendLine($"Error {currentLine[0]} duplicated on line {lineIndex}");
            }
            else
            {
                keys.Add(currentLine[0]);
            }
        }
        
        if(issuesCount > 0)
            message = messageBuilder.ToString();
        else
            message = "No issues found in translations file";

        warningsMessage = string.Join("\n", GetLineBreakWarnings(translationText));

        return issuesCount == 0;
    }

    // Checks that every non empty translation of a key has the same amount of line breaks (\n) across languages
    public static List<string> GetLineBreakWarnings(string translationText)
    {
        var warnings = new List<string>();
        if (string.IsNullOrEmpty(translationText))
            return warnings;

        string[] lines = translationText.Split('\n');
        string[] languages = lines[0].Trim(charsToTrim).Split('\t');

        StringBuilder countsBuilder = new StringBuilder();
        for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
        {
            string[] lineValues = lines[lineIndex].Split('\t');
            string key = lineValues[0].Trim(charsToTrim);
            if (key == "")
                continue;

            int expectedLineBreaks = -1;
            bool mismatch = false;
            countsBuilder.Clear();

            for (int column = 1; column < lineValues.Length; ++column)
            {
                string value = lineValues[column].Trim(charsToTrim);
                if (value == "")
                    continue;

                int lineBreaks = CountLineBreaks(value);
                if (expectedLineBreaks == -1)
                    expectedLineBreaks = lineBreaks;
                else if (expectedLineBreaks != lineBreaks)
                    mismatch = true;

                string language = column < languages.Length ? languages[column] : $"column {column}";
                if (countsBuilder.Length > 0)
                    countsBuilder.Append(", ");
                countsBuilder.Append($"{language}: {lineBreaks}");
            }

            if (mismatch)
                warnings.Add($"Warning {key} has different line breaks on line {lineIndex} ({countsBuilder})");
        }

        return warnings;
    }

    public static int CountLineBreaks(string value)
    {
        int count = 0;
        int index = value.IndexOf("\\n", StringComparison.Ordinal);
        while (index != -1)
        {
            count++;
            index = value.IndexOf("\\n", index + 2, StringComparison.Ordinal);
        }

        return count;
    }

    static string TryGetFallbackText()
    {
        // Use reflection to check if SimpleTranslationsFallback has a public static string field called FallbackBaseText, and if it does, use that as the base translation text
        Type fallbackType = typeof(SimpleTranslationsFallback);
        var fallbackField = fallbackType.GetField("FallbackBaseText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (fallbackField != null && fallbackField.FieldType == typeof(string))
        {
            return (string)fallbackField.GetValue(null);
        }

        return "";
    }

#if UNITY_EDITOR
    static string[] _cachedKeys;
    static string _cachedKeysPath;
    static DateTime _cachedKeysWriteTime;

    static string GetEditorTranslationsPath(SimpleTranslationsConfig config)
    {
        if (!BatUtils.LoadConfig(out SimpleTranslationsEditorConfig configEditor, false))
            return null;

        string folderPath;
        if (config.fileMethod == SimpleTranslationsConfig.FileMethod.Resources)
            folderPath = "Assets/Batbelt/Resources/Batbelt/";
        else if (config.fileMethod == SimpleTranslationsConfig.FileMethod.Addressables)
            folderPath = "Assets/VNGame/Addressables/";
        else
            folderPath = configEditor.downloadFileFolder;

        return $"{folderPath}{config.filename}";
    }

    static string GetCurrentEditorTranslationsPath()
    {
        SimpleTranslationsConfig config;
        if (EditorApplication.isPlaying)
            config = Instance.config;
        else if (!BatUtils.LoadConfig(out config, true))
            return null;

        return GetEditorTranslationsPath(config);
    }

    public static async Task<string[]> GetTranslationKeys()
    {
        string path = GetCurrentEditorTranslationsPath();
        bool fileExists = path != null && File.Exists(path);
        if (fileExists && _cachedKeys != null && _cachedKeysPath == path && _cachedKeysWriteTime == File.GetLastWriteTimeUtc(path))
            return _cachedKeys;

        List<string> keys = new List<string>();
        string translationText = await GetTranslationsFile();
        string[] lines = translationText.Split('\n');

        for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
        {
            string line = lines[lineIndex].Trim(charsToTrim);
            if (line.Length == 0)
                continue;

            keys.Add(line.Split('\t')[0]);
        }

        var result = keys.ToArray();
        if (fileExists)
        {
            _cachedKeys = result;
            _cachedKeysPath = path;
            _cachedKeysWriteTime = File.GetLastWriteTimeUtc(path);
        }

        return result;
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