#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class AssetDatabaseUtils
{
    public static List<T> GetAssetsOfType<T>() where T : Object
    {
        var foundAssetsGUIDs = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var assets = new List<T>();

        foreach (var assetPath in foundAssetsGUIDs)
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(assetPath));
            assets.Add(asset);
        }

        return assets;
    }
}
#endif