using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(Wall), true)]
public class WallEditor : Editor 
{
    public void OnSceneGUI()
    {
        var anchors = serializedObject.FindProperty("anchors");

        if (anchors == null) return;

        var wall = target as Wall;

        EditorGUI.BeginChangeCheck();
        for (int i=0, l=anchors.arraySize; i<l; i++)
        {
            var anchor = anchors.GetArrayElementAtIndex(i);
            var worldPosition = wall.transform.TransformPoint(anchor.vector3Value);
            Handles.Label(worldPosition, $"Anchor {i}");
            anchor.vector3Value = wall.transform.InverseTransformPoint(Handles.PositionHandle(worldPosition, Quaternion.identity));
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            wall.UpdateMesh();
        }
    }
}
