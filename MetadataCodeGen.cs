#if UNITY_EDITOR
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
            writter.WriteLine("    public const string " + layerName.Replace(' ', '_').ToUpper() + " = \"" + layerName + "\";");
            int layerInt = LayerMask.NameToLayer(layerName);
            writter.WriteLine("    public const int " + layers[layerIndex].Replace(' ', '_').ToUpper() + "_INDEX = " + layerInt.ToString() + ";");
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

    [MenuItem("Batbelt/Codegen/Generate Animator Parameter Files")]
    public static void GenerateAnimatorParameterFiles()
    {
        string filePath = "Assets/CodeGen/AnimatorParameters/";
        BatUtils.CheckAndGenerateAssetsFolder(filePath);

        string[] animatorsGUIDs = AssetDatabase.FindAssets("t:AnimatorController");
        for (int animatorIndex = 0; animatorIndex < animatorsGUIDs.Length; ++animatorIndex)
        {
            string path = AssetDatabase.GUIDToAssetPath(animatorsGUIDs[animatorIndex]);
            UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);

            StreamWriter writter = new StreamWriter(filePath + animatorController.name + "Parameters.cs");
            writter.WriteLine("// NOTE(Batbelt): This file was generated automaticaly, do not edit by hand\n\n");
            writter.WriteLine("public class " + animatorController.name + "Parameters { ");
            UnityEngine.AnimatorControllerParameter[] parameters = animatorController.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; ++parameterIndex)
            {
                writter.WriteLine("    public const string " + parameters[parameterIndex].name.Replace(' ', '_').ToUpper() + "_" + parameters[parameterIndex].type.ToString().ToUpper() + " = \"" + parameters[parameterIndex].name + "\";");
            }
            writter.WriteLine("}");
            writter.Close();
        }

        AssetDatabase.Refresh();
    }

}
#endif