#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveTransforms : EditorWindow
{

    public List<Vector3> positions = new List<Vector3>();
    public List<Quaternion> rotations = new List<Quaternion>();
    public List<Vector3> scales = new List<Vector3>();

    [MenuItem("Batbelt/Save Transforms")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SaveTransforms));
    }

    public void OnGUI()
    {
        GUILayout.Label("Save Transforms");

        GUILayout.Space(16);

        Transform selectedTransform = Selection.activeTransform;
        if (selectedTransform != null)
        {
            GUILayout.Label("Current transform : " + selectedTransform.name);

            if (GUILayout.Button("Copy Children Transforms"))
            {
                positions.Clear();
                rotations.Clear();
                scales.Clear();

                for (int i = 0; i < selectedTransform.childCount; ++i)
                {
                    Transform child = selectedTransform.GetChild(i);
                    positions.Add(child.localPosition);
                    rotations.Add(child.localRotation);
                    scales.Add(child.localScale);
                }
            }
            GUI.enabled = positions.Count > 0;
            if (GUILayout.Button("Paste Children Transforms"))
            {
                Undo.SetCurrentGroupName("Paste Childrens");
                for (int i = 0; i < selectedTransform.childCount; ++i)
                {
                    Transform child = selectedTransform.GetChild(i);
                    Undo.RecordObject(child, "Child Transform");
                    child.localPosition = positions[i];
                    child.localRotation = rotations[i];
                    child.localScale = scales[i];
                }
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            GUILayout.Space(16);
#if !UNITY_MIN_MODULES
            GUILayout.Label("Utils");
            if (GUILayout.Button("Mesh Colliders Convex Enable"))
            {
                SetConvexMeshColliders(selectedTransform, true);
            }
            if (GUILayout.Button("Mesh Colliders Convex Disable"))
            {
                SetConvexMeshColliders(selectedTransform, false);
            }
#endif
        }
    }
#if !UNITY_MIN_MODULES
    private void SetConvexMeshColliders(Transform transform, bool value)
    {
        MeshCollider[] colliders = transform.GetComponentsInChildren<MeshCollider>(true);
        for (int i = 0; i < colliders.Length; ++i)
        {
            colliders[i].convex = value;
        }
    }
#endif
}
#endif