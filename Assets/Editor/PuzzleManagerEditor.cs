using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PuzzleManager))]
public class PuzzleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PuzzleManager myScript = (PuzzleManager)target;
        if (GUILayout.Button("Add Sphere"))
        {
            myScript.AddSphere();
        }
    }
}