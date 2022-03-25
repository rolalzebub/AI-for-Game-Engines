using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBoundingBoxVoxelizer : MonoBehaviour
{

    public float AgentHeight;
    public float AgentWidth;

    MeshFilter meshToVoxelize;

    public Mesh VoxelizeMesh()
    {
        Mesh voxelizedMesh = new Mesh();
        Bounds meshBounds = meshToVoxelize.sharedMesh.bounds;

        //make array to contain the 2D grid of voxels
        int gridX = Mathf.CeilToInt(meshBounds.size.x / AgentWidth);
        int gridY = Mathf.CeilToInt(meshBounds.size.y / AgentHeight);

        Bounds[,,] voxelGrid = new Bounds[gridX, gridY, gridX];




        return voxelizedMesh;
    }
}
