#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;

public class SimpleBuilder : EditorWindow
{
    
    private static SimpleBuilderConfig config;

    private static SimpleBuilder simpleBuilder;

    private static ScriptingImplementation savedScriptingImplementation;
    private static AndroidArchitecture savedAndroidArchitecture;

    [MenuItem("Batbelt/Windows/Simple Build")]
    public static void CreateWindow()
    {
        simpleBuilder = (SimpleBuilder)GetWindow(typeof(SimpleBuilder));
    }

    private static string[] GetConfiguredScenes()
    {
        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        string[] scenes = new string[buildScenes.Length];
        for (int i = 0; i < buildScenes.Length; i++)
        {
            scenes[i] = buildScenes[i].path;
        }
        return scenes;
    }
    
    private static void BuildAndroid(bool quickBuild)
    {
        string filename = $"{(quickBuild ? "Quick-" : "")}Platazo-{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode}";
        string path = EditorUtility.SaveFilePanel("Choose the folder (DO NOT CHANGE FILENAME)", "", filename, EditorUserBuildSettings.buildAppBundle ? "aab" : "apk");
        
        PlayerSettings.Android.keystorePass = "qdyK&!R%K7FU";
        PlayerSettings.Android.keyaliasPass = "qdyK&!R%K7FU";

        EditorUserBuildSettings.buildAppBundle = true;
        if(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP)
        {
            EditorUserBuildSettings.androidCreateSymbolsZip = true;
        }

        if(path == null || path == "") {
            Debug.LogError("File path is empty");
            return;
        }

        string[] levels = GetConfiguredScenes();
        BuildOptions options = new BuildOptions();

        AndroidArchitecture targetArchitectures = PlayerSettings.Android.targetArchitectures;
        
        if(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP)
        {
            if(targetArchitectures == (AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64))
            {
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
                BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, BuildOptions.None);

                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, BuildOptions.None);

                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            } 

            BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, BuildOptions.AutoRunPlayer);

            string pathFolder = Path.GetDirectoryName(path);
            string symbolsPath = $"{pathFolder}{Path.DirectorySeparatorChar}{filename}-{PlayerSettings.bundleVersion}-v{PlayerSettings.Android.bundleVersionCode}.symbols.zip";
            if(File.Exists(symbolsPath))
            {
                using (FileStream zipToOpen = new FileStream(symbolsPath, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        var entries = archive.Entries;
                        for(int i = 0; i < entries.Count; ++i)
                        {
                            var entry = entries[i];
                            if(!entry.Name.EndsWith("/")) {
                                if(entry.FullName.Contains(".dbg."))
                                {
                                    entry.Delete();
                                    i--;
                                }
                                else if(entry.FullName.Contains(".sym."))
                                {
                                    var newEntry = archive.CreateEntry(entry.FullName.Replace(".sym", ""));
                                    using (var openEntry = entry.Open())
                                    using (var openNewEntry = newEntry.Open())
                                        openEntry.CopyTo(openNewEntry);
                                    entry.Delete();
                                    i--;
                                }
                            }
                        }
                    }
                }
                
                string newSymbolsPath = $"{pathFolder}{Path.DirectorySeparatorChar}{filename}.symbols.zip";
                File.Copy(symbolsPath, newSymbolsPath, true);
                File.Delete(symbolsPath);
            }
        }
        else
        {            
            BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, BuildOptions.AutoRunPlayer);
        }
    }

    private static void SaveCurrentConfig()
    {
        savedScriptingImplementation = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
        savedAndroidArchitecture = PlayerSettings.Android.targetArchitectures;
    }

    private static void RestoreSavedConfig()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, savedScriptingImplementation);
        PlayerSettings.Android.targetArchitectures = savedAndroidArchitecture;
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        
        EditorUserBuildSettings.buildAppBundle = EditorGUILayout.Toggle("Build App Bundle", EditorUserBuildSettings.buildAppBundle);

        PlayerSettings.bundleVersion = EditorGUILayout.TextField("Version", PlayerSettings.bundleVersion);
        PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("Bundle Version Code", PlayerSettings.Android.bundleVersionCode);

        if(EditorGUI.EndChangeCheck())
        {

        }

        GUILayout.Space(16);

        if(GUILayout.Button("Quick Mono Build"))
        {
            SaveCurrentConfig();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;

            BuildAndroid(true);

            RestoreSavedConfig();
        }

        GUILayout.Space(16);
        
        if(GUILayout.Button("Quick IL2CPP Build (x86)"))
        {
            SaveCurrentConfig();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;

            BuildAndroid(true);

            RestoreSavedConfig();
        }

        if(GUILayout.Button("Quick IL2CPP Build (x64)"))
        {
            SaveCurrentConfig();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            BuildAndroid(true);

            RestoreSavedConfig();
        }

        if(GUILayout.Button("Release IL2CPP Build"))
        {
            SaveCurrentConfig();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            BuildAndroid(false);

            RestoreSavedConfig();
        }
    }

    public struct SimpleBuilderConfig
    {

    }

}
#endif