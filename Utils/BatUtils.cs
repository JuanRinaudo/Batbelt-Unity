using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;

#if NEWTONSOFT_ENABLED
using Newtonsoft.Json;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

public static class BatUtils
{

    private const string SAVE_PATH_PREFIX = "Assets/BatBelt/Config/";
    private const string LOAD_PATH_RUNTIME_PREFIX = "Config/";
    private const string SAVE_PATH_RUNTIME_PREFIX = "Assets/BatBelt/Resources/Config/";
    private const string JSON_EXTENSION = ".json";

    private const string BATBELT_RESOURCES_PATH = "Assets/Batbelt/Resources/Batbelt/";

    private readonly static char[] DEFINE_SEPARATOR = new char[] { ';', ',', ' ' };

    static T Deserialize<T>(string jsonText)
    {  
#if NEWTONSOFT_ENABLED
        return JsonConvert.DeserializeObject<T>(jsonText);
#else
        return JsonUtility.FromJson<T>(jsonText);
#endif
    }

    static string Serialize<T>(T target)
    {  
#if NEWTONSOFT_ENABLED
        return JsonConvert.SerializeObject(target);
#else
        return JsonUtility.ToJson(target);
#endif
    }

    public static bool LoadConfig<T>(out T output)
    {
        string filepath = LOAD_PATH_RUNTIME_PREFIX + typeof(T).Name;

        TextAsset serializedJson = Resources.Load<TextAsset>(filepath);
        if(serializedJson != null)
        {
            string jsonText = serializedJson.text;
            output = Deserialize<T>(jsonText);
        }
        else
        {
            output = default(T);
            return false;
        }

        return true;
    }

    public static bool LoadPersistentData<T>(out T output)
    {
        return LoadPersistentData(out output, typeof(T).Name);
    }

    public static bool LoadPersistentData<T>(out T output, string name)
    {
        string filepath = GetUniquePersistentDataPath() + "/" + name + JSON_EXTENSION;
        if(File.Exists(filepath))
        {
            StreamReader reader = new StreamReader(filepath);
            output = Deserialize<T>(reader.ReadToEnd());
            reader.Close();
        }
        else
        {
            output = default(T);
            return false;
        }

        return true;
    }

    public static string GetUniquePersistentDataPath()
    {        
        string dataPath = Application.persistentDataPath;
#if UNITY_EDITOR
        if (Application.dataPath.Contains("_Clone"))
        {
            dataPath += "_clone";
        }
        CheckAndGenerateFolder(dataPath + "/");
#endif
        return dataPath;
    }
    
    public static void DeletePersistentData<T>(T input)
    {
        DeletePersistentData(typeof(T).Name);
    }

    public static void DeletePersistentData<T>(string name)
    {
        string filename = GetUniquePersistentDataPath() + "/" + name + JSON_EXTENSION;
        if(File.Exists(filename))
        {
            File.Delete(filename);
        }
    }
    
    public static void SavePersistentData<T>(T input)
    {
        SavePersistentData(input, typeof(T).Name);
    }

    public static void SavePersistentData<T>(T input, string name)
    {
        StreamWriter writter = new StreamWriter(GetUniquePersistentDataPath() + "/" + name + JSON_EXTENSION);
        writter.Write(Serialize(input));
        writter.Close();
    }

    public static void SavePersistentData(string text, string filename)
    {
        StreamWriter writter = new StreamWriter(GetUniquePersistentDataPath() + "/" + filename);
        writter.Write(text);
        writter.Close();
    }

    public static void SavePersistentData(byte[] bytes, string filename)
    {
        File.WriteAllBytes(GetUniquePersistentDataPath() + "/" + filename, bytes);
    }

#if UNITY_EDITOR
    public static bool LoadConfig<T>(out T output, bool runtimeConfig = true)
    {
        string pathPrefix = runtimeConfig ? SAVE_PATH_RUNTIME_PREFIX : SAVE_PATH_PREFIX;

        string filepath = pathPrefix + typeof(T).Name + JSON_EXTENSION;
        if(File.Exists(filepath))
        {
            StreamReader reader = new StreamReader(File.OpenRead(filepath));
            output = Deserialize<T>(reader.ReadToEnd());
            reader.Close();
        }
        else
        {
            output = default(T);
            return false;
        }

        return true;
    }

