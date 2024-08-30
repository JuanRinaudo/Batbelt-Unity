#if UNITY_EDITOR
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SimpleTranslationsEditor : EditorWindow
{
    
    private static SimpleTranslationsConfig config;
    private static SimpleTranslationsEditorConfig editorConfig;

    public bool viewTexts;
    public int selectedLanguage;

    private static Vector2 textsScrollPosition = new Vector2();
    private static WebClient downloadClient;

    [MenuItem("Batbelt/Translation/Config")]
    public static void CreateWindow()
    {
        SimpleTranslationsEditor simpleTranslations = (SimpleTranslationsEditor)GetWindow(typeof(SimpleTranslationsEditor));

        LoadConfig();
    }

    private void OnGUI()
    {
        if(!editorConfig.inited || !config.inited)
        {
            LoadConfig();
        }

        EditorGUILayout.LabelField("Simple translations");
        
        string[] languages = null;
        Dictionary<string, string> values = new Dictionary<string, string>();
        if(downloadClient == null) {
            string translationText = SimpleTranslations.GetTranslationsFile();

            if (translationText != null && translationText != "")
            {
                string[] lines = translationText.Split('\n');
                languages = lines[0].Trim(SimpleTranslations.charsToTrim).Split('\t');

                for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
                {
                    string[] lineValues = lines[lineIndex].Split('\t');
                    values.Add(lineValues[0], lineValues[selectedLanguage]);
                }
            }
        }

        EditorGUILayout.Space(8);

        if(languages != null)
        {
            selectedLanguage = EditorGUILayout.Popup("Language", selectedLanguage, languages);

            viewTexts = EditorGUILayout.Foldout(viewTexts, "View texts");
            if(viewTexts)
            {
                textsScrollPosition = EditorGUILayout.BeginScrollView(textsScrollPosition, EditorStyles.helpBox);

                foreach(var key in values.Keys)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField(key);
                    EditorGUILayout.LabelField(values[key]);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }
        else
        {
            if(downloadClient == null) {
                EditorGUILayout.LabelField("No language founds, make sure a translation file is generated");
            }
            else {
                Repaint();
                EditorGUILayout.LabelField("DOWNLOADING" + BatUtils.ChangingDots());
            }
        }

        EditorGUILayout.Space(8);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Generate data from gdrive sheet");
        editorConfig.gDriveSheetFileId = EditorGUILayout.TextField("File ID", editorConfig.gDriveSheetFileId);
        editorConfig.tabId = EditorGUILayout.TextField("Tab ID", editorConfig.tabId);

        EditorGUILayout.Space(8);
        
        config.filename = EditorGUILayout.TextField("File name", config.filename);

        config.useResources = EditorGUILayout.Toggle("Use Resources", config.useResources);
        if(!config.useResources)
        {
            editorConfig.downloadFileFolder = EditorGUILayout.TextField("Download file folder", editorConfig.downloadFileFolder);
        }

        if (EditorGUI.EndChangeCheck())
        {
            SaveConfig();
        }

        EditorGUILayout.Space(8);

        GUI.enabled = downloadClient == null || editorConfig.gDriveSheetFileId != "";
        if(downloadClient != null)
        {
            if(GUILayout.Button("Cancel"))
            {
                downloadClient.CancelAsync();
                downloadClient.Dispose();
                downloadClient = null;
            }
        }
        else {
            if(GUILayout.Button("Download"))
            {
                DownloadFile();
            }
        }
        if(GUILayout.Button("Generate Language File"))
        {
            GenerateLanguagesFile();
        }
        if(GUILayout.Button("Generate Keys File"))
        {
            GenerateKeysFile();
        }
        GUI.enabled = true;
    }

    private void DownloadFile()
    {
        if(config.filename.Contains("/") || config.filename.Contains("\\")) {
            Debug.LogError("File name can't contain a / or \\");
            return;
        }

        var downloadFolder = "";
        if(config.useResources)
        {
            downloadFolder = "Assets/Batbelt/Resources/Batbelt/";
        }
        else
        {
            downloadFolder = editorConfig.downloadFileFolder;
        }

        if(!(downloadFolder.EndsWith("/") || downloadFolder.EndsWith("\\"))) {
            downloadFolder += "/";
        }
        
        BatUtils.CheckAndGenerateAssetsFolder(downloadFolder);
        downloadClient = BatUtils.DownloadTSVFromPublicSheet(downloadFolder + config.filename, editorConfig.gDriveSheetFileId, editorConfig.tabId, (object sender, AsyncCompletedEventArgs e) =>
        {
            if (e.Cancelled) {
                Debug.LogError("File download cancelled");
                return;
            }
            if (e.Error != null) {
                Debug.LogError(e.Error.ToString());
                return;
            }

            Debug.Log("Download completed");
                
            downloadClient = null;
            AssetDatabase.Refresh();
        });
    }

    private static void LoadConfig()
    {
        if (!BatUtils.LoadConfig(out config, true))
        {
            config = new SimpleTranslationsConfig();
            config.inited = true;
            config.filename = "";
            config.useResources = false;
            SaveConfig();
        }

        if (!BatUtils.LoadConfig(out editorConfig, false))
        {
            editorConfig = new SimpleTranslationsEditorConfig();
            editorConfig.inited = true;
            editorConfig.gDriveSheetFileId = "";
            editorConfig.tabId = "0";
            editorConfig.downloadFileFolder = "";
            SaveConfig();
        }
    }

    private static void SaveConfig()
    {
        BatUtils.SaveConfig(config, true);
        BatUtils.SaveConfig(editorConfig);
    }
    
    [MenuItem("Batbelt/Translation/Generate Languages File")]
    public static void GenerateLanguagesFile()
    {
        string folderPath = MetadataCodeGen.codegenFolderPath;
        BatUtils.CheckAndGenerateAssetsFolder(MetadataCodeGen.codegenFolderPath);

        StreamWriter writter = new StreamWriter($"{folderPath}TranslationLanguages.cs");
        writter.WriteLine("public class TranslationLanguages {");

        string translationText = SimpleTranslations.GetTranslationsFile();
        string[] lines = translationText.Split('\n');
        string[] languages = lines[0].Trim(SimpleTranslations.charsToTrim).Split('\t');

        for (int languageIndex = 0; languageIndex < languages.Length; ++languageIndex)
        {
            if(languages[languageIndex] != "")
            {
                writter.WriteLine("\tpublic static string " + BatUtils.NormalizeKey(languages[languageIndex]).ToUpper() + " = \"" + languages[languageIndex] + "\";");
            }
        }

        writter.WriteLine("}");
        writter.Close();
    }

    [MenuItem("Batbelt/Translation/Generate Keys File")]
    public static void GenerateKeysFile()
    {
        string folderPath = MetadataCodeGen.codegenFolderPath;
        BatUtils.CheckAndGenerateAssetsFolder(folderPath);

        StreamWriter writter = new StreamWriter($"{folderPath}TranslationKeys.cs");
        writter.WriteLine("public class TranslationKeys {");
        
        string translationText = SimpleTranslations.GetTranslationsFile();
        string[] lines = translationText.Split('\n');

        for (int lineIndex = 1; lineIndex < lines.Length; ++lineIndex)
        {
            string[] currentLine = lines[lineIndex].Trim(SimpleTranslations.charsToTrim).Split('\t');
            if (currentLine[0] != "")
            {
                writter.WriteLine("\tpublic static string " + BatUtils.NormalizeKey(currentLine[0]).ToUpper() + " = \"" + currentLine[0] + "\";");
            }
        }

        writter.WriteLine("}");
        writter.Close();
    }

}
#endif