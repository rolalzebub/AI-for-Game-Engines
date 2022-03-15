using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMeshObject : MonoBehaviour
{
    MeshFilter mesh;
    public bool isWalkable;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>();
    }

    public MeshFilter GetMesh()
    {
        if (mesh == null)
            mesh = GetComponent<MeshFilter>();


        return mesh;
    }
}
