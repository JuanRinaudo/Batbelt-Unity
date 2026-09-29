#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(SimpleConfig), editorForChildClasses: true)]
[CanEditMultipleObjects]
public class SimpleConfigEditor : Editor
{
    class PropertyMeta
    {
        public string PropertyPath;
        public string DisplayName;
        public string Category;
        public bool HasHeader;
    }

    SearchField _searchField;
    string _searchString = string.Empty;
    readonly List<PropertyMeta> _cachedProperties = new List<PropertyMeta>();

    void OnEnable()
    {
        _searchField = new SearchField();
        BuildPropertyCache();
    }

    void BuildPropertyCache()
    {
        _cachedProperties.Clear();
        if (target == null) return;

        Type targetType = target.GetType();
        string currentCategory = "General";

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Skip default script pointer field from caching
            if (iterator.name == "m_Script") continue;

            FieldInfo field = GetFieldInfo(targetType, iterator.name);
            bool hasHeader = false;

            if (field != null)
            {
                var headerAttr = field.GetCustomAttribute<HeaderAttribute>(true);
                if (headerAttr != null)
                {
                    currentCategory = headerAttr.header;
                    hasHeader = true;
                }
            }

            _cachedProperties.Add(new PropertyMeta
            {
                PropertyPath = iterator.name,
                DisplayName = iterator.displayName,
                Category = currentCategory,
                HasHeader = hasHeader
            });
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
        {
            SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");
            if (scriptProp != null)
                EditorGUILayout.PropertyField(scriptProp);
        }

        EditorGUILayout.Space(2);
        Rect searchRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
        _searchString = _searchField.OnGUI(searchRect, _searchString);
        EditorGUILayout.Space(4);

        bool isSearching = !string.IsNullOrWhiteSpace(_searchString);
        string lastDrawnCategory = null;
        int matchCount = 0;

        foreach (var meta in _cachedProperties)
        {
            SerializedProperty prop = serializedObject.FindProperty(meta.PropertyPath);
            if (prop == null) continue;

            if (isSearching && !MatchesFilter(prop, meta, _searchString.Trim()))
            {
                continue;
            }

            matchCount++;

            // When searching, if the field with [Header] was filtered out,
            // render a category header so you don't lose context
            if (isSearching && meta.Category != lastDrawnCategory && !meta.HasHeader && meta.Category != "General")
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField(meta.Category, EditorStyles.boldLabel);
            }

            lastDrawnCategory = meta.Category;

            EditorGUILayout.PropertyField(prop, true);
        }

        if (isSearching && matchCount == 0)
            EditorGUILayout.HelpBox($"No properties found matching '{_searchString}'.", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    bool MatchesFilter(SerializedProperty prop, PropertyMeta meta, string filter)
    {
        if (!string.IsNullOrEmpty(meta.Category) &&
            meta.Category.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        if (meta.PropertyPath.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
            meta.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        return MatchesPropertyValue(prop, filter);
    }

    bool MatchesPropertyValue(SerializedProperty prop, string filter)
    {
        return MatchesPropertyValueRecursive(prop, filter, 0);
    }

    bool MatchesPropertyValueRecursive(SerializedProperty prop, string filter, int depth)
    {
        if (depth > 6 || prop == null) return false;

        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                return prop.longValue.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Boolean:
                return prop.boolValue.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Float:
                return prop.doubleValue.ToString(CultureInfo.InvariantCulture)
                           .IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.String:
                return !string.IsNullOrEmpty(prop.stringValue) &&
                       prop.stringValue.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Enum:
                if (prop.enumValueIndex >= 0 && prop.enumDisplayNames.Length > prop.enumValueIndex)
                {
                    return prop.enumDisplayNames[prop.enumValueIndex]
                               .IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                }
                return false;

            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue != null &&
                       prop.objectReferenceValue.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Vector2:
                return prop.vector2Value.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Vector3:
                return prop.vector3Value.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Vector2Int:
                return prop.vector2IntValue.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Vector3Int:
                return prop.vector3IntValue.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

            case SerializedPropertyType.Generic:
                if (prop.isArray)
                {
                    for (int i = 0; i < prop.arraySize; i++)
                    {
                        if (MatchesPropertyValueRecursive(prop.GetArrayElementAtIndex(i), filter, depth + 1))
                            return true;
                    }
                    return false;
                }

                SerializedProperty copy = prop.Copy();
                SerializedProperty endProp = prop.GetEndProperty();

                if (copy.NextVisible(true))
                {
                    while (!SerializedProperty.EqualContents(copy, endProp))
                    {
                        if (copy.displayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;

                        if (MatchesPropertyValueRecursive(copy, filter, depth + 1))
                            return true;

                        if (!copy.NextVisible(false))
                            break;
                    }
                }
                return false;

            default:
                return false;
        }
    }

    static FieldInfo GetFieldInfo(Type type, string fieldName)
    {
        while (type != null && type != typeof(ScriptableObject) && type != typeof(UnityEngine.Object))
        {
            FieldInfo field = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (field != null) return field;
            type = type.BaseType;
        }
        return null;
    }
}
#endif