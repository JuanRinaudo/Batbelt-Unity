#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneBackup
{
    public static float saveTime = 60;
    public static double lastSaveTimestamp = 0;
    public static bool UpdateAdded = false;

    public const string BACKUP_EXTENSION = ".bck";
    public const string SCENE_EXTENSION = ".unity";

    static SceneBackup()
    {        
        //bool updateAdded = false;
        //Delegate[] delegates = EditorApplication.update.GetInvocationList();
        //for(int i = 0; i < delegates.Length; ++i)
        //{
        //    delegates
        //}
        
        if(!UpdateAdded)
        {
            EditorApplication.update += Update;
            UpdateAdded = true;
        }
    }
    
    static void Update()
    {
        if(!EditorApplication.isPlaying && EditorApplication.timeSinceStartup - lastSaveTimestamp > saveTime)
        {
            int sceneCount = EditorSceneManager.sceneCount;
            for(int i = 0; i < sceneCount; ++i)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if(scene.path.IndexOf(BACKUP_EXTENSION) == -1)
                {
                    string scenePath = scene.path.Substring(0, scene.path.IndexOf(SCENE_EXTENSION));
                    EditorSceneManager.SaveScene(scene, scenePath + BACKUP_EXTENSION + SCENE_EXTENSION, true);
                }
            }
            lastSaveTimestamp = EditorApplication.timeSinceStartup;
        }
    }
}
#endif