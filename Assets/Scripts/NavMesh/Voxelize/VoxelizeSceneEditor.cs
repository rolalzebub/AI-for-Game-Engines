using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.ComponentModel;

[CustomEditor(typeof(VoxelizeScene))]
public class VoxelizeSceneEditor : Editor
{
    BackgroundWorker bg;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Voxelize Scene"))
        {
            (target as VoxelizeScene).VoxelizeSceneByCombiningMeshes();
        }
    }
}