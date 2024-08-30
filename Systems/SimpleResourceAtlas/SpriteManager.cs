using System;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
#endif

[Serializable]
public struct MappedSprite
{
	public string path;
	public int index;

	public MappedSprite(string path, int index) {
		this.path = path;
		this.index = index;
	}
}

public class SpriteManager : MonoBehaviour
{

	private static SpriteManager instance;
	public static SpriteManager Instance
	{
		get {
			if(instance == null) { Initialize(); }
			return instance;
		}
	}

	private SpriteMap spriteMap;

    private const string SPRITE_MAP_FILE = "SpriteMap.asset";
    private const string CODEGEN_KEYS_NAME = "SpriteMapKeys.cs";
    private const string SPRITE_MAP_PATH = "SpriteManager/SpriteMap";

	private static void Initialize()
	{
		Type spriteManagerType = typeof(SpriteManager);
		new GameObject(spriteManagerType.Name, spriteManagerType);
	}

	private void Awake()
	{
		if(instance == null) {
			instance = this;

			// NOTE(Juan): Load sprite mapper
			spriteMap = Resources.Load<SpriteMap>(SPRITE_MAP_PATH);
		}
		else {
			Destroy(gameObject);
		}
	}

	public Sprite GetSprite (string key)
	{
		key = key.ToLower();
		if(spriteMap.mapping.ContainsKey(key)) {
			MappedSprite spriteData = spriteMap.mapping[key];
			if(spriteData.index >= 0) {
				Sprite[] atlasSprites = Resources.LoadAll<Sprite>(spriteData.path);
				return atlasSprites[spriteData.index];
			}
			else {
				Sprite sprite = Resources.Load<Sprite>(spriteData.path);
				return sprite;
			}
		}
		return null;
	}

#if UNITY_EDITOR
	private static SpriteMap editorLoadedMap;
	public static string[] GetAllKeys()
	{
		if(editorLoadedMap == null) {
			editorLoadedMap = Resources.Load<SpriteMap>(SPRITE_MAP_PATH);
		}

		if(editorLoadedMap != null) {
			string[] keys = new string[editorLoadedMap.mapping.Count];
			editorLoadedMap.mapping.Keys.CopyTo(keys, 0);

			return keys;
		}

		return null;
	}

	public static string TryGetSpriteKey(Sprite sprite)
	{
		if(sprite != null) {
			if(editorLoadedMap.mapping.ContainsKey(sprite.name)) {
				return sprite.name;
			}
			else {
				Debug.LogError("Sprite " + sprite.name + " not found");
			}
		}

		return "";
	}
	
	public static Sprite TryGetEditorSprite(string key)
	{
		if(editorLoadedMap == null) {
			editorLoadedMap = Resources.Load<SpriteMap>(SPRITE_MAP_PATH);
		}

		if(editorLoadedMap != null && editorLoadedMap.mapping.ContainsKey(key)) {
			return Resources.Load<Sprite>(editorLoadedMap.mapping[key].path);
		}

		return null;
	}

