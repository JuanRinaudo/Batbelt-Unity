﻿#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class MetadataCodeGen
{

    [MenuItem("Batbelt/Codegen/Generate Layer File")]
    public static void GenerateLayerFile()
    {
        string filePath = "Assets/CodeGen/";
        BatUtils.CheckAndGenerateAssetsFolder(filePath);
        StreamWriter writter = new StreamWriter(filePath + "UnityLayers.cs");
        writter.WriteLine("// NOTE(Batbelt): This file was generated automaticaly, do not edit by hand\n\n");
        writter.WriteLine("public class UnityLayers {");
        string[] layers = InternalEditorUtility.layers;
        for(int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
        {
            string layerName = layers[layerIndex];
            string formattedName = layerName.Replace(' ', '_').ToUpper();
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

    [MenuItem("Batbelt/Codegen/Generate Tag File")]
    public static void GenerateTagsFile()
    {
        string filePath = "Assets/CodeGen/";
        BatUtils.CheckAndGenerateAssetsFolder(filePath);
        StreamWriter writter = new StreamWriter(filePath + "UnityTags.cs");
        writter.WriteLine("// NOTE(Batbelt): This file was generated automaticaly, do not edit by hand\n\n");
        writter.WriteLine("public class UnityTags {");
        string[] layers = InternalEditorUtility.tags;
        for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
        {
            writter.WriteLine("    public const string " + layers[layerIndex].Replace(' ', '_').ToUpper() + " = \"" + layers[layerIndex] + "\";");
        }
        writter.WriteLine("}");
        writter.Close();

        AssetDatabase.Refresh();
    }

    [MenuItem("Batbelt/Codegen/Generate Animator Files")]
    public static void GenerateAnimatorParameterFiles()
    {
        string filePath = "Assets/CodeGen/Animator/";
        BatUtils.CheckAndGenerateAssetsFolder(filePath);

        string[] animatorsGUIDs = AssetDatabase.FindAssets("t:AnimatorController");
        for (int animatorIndex = 0; animatorIndex < animatorsGUIDs.Length; ++animatorIndex)
        {
            string path = AssetDatabase.GUIDToAssetPath(animatorsGUIDs[animatorIndex]);
            UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);

            StreamWriter writter = new StreamWriter(filePath + animatorController.name + "Animator.cs");
            writter.WriteLine("// NOTE(Batbelt): This file was generated automaticaly, do not edit by hand\n\n");
            writter.WriteLine("public class " + animatorController.name + "Animator { ");
            UnityEngine.AnimatorControllerParameter[] parameters = animatorController.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; ++parameterIndex)
            {
                writter.WriteLine("    public const string " + parameters[parameterIndex].name.Replace(' ', '_').ToUpper() + "_" + parameters[parameterIndex].type.ToString().ToUpper() + " = \"" + parameters[parameterIndex].name + "\";");
            }
            UnityEditor.Animations.AnimatorControllerLayer[] layers = animatorController.layers;
            for(int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
            {
                string prefix = layers[layerIndex].name.Replace(" ", "_").ToUpper();
                UnityEditor.Animations.ChildAnimatorState[] states = layers[layerIndex].stateMachine.states;
                for(int stateIndex = 0; stateIndex < states.Length; ++stateIndex)
                {
                    UnityEditor.Animations.AnimatorState currentState = states[stateIndex].state;
                    string keyName = currentState.name.Replace(' ', '_').ToUpper();
                    writter.WriteLine("    public const string " + keyName + " = \"" + currentState.name + "\";");
                    writter.WriteLine("    public const int " + keyName + "_HASH = " + currentState.nameHash + ";");
                }
            }
            writter.WriteLine("}");
            writter.Close();
        }

        AssetDatabase.Refresh();
    }

    private static string NormalizeKey(string key)
    {
        return key.Replace(' ', '_').Replace('-', '_').Replace('[', '_').Replace(']', '_').Replace('/', '_').Replace('\\', '_').Replace('.', '_').Replace('&', '_').ToUpper();
    }

    [MenuItem("Batbelt/Codegen/Generate Resources File")]
    public static void GenerateResourcesFile()
    {
        string filePath = "Assets/CodeGen/";
        BatUtils.CheckAndGenerateAssetsFolder(filePath);

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

        StreamWriter writter = new StreamWriter(filePath + "UnityResources.cs");
        writter.WriteLine("// #NOTE(StationUtils): This file was generated automaticaly, do not edit by hand\n\n");
        writter.WriteLine("public class UnityResources {");

        while (folderStack.Count > 0)
        {
            string fullPath = folderStack.Pop();

            string[] fileEntries = Directory.GetFileSystemEntries(fullPath);
            foreach (string entryFullPath in fileEntries)
            {
                int cropStartIndex = entryFullPath.IndexOf("/Resources") + 11; // #NOTE (Juan): Added 11 to index because it goes to the next slash;

                string resourcePath = entryFullPath.Substring(cropStartIndex);

                if (!resourcePath.EndsWith(".meta"))
                {
                    FileAttributes attributes = File.GetAttributes(entryFullPath);

                    if (attributes.HasFlag(FileAttributes.Directory))
                    {
                        // #TODO (Juan): If this is slow, check for a faster/better way to do this
                        string key = NormalizeKey(resourcePath);
                        string value = resourcePath.Replace('\\', '/');

                        folderStack.Push(entryFullPath);
                        // #NOTE (Juan): A final / is added to the prefix for easy path append
                        writter.WriteLine("    public const string PREFIX_" + key + " = \"" + value + "/\";");
                    }
                    else
                    {
                        int extensionIndex = resourcePath.LastIndexOf(".");
                        string fileExtension = resourcePath.Substring(extensionIndex);

                        string value = resourcePath.Substring(0, extensionIndex).Replace('\\', '/');
                        // #TODO (Juan): If this is slow, check for a faster/better way to do this
                        string key = NormalizeKey(value);

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
                        writter.WriteLine("    public const string " + keyPrefix + key + " = \"" + value + "\";");
                    }
                }
            }
        }
        //for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
        //{
        //    writter.WriteLine("    public const string " + layers[layerIndex].Replace(' ', '_').ToUpper() + " = \"" + layers[layerIndex] + "\";");
        //}

        writter.WriteLine("}");
        writter.Close();

        AssetDatabase.Refresh();
    }

}
#endif