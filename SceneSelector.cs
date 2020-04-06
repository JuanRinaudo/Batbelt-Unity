#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneSelector : EditorWindow
{
     
    [System.Serializable]
    struct SceneSelectorData
    {
        public string name;
        public string path;
        public int index;
    }

    private static SceneSelector sceneSelector;
    [SerializeField]
    private List<SceneSelectorData> sceneData = new List<SceneSelectorData>();

    [MenuItem("Batbelt/Windows/Scene Selector")]
    public static void CreatwWindow()
    {
        sceneSelector = (SceneSelector)EditorWindow.GetWindow(typeof(SceneSelector));
        sceneSelector.RefreshScenes();

        EditorBuildSettings.sceneListChanged += sceneSelector.RefreshScenes;
    }

    private void RefreshScenes()
    {
        sceneData.Clear();

        int editorSceneIndex = 0;

        EditorBuildSettingsScene[] sceneList = EditorBuildSettings.scenes;
        for (int sceneIndex = 0; sceneIndex < sceneList.Length; ++sceneIndex)
        {
            string path = sceneList[sceneIndex].path;
            int lastSlashIndex = path.LastIndexOf("/") + 1;
            int unityIndex = path.IndexOf(".unity");
            string name = path.Substring(lastSlashIndex, unityIndex - lastSlashIndex);

            SceneSelectorData data = new SceneSelectorData();
            data.name = name;
            data.path = path;
            data.index = editorSceneIndex;

            if(sceneList[sceneIndex].enabled)
            {
                ++editorSceneIndex;
            }

            sceneData.Add(data);
        }
    }

    private void OnGUI()
    {
        for(int sceneIndex = 0; sceneIndex < sceneData.Count; ++sceneIndex)
        {
            SceneSelectorData data = sceneData[sceneIndex];
            if (GUILayout.Button(data.name))
            {
                if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(data.path, OpenSceneMode.Single);
                }
            }
        }

        GUILayout.Space(16);

        if(GUILayout.Button("Refresh"))
        {
            RefreshScenes();
        }
    }

    private void OnDestroy()
    {
        if(sceneSelector != null)
        {
            EditorBuildSettings.sceneListChanged -= sceneSelector.RefreshScenes;
        }
        sceneSelector = null;
    }

}
#endif