    [MenuItem("Batbelt/SpriteManager/Rebuild Keys")]
    public static void RebuildSpriteManagerKeys()
    {
        Debug.Log("Rebuildi keys");

        string path = GetAssetPath() + SPRITE_MAP_FILE;
        SpriteMap spriteMap = (SpriteMap)AssetDatabase.LoadAssetAtPath(path, typeof(SpriteMap));
        if (spriteMap == null)
        {
            Debug.Log("No sprite map file found, creating a new one");
            spriteMap = new SpriteMap();
            AssetDatabase.CreateAsset(spriteMap, path);
        }

        string keyGenPath = GetCodeGenPath() + CODEGEN_KEYS_NAME;
        StreamWriter keyGenWriter = new StreamWriter(keyGenPath, false);
        keyGenWriter.WriteLine("// NOTE(Batbelt): This file is generated automaticaly, do not touch by hand");
        keyGenWriter.WriteLine("");
        keyGenWriter.WriteLine("public class SpriteMapKeys\n{\n");

        if (spriteMap.mapping == null)
        {
            spriteMap.mapping = new MappedSpritesDictionary();
        }
        else
        {
            spriteMap.mapping.Clear();
        }

        int resourceLenght = "/Resources/".Length;

        string resourcePath = "";
        string spriteName = "";
        string[] allAssetPaths;
        string assetPath;
        try
        {
            allAssetPaths = AssetDatabase.FindAssets("t: Texture2D");
            for (int pathIndex = 0; pathIndex < allAssetPaths.Length; ++pathIndex)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(allAssetPaths[pathIndex]);
                if (assetPath.IndexOf(".png") > 0 || assetPath.IndexOf(".jpg") > 0)
                {
                    TextureImporter asset = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                    if (asset.textureType == TextureImporterType.Sprite)
                    {
                        int resourcesTextIndex = assetPath.IndexOf("/Resources/");
                        int packageTextIndex = assetPath.IndexOf("Packages/");
                        if (resourcesTextIndex > -1 && packageTextIndex != 0)
                        {
                            resourcePath = Path.ChangeExtension(assetPath, "");
                            resourcePath = resourcePath.Substring(resourcesTextIndex + resourceLenght, resourcePath.Length - resourcesTextIndex - resourceLenght - 1);
                            if (asset.spriteImportMode == SpriteImportMode.Single)
                            {
                                Sprite sprite = Resources.Load<Sprite>(resourcePath);
                                if(sprite != null)
                                {
                                    spriteName = ExtractAssetName(sprite.name.ToLower());
                                    spriteMap.mapping.Add(spriteName, new MappedSprite(resourcePath, -1));
                                    WriteToGeneratedKeys(keyGenWriter, spriteName);
                                }
                            }
                            else if (asset.spriteImportMode == SpriteImportMode.Multiple)
                            {
                                Sprite[] atlasSprites = Resources.LoadAll<Sprite>(resourcePath);
                                if (atlasSprites != null)
                                {
                                    for (int spriteIndex = 0; spriteIndex < atlasSprites.Length; ++spriteIndex)
                                    {
                                        spriteName = ExtractAssetName(atlasSprites[spriteIndex].name.ToLower());
                                        spriteMap.mapping.Add(spriteName, new MappedSprite(resourcePath, spriteIndex));
                                        WriteToGeneratedKeys(keyGenWriter, spriteName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (ArgumentException exception)
        {
            Debug.LogError($"Error on sprite: {spriteName}");
            Debug.LogError($"Message: {exception.Message}");
        }

        keyGenWriter.WriteLine("}");
        keyGenWriter.Close();

        EditorUtility.SetDirty(spriteMap);

        Debug.Log("Keys rebuilt");
    }

	private static void WriteToGeneratedKeys(StreamWriter writer, string key) {
        string normalizedKey = key.Replace("-", "_").Replace(" ", "_").Replace("[", string.Empty).Replace("]", string.Empty).ToUpper();
        writer.WriteLine("    public const string " + normalizedKey + " = \"" + key + "\";");
	}

    private static string ExtractAssetName(string name)
    {
        int blockStartIndex = name.IndexOf("[");
        int blockEndIndex = name.IndexOf("]");
        if (blockStartIndex != -1 && blockEndIndex != -1) { return name.Substring(blockStartIndex + 1, blockEndIndex - 1); }
        else { return name; }
    }

    [MenuItem("Batbelt/SpriteManager/View SpriteMap")]
    private static void ViewSpriteMap()
    {
        UnityEngine.Object[] spriteMaps = Resources.FindObjectsOfTypeAll(typeof(SpriteMap));
        Selection.objects = spriteMaps;
        Selection.SetActiveObjectWithContext(spriteMaps[0], spriteMaps[0]);
    }

    private static string GetCodeGenPath()
    {
        string folderPath = "Assets/Batbelt/CodeGen/SpriteManager/";
        BatUtils.CheckAndGenerateAssetsFolder(folderPath);
        return folderPath;
    }

    private static string GetAssetPath()
    {
        string folderPath = "Assets/Batbelt/Resources/SpriteManager/";
        BatUtils.CheckAndGenerateAssetsFolder(folderPath);
        return folderPath;
    }
#endif

}