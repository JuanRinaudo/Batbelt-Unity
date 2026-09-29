using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(WeightedList<>), true)]
public class WeightedListDrawer : PropertyDrawer
{
    readonly Dictionary<string, ReorderableList> listCache = new Dictionary<string, ReorderableList>();
    const float BoxPadding = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ReorderableList list = GetOrCreateList(property);
        return list.GetHeight() + (BoxPadding * 2f);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        property.serializedObject.Update();

        GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

        Rect listRect = new Rect(position.x + BoxPadding, position.y + BoxPadding, position.width - (BoxPadding * 2f), position.height - (BoxPadding * 2f));
        ReorderableList list = GetOrCreateList(property);
        list.DoList(listRect);

        property.serializedObject.ApplyModifiedProperties();
        EditorGUI.EndProperty();
    }

    ReorderableList GetOrCreateList(SerializedProperty property)
    {
        string key = property.propertyPath;
        SerializedProperty itemsProp = property.FindPropertyRelative("items");

        if (listCache.TryGetValue(key, out ReorderableList existingList) && existingList.serializedProperty.serializedObject == property.serializedObject)
        {
            existingList.serializedProperty = itemsProp;
            return existingList;
        }

        var list = new ReorderableList(property.serializedObject, itemsProp, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

        list.drawHeaderCallback = (Rect rect) =>
        {
            Rect titleRect = new Rect(rect.x, rect.y, rect.width * 0.6f, EditorGUIUtility.singleLineHeight);
            Rect countRect = new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            GUIStyle countStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

            EditorGUI.LabelField(titleRect, property.displayName, titleStyle);
            EditorGUI.LabelField(countRect, $"{list.serializedProperty.arraySize} Elements", countStyle);

            float columnY = rect.y + EditorGUIUtility.singleLineHeight + 2f;
            float handlePadding = 18f;
            float availableWidth = rect.width - handlePadding;
            float weightWidth = Mathf.Max(availableWidth * 0.35f, 60f);
            float valueWidth = availableWidth - weightWidth - 6f;

            Rect weightHeaderRect = new Rect(rect.x + handlePadding, columnY, weightWidth, EditorGUIUtility.singleLineHeight);
            Rect valueHeaderRect = new Rect(weightHeaderRect.xMax + 6f, columnY, valueWidth, EditorGUIUtility.singleLineHeight);

            GUIStyle colHeaderStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontStyle = FontStyle.Bold };

            EditorGUI.LabelField(weightHeaderRect, "Weight", colHeaderStyle);
            EditorGUI.LabelField(valueHeaderRect, "Value", colHeaderStyle);
        };

        list.headerHeight = (EditorGUIUtility.singleLineHeight * 2) + 6f;

        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            if (index < 0 || index >= list.serializedProperty.arraySize) return;

            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty weightProp = element.FindPropertyRelative("weight");
            SerializedProperty valueProp = element.FindPropertyRelative("value");

            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            rect.y += 2f;
            float singleLine = EditorGUIUtility.singleLineHeight;
            float spacing = 6f;
            float weightWidth = Mathf.Max(rect.width * 0.35f, 60f);
            float valueWidth = rect.width - weightWidth - spacing;

            Rect weightRect = new Rect(rect.x, rect.y, weightWidth, singleLine);
            Rect valueRect = new Rect(weightRect.xMax + spacing, rect.y, valueWidth, singleLine);

            EditorGUI.PropertyField(weightRect, weightProp, GUIContent.none);
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none, true);

            EditorGUI.indentLevel = originalIndent;
        };

        list.onAddCallback = (l) =>
        {
            int index = l.serializedProperty.arraySize;
            l.serializedProperty.InsertArrayElementAtIndex(index);
            SerializedProperty newItem = l.serializedProperty.GetArrayElementAtIndex(index);
            newItem.FindPropertyRelative("weight").floatValue = 1f;
        };

        listCache[key] = list;
        return list;
    }
}