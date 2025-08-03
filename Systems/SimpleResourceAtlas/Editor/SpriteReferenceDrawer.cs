#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using RotaryHeart.Lib.AutoComplete;

[CustomPropertyDrawer(typeof(SpriteReference))]
public class SpriteReferenceDrawer : PropertyDrawer
{
    private static SerializedProperty pickingProperty; 

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var keyRect = new Rect(position.x, position.y, position.width - 45, position.height);
        var previewRect = new Rect(position.x + position.width - 40, position.y, 17.5f, position.height);
        var selectRect = new Rect(position.x + position.width - 17.5f, position.y, 17.5f, position.height);

        string[] keyOptions = SpriteManager.GetAllKeys();
        SerializedProperty keyProperty = property.FindPropertyRelative("key");
        keyProperty.stringValue = AutoCompleteTextField.EditorGUI.AutoCompleteTextField(keyRect, keyProperty.stringValue, keyOptions);

        Sprite sprite = SpriteManager.TryGetEditorSprite(keyProperty.stringValue);
        if(GUI.Button(previewRect, "V") && sprite != null) {
            EditorGUIUtility.PingObject(sprite);
        }

        if(GUI.Button(selectRect, "P")) {
            pickingProperty = keyProperty;
            EditorGUIUtility.ShowObjectPicker<Sprite>(sprite, false, "", -1);
        }

        if(Event.current.commandName == "ObjectSelectorClosed") {
            Sprite pickedSprite = (Sprite)EditorGUIUtility.GetObjectPickerObject();
            string pickedKey = SpriteManager.TryGetSpriteKey(pickedSprite);
            if(pickedKey != "") {
                pickingProperty.stringValue = pickedKey;
            }
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif