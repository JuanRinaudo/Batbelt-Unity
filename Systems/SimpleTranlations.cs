using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleTranslations : MonoBehaviour
{
    public static bool translationsEnabled = false;

    public static SimpleTranslations instance;

    private static string RESOURCE_FILE_PATH = "Batbelt/Translations";

    [HideInInspector] public int languageCount;
    [HideInInspector] public string currentLanguage;
    private Dictionary<string, int> languageColumn = new Dictionary<string, int>();
    private Dictionary<string, string> languageValues = new Dictionary<string, string>();

    public string systemLanguage;
    public string defaultLanguage = "key";

    private static char[] charsToTrim = { ' ', '\r', '\n', '\t' };

    public static SimpleTranslations CreateTranslationsSingleton()
    {
        GameObject instance = new GameObject();
        instance.name = "SimpleTranslationsInstance";

        SimpleTranslations translations = instance.AddComponent<SimpleTranslations>();

        return translations;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }

        CultureInfo cultureInfo = CultureInfo.InstalledUICulture;
        systemLanguage = cultureInfo.TwoLetterISOLanguageName;

        SetLanguage(systemLanguage);

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;

        languageColumn.Clear();
        languageValues.Clear();
        TextAsset languageTexts = Resources.Load<TextAsset>(RESOURCE_FILE_PATH);

        Debug.Log(language);

        if(languageTexts != null)
        {
            string[] lines = languageTexts.text.Split('\n');
            string[] languages = lines[0].Trim(charsToTrim).Split('\t');
            
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
                string[] keys = lines[lineIndex].Split('\t');
                languageValues.Add(keys[0], keys[languageIndex]);
            }
        }
    }

    public bool HasKey(string key)
    {
        return languageValues != null && languageValues.ContainsKey(key);
    }

    public string GetText(string key)
    {
        if (languageValues != null)
        {
            return languageValues[key];
        }
        else
        {
            Debug.LogError("Translation key not found");
            return "KEY NOT FOUND!";
        }
    }

#if UNITY_EDITOR
    [MenuItem("Batbelt/Translation/Generate Languages File")]
    public static void GenerateLanguagesFile()
    {
        StreamWriter writter = new StreamWriter("Assets/Batbelt/CodeGen/TranslationLanguages.cs");
        writter.WriteLine("public class TranslationLanguages {");

        TextAsset languageTexts = Resources.Load<TextAsset>(RESOURCE_FILE_PATH);
        string[] lines = languageTexts.text.Split('\n');
        string[] languages = lines[0].Trim(charsToTrim).Split('\t');

        for (int languageIndex = 0; languageIndex < languages.Length; ++languageIndex)
        {
            if(languages[languageIndex] != "")
            {
                writter.WriteLine("\tpublic static string " + BatUtils.NormalizeKey(languages[languageIndex]) + " = \"" + languages[languageIndex] + "\";");
            }
        }

        writter.WriteLine("}");
        writter.Close();
    }

    [MenuItem("Batbelt/Translation/Generate Keys File")]
    public static void GenerateKeysFile()
    {
        StreamWriter writter = new StreamWriter("Assets/Batbelt/CodeGen/TranslationKeys.cs");
        writter.WriteLine("public class TranslationKeys {");

        TextAsset languageTexts = Resources.Load<TextAsset>(RESOURCE_FILE_PATH);
        string[] lines = languageTexts.text.Split('\n');

        int keyIndex = 0;
        for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
        {
            string[] keys = lines[lineIndex].Trim(charsToTrim).Split('\t');
            if (keys[0] != "")
            {
                writter.WriteLine("\tpublic static string " + BatUtils.NormalizeKey(keys[0]) + " = \"" + keys[0] + "\";");
                ++keyIndex;
            }
        }

        writter.WriteLine("}");
        writter.Close();
    }
#endif

}
