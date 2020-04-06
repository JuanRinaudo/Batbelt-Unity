#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BatUtils
{

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