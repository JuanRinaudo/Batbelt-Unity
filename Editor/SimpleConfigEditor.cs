// using UnityEditor;
// using UnityEngine;
//
// [CustomEditor(typeof(SimpleConfig), true)]
// public class SimpleConfigEditor : Editor
// {
//     private string _searchText = "";
//
//     public override void OnInspectorGUI()
//     {
//         serializedObject.Update();
//
//         DrawSearchBar();
//
//         EditorGUILayout.Space(8);
//
//         DrawFields();
//
//         serializedObject.ApplyModifiedProperties();
//     }
//
//     private void DrawSearchBar()
//     {
//         using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
//         {
//             using (new EditorGUILayout.HorizontalScope())
//             {
//                 GUILayout.Label("🔍", GUILayout.Width(20));
//
//                 GUI.SetNextControlName("SearchField");
//                 _searchText = EditorGUILayout.TextField(_searchText);
//
//                 if (!string.IsNullOrEmpty(_searchText))
//                 {
//                     if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(20)))
//                     {
//                         _searchText = "";
//                         GUI.FocusControl(null);
//                     }
//                 }
//             }
//         }
//     }
//
//     private void DrawFields()
//     {
//         SerializedProperty iterator = serializedObject.GetIterator();
//         bool enterChildren = true;
//         bool hasResults = false;
//
//         using (new EditorGUILayout.VerticalScope())
//         {
//             while (iterator.NextVisible(enterChildren))
//             {
//                 enterChildren = false;
//
//                 if (iterator.propertyPath == "m_Script")
//                     continue;
//
//                 if (iterator.propertyPath == "m_Name")
//                     continue;
//
//                 if (!MatchesSearch(iterator.displayName))
//                     continue;
//
//                 EditorGUILayout.PropertyField(iterator, true);
//                 hasResults = true;
//             }
//
//             if (!hasResults && !string.IsNullOrEmpty(_searchText))
//             {
//                 EditorGUILayout.HelpBox(
//                     $"No fields matching \"{_searchText}\"",
//                     MessageType.Info
//                 );
//             }
//         }
//     }
//
//     private bool MatchesSearch(string fieldName)
//     {
//         if (string.IsNullOrEmpty(_searchText))
//             return true;
//
//         return fieldName.ToLowerInvariant()
//             .Contains(_searchText.ToLowerInvariant());
//     }
// }