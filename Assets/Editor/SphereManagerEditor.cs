using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SphereManager))]
public class SphereManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SphereManager myScript = (SphereManager)target;
        if (GUILayout.Button("Add Point"))
        {
            myScript.AddPoint();
        }
    }
}