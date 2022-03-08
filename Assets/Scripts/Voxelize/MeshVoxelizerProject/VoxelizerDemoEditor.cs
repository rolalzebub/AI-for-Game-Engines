using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MeshVoxelizerProject;

[CustomEditor(typeof(VoxelizerDemo))]
public class VoxelizerDemoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Voxelize"))
        {
            ((VoxelizerDemo)target).DoYaThing();
        }
    }
}
