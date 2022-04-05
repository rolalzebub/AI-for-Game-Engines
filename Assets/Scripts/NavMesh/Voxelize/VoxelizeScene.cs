using MeshVoxelizerProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelizeScene : MonoBehaviour
{
    List<SceneMeshObject> allSceneMeshes;
    public int gridResolution;
    public float XZCellSize;
    public float YCellSize;
    public float walkableSlopeAngle = 45.0f;

    Heightfield sceneField;

    bool sceneVoxed = false;

    public DebugHeightSpanDrawMode debugMode;

    public void VoxelizeSceneByCombiningMeshes()
    {
        allSceneMeshes = new List<SceneMeshObject>(FindObjectsOfType<SceneMeshObject>());

        Mesh sceneMesh = new Mesh();
        CombineInstance[] meshesToCombine = new CombineInstance[allSceneMeshes.Count];
        for(int i = 0; i < allSceneMeshes.Count; i++)
        {
            meshesToCombine[i].mesh = allSceneMeshes[i].GetMesh().sharedMesh;
            meshesToCombine[i].transform = allSceneMeshes[i].GetMesh().transform.localToWorldMatrix;

        }
        sceneMesh.CombineMeshes(meshesToCombine);

        sceneField = new Heightfield(XZCellSize, YCellSize, walkableSlopeAngle);

        sceneField.CreateHeightFieldGrid(sceneMesh.bounds, walkableSlopeAngle);
        sceneField.CheckHeightfieldAgainstMesh(sceneMesh);

        sceneVoxed = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!sceneVoxed)
            return;

        if (sceneField == null)
            return;
            
        var grid = sceneField.GetHeightSpans();

        if (grid == null)
            return;
            
        for(int xIndex = 0; xIndex < sceneField.gridRows - 1; xIndex++)
        {
            for(int zIndex = 0; zIndex < sceneField.gridRows - 1; zIndex++)
            {

                foreach(var item in grid[xIndex, zIndex])
                {
                    bool drawCube = false;
                    switch (debugMode)
                    {
                        case DebugHeightSpanDrawMode.Both:
                            { 
                                if (item.type == Heightfield.HeightFieldVoxelType.Solid)
                                {
                                    Gizmos.color = Color.red;
                                }
                                else
                                {
                                    Gizmos.color = Color.green;
                                }
                                drawCube = true;
                                break;
                            }
                        case DebugHeightSpanDrawMode.ClosedSpans:
                            {
                                Gizmos.color = Color.red;
                                if (item.type == Heightfield.HeightFieldVoxelType.Solid)
                                {
                                    drawCube = true;
                                }
                                break;
                            }
                        case DebugHeightSpanDrawMode.OpenSpans:
                            {
                                Gizmos.color = Color.green;
                                if (item.type == Heightfield.HeightFieldVoxelType.Open)
                                {
                                    drawCube = true;
                                }
                                break;
                            }
                    }
                    if (drawCube)
                    {
                        int cubeVertSize = item.spanVoxels.Count;

                        Vector3 min = item.spanVoxels[0].VoxelBounds.Min, max = Vector3.zero;

                        foreach (var voxel in item.spanVoxels)
                        {
                            min = Vector3.Min(min, voxel.VoxelBounds.Min);
                            max = Vector3.Max(max, voxel.VoxelBounds.Max);
                        }

                        AABB spanBounds = new AABB(min, max);

                        Gizmos.DrawWireCube(spanBounds.Center, max - min);
                    }
                }
            }
        }
    }

    public enum DebugHeightSpanDrawMode
    {
        ClosedSpans,
        OpenSpans,
        Both
    };
}

