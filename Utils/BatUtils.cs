#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class BatUtils
{

    private const string SAVE_PATH_PREFIX = "Assets/BatBelt/Config/";
    private const string JSON_EXTENSION = ".json";

    public static bool LoadConfig<T>(out T output, string configName)
    {
        string filepath = SAVE_PATH_PREFIX + configName + JSON_EXTENSION;
        
        if(File.Exists(filepath))
        {
            StreamReader reader = new StreamReader(filepath);
            output = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            reader.Close();
        }
        else
        {
            output = default(T);
            return false;
        }

        return true;
    }

    public static void SaveConfig<T>(T config, string configName)
    {
        CheckAndGenerateAssetsFolder(SAVE_PATH_PREFIX);
        StreamWriter writter = new StreamWriter(SAVE_PATH_PREFIX + configName + JSON_EXTENSION);        
        writter.Write(JsonConvert.SerializeObject(config));
        writter.Close();
    }

    public static string NormalizeKey(string key)
    {
        return (char.IsNumber(key[0]) ? "_" : "") + key.Replace(' ', '_').Replace('-', '_').Replace('[', '_').Replace(']', '_').Replace('(', '_').Replace(')', '_').Replace('/', '_').Replace('\\', '_').Replace('.', '_').Replace('&', '_').ToUpper();
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

    public static T[] GetAtPath<T>(string path) where T:Object
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

}
#endif