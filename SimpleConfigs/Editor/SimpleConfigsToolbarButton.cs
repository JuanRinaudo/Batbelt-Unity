#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

public class SimpleConfigsToolbarButton
{
    [MainToolbarElement("Configs/SimpleConfigsDropdown", defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement CreateConfigsToolbarButton()
    {
        Texture2D icon = EditorGUIUtility.IconContent("d_ScriptableObject Icon")?.image as Texture2D ?? EditorGUIUtility.IconContent("ScriptableObject Icon")?.image as Texture2D;

        var content = new MainToolbarContent("Configs", icon, "Open SimpleConfig assets");
        return new MainToolbarButton(content, OpenDropdownMenu);
    }

    static void OpenDropdownMenu()
    {
        var menu = new GenericMenu();

        string[] configGUIDs = AssetDatabase.FindAssets("t:SimpleConfig");
        var configsByType = new Dictionary<string, List<SimpleConfig>>();

        foreach (string guid in configGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var config = AssetDatabase.LoadAssetAtPath<SimpleConfig>(path);
            if (config == null) continue;

            string typeName = config.GetType().Name;
            if (!configsByType.TryGetValue(typeName, out var list))
            {
                list = new List<SimpleConfig>();
                configsByType[typeName] = list;
            }
            list.Add(config);
        }

        if (configsByType.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("No SimpleConfig assets found"));
            menu.ShowAsContext();
            return;
        }

        var sortedGroups = configsByType.OrderBy(k => k.Key);

        foreach (var group in sortedGroups)
        {
            List<SimpleConfig> items = group.Value;
            items.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

            if (items.Count == 1)
            {
                // Only one of this type: put directly in the root menu without submenus
                SimpleConfig config = items[0];
                menu.AddItem(new GUIContent(config.name), false, () => SelectConfig(config));
            }
            else
            {
                // Multiple of this type: create a submenu folder
                foreach (SimpleConfig config in items)
                {
                    string menuPath = $"{group.Key}/{config.name}";
                    menu.AddItem(new GUIContent(menuPath), false, () => SelectConfig(config));
                }
            }
        }

        menu.ShowAsContext();
    }

    static void SelectConfig(SimpleConfig config)
    {
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }
}
#endif