using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TranslationKeys
{

}

public enum TranslationLanguages
{
    SPANISH,
    ENGLISH,
    PORTUGUESE
}

public class TranslationManager : MonoBehaviour
{

    private static string FILE_PATH = "Data/Translations";

    [HideInInspector] public int languageCount;
    [HideInInspector] public TranslationLanguages currentLanguage;
    private string[] languageKeys;

    public TranslationLanguages defaultLanguage;

    private void Awake()
    {
        languageCount = System.Enum.GetNames(typeof(TranslationLanguages)).Length;
        SelectDefaultLanguage();
        SetLanguage(defaultLanguage);
    }

    private void SelectDefaultLanguage()
    {
        if (Application.systemLanguage == SystemLanguage.Spanish)
        {
            defaultLanguage = TranslationLanguages.SPANISH;
        }
        else if(Application.systemLanguage == SystemLanguage.English)
        {
            defaultLanguage = TranslationLanguages.ENGLISH;
        }
        else if (Application.systemLanguage == SystemLanguage.Portuguese)
        {
            defaultLanguage = TranslationLanguages.PORTUGUESE;
        }
    }

    public void SetLanguage(TranslationLanguages language)
    {
        currentLanguage = language;

        TextAsset languageTexts = Resources.Load<TextAsset>(FILE_PATH);

        string[] lines = languageTexts.text.Split('\n');
        string[] languages = lines[0].Split('\t');
        int languageIndex = (int)language;
        List<string> languageValues = new List<string>();
        for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
        {
            string[] keys = lines[lineIndex].Split('\t');
            languageValues.Add(keys[languageIndex]);
        }

        languageKeys = languageValues.ToArray();
    }

    public bool HasKey(TranslationKeys enumKey)
    {
        int keyIndex = (int)enumKey;
        return languageKeys != null && keyIndex < languageKeys.Length;
    }

    public string GetText(TranslationKeys enumKey)
    {
        int keyIndex = (int)enumKey;
        if (languageKeys != null && keyIndex < languageKeys.Length && languageKeys[keyIndex] != "")
        {
            return languageKeys[keyIndex];
        }
        else
        {
            return "<i>" + System.Enum.GetName(typeof(TranslationKeys), enumKey) + "</i>";
        }
    }

#if UNITY_EDITOR
    private static string EDITOR_FILE_PATH = "Assets/Resources/Data/Translations.tsv";
    [MenuItem("Batbelt/Translation/Generate Languages File")]
    public static void GenerateLanguagesFile()
    {
        StreamWriter writter = new StreamWriter("Assets/CodeGen/TranslationLanguages.cs");
        writter.WriteLine("public enum TranslationLanguages {");
        StreamReader reader = new StreamReader(EDITOR_FILE_PATH);
        string[] languages = reader.ReadLine().Split('\t');
        writter.WriteLine("    DEFAULT = 0,");
        for (int languageIndex = 1; languageIndex < languages.Length; ++languageIndex)
        {
            if(languages[languageIndex] != "")
            {
                writter.WriteLine("    " + languages[languageIndex].Replace(' ', '_').ToUpper() + " = " + languageIndex + ",");
            }
        }
        writter.WriteLine("}");
        writter.Close();
    }

    [MenuItem("Batbelt/Translation/Generate Keys File")]
    public static void GenerateKeysFile()
    {
        StreamWriter writter = new StreamWriter("Assets/CodeGen/TranslationKeys.cs");
        writter.WriteLine("public enum TranslationKeys {");
        StreamReader reader = new StreamReader(EDITOR_FILE_PATH);
        string[] languages = reader.ReadLine().Split('\t');
        int keyIndex = 0;
        while (!reader.EndOfStream)
        {
            string[] keys = reader.ReadLine().Split('\t');
            if (keys[0] != "")
            {
                writter.WriteLine("    " + keys[0].Replace(' ', '_').ToUpper() + " = " + keyIndex + ",");
                ++keyIndex;
            }
        }
        reader.Close();
        writter.WriteLine("}");
        writter.Close();
    }
#endif

}