    public static void SaveConfig<T>(T config, bool runtimeConfig = false)
    {
        string pathPrefix = runtimeConfig ? SAVE_PATH_RUNTIME_PREFIX : SAVE_PATH_PREFIX;

        CheckAndGenerateAssetsFolder(pathPrefix);
        StreamWriter writter = new StreamWriter(pathPrefix + typeof(T).Name + JSON_EXTENSION);     
        writter.Write(Serialize(config));
        writter.Close();

        if(runtimeConfig)
        {
            AssetDatabase.Refresh();
        }
    }

    public static string NormalizeKey(string key)
    {
        return (char.IsNumber(key[0]) ? "_" : "") + key.Replace(' ', '_').Replace('-', '_').Replace('[', '_').Replace(']', '_').Replace('(', '_').Replace(')', '_').Replace('/', '_').Replace('\\', '_').Replace('.', '_').Replace('&', '_').Replace('$', '_').Replace('#', '_').Replace('!', '_');
    }

    public static bool CheckAndGenerateAssetsFolder(string path)
    {
        string[] pathParts = path.Split('/');

        int startIndex = 0;
        string runningPath = "";
        if (pathParts[0] == "Assets")
        {
            startIndex = 1;
            runningPath = "Assets";
        }

        for(int pathIndex = startIndex; pathIndex < pathParts.Length; ++pathIndex)
        {
            if(pathParts[pathIndex] != "")
            {
                if (!AssetDatabase.IsValidFolder(runningPath + "/" + pathParts[pathIndex]))
                {
                    AssetDatabase.CreateFolder(runningPath, pathParts[pathIndex]);
                }

                runningPath += "/" + pathParts[pathIndex];
            }
        }

        return true;
    }
    
    public static bool CheckAndGenerateFolder(string path)
    {
        string[] pathParts = path.Split('/');

        int startIndex = 1;
        string runningPath = pathParts[0];

        for(int pathIndex = startIndex; pathIndex < pathParts.Length; ++pathIndex)
        {
            if(pathParts[pathIndex] != "")
            {
                if (!Directory.Exists(runningPath + "/" + pathParts[pathIndex]))
                {
                    Directory.CreateDirectory(runningPath + "/" + pathParts[pathIndex]);
                }

                runningPath += "/" + pathParts[pathIndex];
            }
        }

        return true;
    }

    public static T[] GetAtPath<T>(string path) where T:UnityEngine.Object
    {
        List<T> list = new List<T>();
        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        foreach (string fileName in fileEntries)
        {
            string localPath = "Assets/" + path;

            int index = fileName.LastIndexOf("\\");
            if (index > 0)
            {
                localPath += fileName.Substring(index);
            }

            T asset = AssetDatabase.LoadAssetAtPath<T>(localPath);

            if (asset != null)
            {
                list.Add(asset);
            }
        }

        return list.ToArray();
    }

