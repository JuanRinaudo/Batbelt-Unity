#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public static class MetadataCodeGen
{
    
    public const string sharedCodegenFolderPath = "../PlatazoShared/";
    public const string codegenFolderPath = "Assets/Batbelt/CodeGen/";

    [MenuItem("Batbelt/Codegen/Generate All Files")]
    public static void GenerateAllFiles()
    {
        GenerateLayerFile();
        GenerateSortingLayerFile();
        GenerateTagsFile();
        GenerateAnimatorParameterFiles();
        GenerateResourcesFile();
        GenerateScenesFile();
    }

    public static StreamWriter StartCodegenMetaWritter(string filePath, string fileName)
    {
        BatUtils.CheckAndGenerateAssetsFolder(filePath);

        StreamWriter writter = new StreamWriter(filePath + fileName + ".cs");
        writter.NewLine = Environment.NewLine;
        writter.WriteLine("// NOTE(CodeGen): This file was generated automaticaly, do not edit by hand");
        writter.WriteLine("");
        writter.WriteLine("");
        writter.WriteLine("public static class " + fileName + " {");

        return writter;
    }

    [MenuItem("Batbelt/Codegen/Generate Layer File")]
    public static void GenerateLayerFile()
    {
        StreamWriter writter = StartCodegenMetaWritter(codegenFolderPath, "UnityLayers");

        string[] layers = InternalEditorUtility.layers;
        for(int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
        {
            string layerName = layers[layerIndex];
            string formattedName = BatUtils.NormalizeKey(layerName).ToUpper();
            writter.WriteLine("    public const string " + formattedName + " = \"" + layerName + "\";");
            int layerInt = LayerMask.NameToLayer(layerName);
            writter.WriteLine("    public const int " + formattedName + "_INDEX = " + layerInt.ToString() + ";");
            int layerMask = LayerMask.GetMask(layerName);
            writter.WriteLine("    public const int " + formattedName + "_MASK = " + layerMask.ToString() + ";");
        }
        writter.WriteLine("}");
        writter.Close();

        AssetDatabase.Refresh();
    }

    [MenuItem("Batbelt/Codegen/Generate Sorting Layer File")]
    public static void GenerateSortingLayerFile()
    {
        StreamWriter writter = StartCodegenMetaWritter(codegenFolderPath, "UnitySortingLayers");

        for(int layerIndex = 0; layerIndex < SortingLayer.layers.Length; ++layerIndex)
        {
            SortingLayer sortingLayer = SortingLayer.layers[layerIndex];
            string formattedName = BatUtils.NormalizeKey(sortingLayer.name).ToUpper();
            writter.WriteLine("    public const string " + formattedName + " = \"" + sortingLayer.name + "\";");
            writter.WriteLine("    public const int " + formattedName + "_ID = " + sortingLayer.id + ";");
            writter.WriteLine("    public const int " + formattedName + "_VALUE = " + sortingLayer.value + ";");
        }
        writter.WriteLine("}");
        writter.Close();

        AssetDatabase.Refresh();
    }

    [MenuItem("Batbelt/Codegen/Generate Tag File")]
    public static void GenerateTagsFile()
    {
        StreamWriter writter = StartCodegenMetaWritter(codegenFolderPath, "UnityTags");

        string[] layers = InternalEditorUtility.tags;
        for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
        {
            writter.WriteLine("    public const string " + BatUtils.NormalizeKey(layers[layerIndex]).ToUpper() + " = \"" + layers[layerIndex] + "\";");
        }
        writter.WriteLine("}");
        writter.Close();

        AssetDatabase.Refresh();
    }

    [MenuItem("Batbelt/Codegen/Generate Animator Files")]
    public static void GenerateAnimatorParameterFiles()
    {
        string filePath = codegenFolderPath + "Animator/";

        string[] animatorsGUIDs = AssetDatabase.FindAssets("t:AnimatorController");
        for (int animatorIndex = 0; animatorIndex < animatorsGUIDs.Length; ++animatorIndex)
        {
            string path = AssetDatabase.GUIDToAssetPath(animatorsGUIDs[animatorIndex]);
            UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
            
            string filename = BatUtils.NormalizeKey(animatorController.name);
            StreamWriter writter = StartCodegenMetaWritter(filePath, filename + "Animator");

            UnityEngine.AnimatorControllerParameter[] parameters = animatorController.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; ++parameterIndex)
            {
                string animationParameterName = BatUtils.NormalizeKey(parameters[parameterIndex].name);
                writter.WriteLine("    public const string " + animationParameterName.ToUpper() + "_" + parameters[parameterIndex].type.ToString().ToUpper() + " = \"" + parameters[parameterIndex].name + "\";");
                writter.WriteLine("    public const int HASH_" + animationParameterName.ToUpper() + "_" + parameters[parameterIndex].type.ToString().ToUpper() + " = " + parameters[parameterIndex].nameHash + ";");
            }
            UnityEditor.Animations.AnimatorControllerLayer[] layers = animatorController.layers;
            for(int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
            {
                string prefix = BatUtils.NormalizeKey(layers[layerIndex].name).ToUpper();
                UnityEditor.Animations.ChildAnimatorState[] states = layers[layerIndex].stateMachine.states;
                for(int stateIndex = 0; stateIndex < states.Length; ++stateIndex)
                {
                    UnityEditor.Animations.AnimatorState currentState = states[stateIndex].state;
                    string keyName = BatUtils.NormalizeKey(currentState.name).ToUpper();
                    writter.WriteLine("    public const string " + keyName + " = \"" + currentState.name + "\";");
                    writter.WriteLine("    public const int " + keyName + "_HASH = " + currentState.nameHash + ";");
                }
            }
            writter.WriteLine("}");
            writter.Close();
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Batbelt/Codegen/Generate Resources File")]
    public static void GenerateResourcesFile()
    {
        string[] foldersGUIDs = AssetDatabase.FindAssets("t:Folder");

        int gameFolderLastSlashIndex = Application.dataPath.LastIndexOf('/') + 1;
        string gameFolder = Application.dataPath.Substring(0, gameFolderLastSlashIndex);

        Stack<string> folderStack = new Stack<string>();
        for (int folderIndex = 0; folderIndex < foldersGUIDs.Length; ++folderIndex)
        {
            string path = AssetDatabase.GUIDToAssetPath(foldersGUIDs[folderIndex]);
            int lastSlashIndex = path.LastIndexOf('/') + 1;
            string lastPart = path.Substring(lastSlashIndex);
            if (!path.StartsWith("Packages/") && lastPart == "Resources")
            {
                string directoryPath = gameFolder + path;
                folderStack.Push(directoryPath);
            }
        }
        
        StreamWriter writter = StartCodegenMetaWritter(codegenFolderPath, "UnityResources");

        while (folderStack.Count > 0)
        {
            string fullPath = folderStack.Pop();

            string[] fileEntries = Directory.GetFileSystemEntries(fullPath);
            foreach (string entryFullPath in fileEntries)
            {
                int fullPathCropIndex = entryFullPath.IndexOf("/Assets") + 8; // #NOTE (Juan): Added X to index because it goes to the next slash;
                int cropStartIndex = entryFullPath.IndexOf("/Resources") + 11; // #NOTE (Juan): Added X to index because it goes to the next slash;

                string keyPath = entryFullPath.Substring(fullPathCropIndex);
                string resourcePath = entryFullPath.Substring(cropStartIndex);

                if (!resourcePath.EndsWith(".meta"))
                {
                    FileAttributes attributes = File.GetAttributes(entryFullPath);

                    if (attributes.HasFlag(FileAttributes.Directory))
                    {
                        // #TODO (Juan): If this is slow, check for a faster/better way to do this
                        string key = BatUtils.NormalizeKey(keyPath).ToUpper();
                        string value = resourcePath.Replace('\\', '/');

                        folderStack.Push(entryFullPath);
                        // #NOTE (Juan): A final / is added to the prefix for easy path append
                        writter.WriteLine("    public const string PREFIX_" + key + " = \"" + value + "/\";");
                    }
                    else
                    {
                        int extensionIndex = keyPath.LastIndexOf(".");
                        string value = keyPath.Substring(0, extensionIndex).Replace('\\', '/');
                        // #TODO (Juan): If this is slow, check for a faster/better way to do this
                        string key = BatUtils.NormalizeKey(value);

                        extensionIndex = resourcePath.LastIndexOf(".");
                        string fileExtension = resourcePath.Substring(extensionIndex);
                        value = resourcePath.Substring(0, extensionIndex).Replace('\\', '/');

                        string keyPrefix = "";
                        // #NOTE (Juan): Supported resource types are added in a case by case basis
                        if (fileExtension == ".prefab")
                        {
                            keyPrefix = "PREFAB_";
                        }
                        else if (fileExtension == ".fbx")
                        {
                            keyPrefix = "MODEL_";
                        }
                        else if (fileExtension == ".json")
                        {
                            keyPrefix = "JSON_";
                        }
                        else if (fileExtension == ".asset")
                        {
                            keyPrefix = "ASSET_";
                        }
                        else if (fileExtension == ".ttf" || fileExtension == ".oft")
                        {
                            keyPrefix = "FONT_";
                        }
                        else if (fileExtension == ".zip")
                        {
                            keyPrefix = "ZIP_";
                        }
                        else if (fileExtension == ".csv")
                        {
                            keyPrefix = "CSV_";
                        }
                        else if (fileExtension == ".png" || fileExtension == ".jpg")
                        {
                            keyPrefix = "TEXTURE_";
                        }
                        else if (fileExtension == ".ogg" || fileExtension == ".wav" || fileExtension == ".mp3")
                        {
                            keyPrefix = "SOUND_";
                        }
                        //else // #NOTE (Juan): This is used to check if there are new formats that need to be added
                        //{
                        //    Debug.Log("File not added: " + entryFullPath);
                        //}
                        writter.WriteLine("    public const string " + keyPrefix + key.ToUpper() + " = \"" + value + "\";");
                    }
                }
            }
        }

        writter.WriteLine("}");
        writter.Close();

        AssetDatabase.Refresh();
    }

    [MenuItem("Batbelt/Codegen/Generate Scenes File")]
    public static void GenerateScenesFile()
    {
        StreamWriter writter = StartCodegenMetaWritter(codegenFolderPath, "UnityScenes");

        string[] scenesGUID = AssetDatabase.FindAssets("t: Scene");
        for (int sceneIndex = 0; sceneIndex < scenesGUID.Length; ++sceneIndex)
        {
            string path = AssetDatabase.GUIDToAssetPath(scenesGUID[sceneIndex]);

            int cropStartIndex = path.IndexOf("/Assets") + 8; // #NOTE (Juan): Added X to index because it goes to the next slash;
            string resourcePath = path.Substring(cropStartIndex);

            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if(scene != null)
            {
                writter.WriteLine("    public const string " + BatUtils.NormalizeKey(path).ToUpper() + " = \"" + scene.name + "\";");
            }
        }
        writter.WriteLine("}");
        writter.Close();

        AssetDatabase.Refresh();
    }

}
#endif