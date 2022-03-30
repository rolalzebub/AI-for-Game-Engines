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
            //for(int yIndex = 0; yIndex < sceneField.gridColumns - 1; yIndex++)
            {
                for(int zIndex = 0; zIndex < sceneField.gridRows - 1; zIndex++)
                {
                    foreach(var item in grid[xIndex, zIndex])
                    {
                        foreach (var voxel in item.spanVoxels)
                        {
                            if (item.type == Heightfield.HeightFieldVoxelType.Solid)
                            {
                                Gizmos.color = Color.red;

                                var cube = voxel.VoxelBounds;
                                Gizmos.DrawWireCube(cube.Center, cube.Max - cube.Min);
                            }
                            else
                            {
                                Gizmos.color = Color.green;

                                var cube = voxel.VoxelBounds;
                                Gizmos.DrawWireCube(cube.Center, cube.Max - cube.Min);
                            }
                        }
                        
                    }
                }
            }
        }
       
    }

}