    public static void PlayClip(AudioClip clip)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayClip", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null
        );
        method.Invoke(null, new object[] { clip });
    }

    public static void StopAllClips()
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "StopAllClips", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { }, null
        );
        method.Invoke(null, new object[] { });
    }

    public static int RunProcess(string fileName, string arguments, string workingDirectory, bool waitForExit = true)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.WorkingDirectory = workingDirectory;

        int exitCode = -1;
        try
        {
            process.Start();

            if (waitForExit)
            {
                process.WaitForExit();
            }
            else
            {
                exitCode = 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("RunProcess error" + e.ToString());
        }
        finally
        {
            if (waitForExit)
            {
                exitCode = process.ExitCode;
                process.Dispose();
            }
        }

        return exitCode;
    }

    public static Color GetBackgroundColor()
    {
        return EditorGUIUtility.isProSkin ? (Color)new Color32(56, 56, 56, 255) : (Color)new Color32(194, 194, 194, 255);
    }

    private static Color DeserializeColor(string lines)
    {
        string[] parts = lines.Split(';');
        float r = float.Parse(parts[1], CultureInfo.InvariantCulture);
        float g = float.Parse(parts[2], CultureInfo.InvariantCulture);
        float b = float.Parse(parts[3], CultureInfo.InvariantCulture);
        float a = float.Parse(parts[4], CultureInfo.InvariantCulture);
        return new Color(r, g, b, a);
    }

    public static Color GetPlaymodeTint()
    {
        return DeserializeColor(EditorPrefs.GetString("Playmode tint", "Playmode tint;1;1;1;1"));
    }

    public static List<string> GetDefinitionList()
    {
#if UNITY_ANDROID
        string[] scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android).Split(DEFINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
#elif UNITY_IOS
        string[] scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.iOS).Split(DEFINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
#elif UNITY_STANDALONE
        string[] scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Split(DEFINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
#elif UNITY_WEBGL
        string[] scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.WebGL).Split(DEFINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
#endif
        List<string> definitionList = new List<string>(scriptingDefines);

        return definitionList;
    }

    public static void SetDefinition(ref List<string> currentDefinitions, string newDefinition)
    {
        if (!currentDefinitions.Contains(newDefinition))
        {
            currentDefinitions.Add(newDefinition);
        }
    }

    public static void RemoveDefinition(ref List<string> currentDefinitions, string newDefinition)
    {
        if (currentDefinitions.Contains(newDefinition))
        {
            currentDefinitions.Remove(newDefinition);
        }
    }

    public static void ApplyDefinitions(List<string> currentDefinitions)
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, String.Join(";", currentDefinitions.ToArray()));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, String.Join(";", currentDefinitions.ToArray()));
    }

    public static WebClient DownloadTSVFromPublicSheet(string filepath, string fileId, string tabId = "0", AsyncCompletedEventHandler onComplete = null)
    {
        string finalExportURL = $"https://docs.google.com/spreadsheets/d/{fileId}/export?gid={tabId}&format=tsv";
        var client = new WebClient();

        Debug.Log("Download started, URL: " + finalExportURL + " to " + filepath);

        if(onComplete != null)
        {
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(onComplete);
        }
        client.DownloadFileAsync(new Uri(finalExportURL), filepath);

        return client;
    }

    public static string GetTempAssetPath()
    {
        CheckAndGenerateAssetsFolder(BATBELT_RESOURCES_PATH);
        return BATBELT_RESOURCES_PATH + "Temp.txt";
    }

    public static string LoadTempFile(bool deleteFile = false)
    {
        string filepath = Path.Combine(Application.dataPath, "../") + BATBELT_RESOURCES_PATH + "Temp.txt";

        StreamReader streamReader = new StreamReader(filepath);
        string text = streamReader.ReadToEnd();
        streamReader.Close();
        
        if (deleteFile)
        {
            File.Delete(filepath);
        }
        
        return text;
    }
#endif

    public static string ChangingDots(int dotCount = 3)
    {
        return new string('.', Mathf.FloorToInt((float)Time.time) % (dotCount + 1));
    }
    
    public static IEnumerator AnimatedDotsCoroutine(TMPro.TextMeshPro text, string textFormat = "{0}", int dotCount = 3, float dotSpeed = 1)
    {
        var wait = new WaitForEndOfFrame();

        dotCount += 1;

        float time = 0;
        var lastDots = -1;
        while(true)
        {
            var currentDots =  Mathf.FloorToInt((float)time) % dotCount;
            if(currentDots != lastDots) { text.text = string.Format(textFormat, new string('.', currentDots)); }
            lastDots = currentDots;
            time = (time + Time.deltaTime * dotSpeed) % dotCount;
            yield return wait;
        }
    }

    public static IEnumerator AnimatedDotsCoroutine(TMPro.TextMeshProUGUI text, string textFormat = "{0}", int dotCount = 3, float dotSpeed = 1)
    {
        var wait = new WaitForEndOfFrame();

        dotCount += 1;

        float time = 0;
        var lastDots = -1;
        while(true)
        {
            var currentDots =  Mathf.FloorToInt((float)time) % dotCount;
            if(currentDots != lastDots) { text.text = string.Format(textFormat, new string('.', currentDots)); }
            lastDots = currentDots;
            time = (time + Time.deltaTime * dotSpeed) % dotCount;
            yield return wait;
        }
    }

    public static IEnumerator AnimatedDotsCoroutine(TMPro.TextMeshProUGUI text, string textFormat, string prefix, string postfix, int dotCount = 3, float dotSpeed = 1)
    {
        var wait = new WaitForEndOfFrame();

        dotCount += 1;

        float time = 0;
        var lastDots = -1;
        while(true)
        {
            var currentDots =  Mathf.FloorToInt((float)time) % dotCount;
            if(currentDots != lastDots) { text.text = prefix + string.Format(textFormat, new string('.', currentDots)) + postfix; }
            lastDots = currentDots;
            time = (time + Time.deltaTime * dotSpeed) % dotCount;
            yield return wait;
        }
    }

}