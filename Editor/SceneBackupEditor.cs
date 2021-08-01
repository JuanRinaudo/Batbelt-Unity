#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneBackupEditor : EditorWindow
{

    [MenuItem("Batbelt/Config/Scene Backup")]
    public static void CreatwWindow()
    {
        SceneBackupEditor sceneBackup = (SceneBackupEditor)EditorWindow.GetWindow(typeof(SceneBackupEditor));
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        
        SceneBackup.config.saveTime = EditorGUILayout.FloatField("Save Time", SceneBackup.config.saveTime);
        
        if(EditorGUI.EndChangeCheck())
        {
            SceneBackup.SaveConfig();
        }
    }

}
#endif