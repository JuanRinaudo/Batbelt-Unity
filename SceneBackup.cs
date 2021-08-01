#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneBackup
{
    public struct SaveBackupConfig {
        public float saveTime;
    }

    public static SaveBackupConfig config;

    public static double lastSaveTimestamp = 0;
    public static bool UpdateAdded = false;

    public const string BACKUP_EXTENSION = ".bck.unity";
    public const string SCENE_EXTENSION = ".unity";
    public const string META_EXTENSION = ".meta";

    static SceneBackup()
    {
        LoadConfig();

        if(!UpdateAdded)
        {
            EditorApplication.update += Update;
            UpdateAdded = true;
        }
    }

    static void LoadConfig()
    { 
        if(!BatUtils.LoadConfig<SaveBackupConfig>(out config, typeof(SceneBackup).Name))
        {
            config = new SaveBackupConfig();
            config.saveTime = 60;
            SaveConfig();
        }
    }

    public static void SaveConfig()
    {
        BatUtils.SaveConfig(config, typeof(SceneBackup).Name);
    }
    
    static void Update()
    {
        if(!EditorApplication.isPlaying && EditorApplication.timeSinceStartup - lastSaveTimestamp > config.saveTime)
        {
            int sceneCount = EditorSceneManager.sceneCount;
            for(int i = 0; i < sceneCount; ++i)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if(scene.path.IndexOf(BACKUP_EXTENSION) == -1 && scene.path.Length > SCENE_EXTENSION.Length)
                {
                    string scenePath = scene.path.Substring(0, scene.path.IndexOf(SCENE_EXTENSION));
                    string folder = scenePath.Substring(0, scenePath.LastIndexOf('/')) + "/Backup/";
                    BatUtils.CheckAndGenerateAssetsFolder(folder);
                    EditorSceneManager.SaveScene(scene, folder + scene.name + BACKUP_EXTENSION, true);
                }
            }
            lastSaveTimestamp = EditorApplication.timeSinceStartup;
        }
    }
}
#endif