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

    Bounds sceneBounds;
    List<Vector3> sceneGridVerts;


    public void VoxelizeSceneByCombiningMeshes()
    {
        //if (allSceneMeshes == null || allSceneMeshes.Count < 1)
        allSceneMeshes = new List<SceneMeshObject>(FindObjectsOfType<SceneMeshObject>());

        Mesh sceneMesh = new Mesh();
        CombineInstance[] meshesToCombine = new CombineInstance[allSceneMeshes.Count];
        for(int i = 0; i < allSceneMeshes.Count; i++)
        {
            meshesToCombine[i].mesh = allSceneMeshes[i].GetMesh().sharedMesh;
            meshesToCombine[i].transform = allSceneMeshes[i].GetMesh().transform.localToWorldMatrix;

        }
        sceneMesh.CombineMeshes(meshesToCombine);

        Heightfield sceneHeightfield = CreateHeightFieldGrid(sceneMesh.bounds);
        sceneHeightfield.CheckHeightfieldAgainstMesh(sceneMesh);

        sceneBounds = sceneMesh.bounds;
    }

    Heightfield CreateHeightFieldGrid(Bounds sceneBounds)
    {
        sceneGridVerts = new List<Vector3>();

        Bounds voxelBound = new Bounds();
        voxelBound.size = new Vector3(XZCellSize, YCellSize, XZCellSize);

        int XCellsCount = Mathf.CeilToInt(sceneBounds.size.x / XZCellSize);
        int ZCellsCount = Mathf.CeilToInt(sceneBounds.size.z / XZCellSize);
        int YCellsCount = Mathf.CeilToInt(sceneBounds.size.y / YCellSize);

        Heightfield heightfield = new Heightfield(XZCellSize, YCellSize, walkableSlopeAngle);
        heightfield.verts = new List<List<List<Vector3>>>();
        heightfield.gridRows = XCellsCount + 1;
        heightfield.gridColumns = YCellsCount + 1;


        Vector3 startPos = new Vector3();
        bool firstRun = false;
        //create vertices to represent the heightfield grid
        for (int xIndex = 0; xIndex <= heightfield.gridRows; xIndex++)
        {
            heightfield.verts.Add(new List<List<Vector3>>());

            for (int yIndex = 0; yIndex <= heightfield.gridColumns; yIndex++)
            {
                heightfield.verts[xIndex].Add(new List<Vector3>());

                for (int zIndex = 0; zIndex <= heightfield.gridRows; zIndex++)
                {
                    if (!firstRun)
                    {
                        startPos = sceneBounds.min;
                        heightfield.verts[0][0].Add(sceneBounds.min);
                        sceneGridVerts.Add(sceneBounds.min);
                        firstRun = true;

                        Debug.Log(sceneBounds.min);
                    }
                    else
                    {
                        //create a grid for voxelizing mesh
                        Vector3 newVert = new Vector3()
                        {
                            x = startPos.x + (xIndex * XZCellSize),
                            y = startPos.y + (yIndex * YCellSize),
                            z = startPos.z + (zIndex * XZCellSize)
                        };
                        heightfield.verts[xIndex][yIndex].Add(newVert);

                        sceneGridVerts.Add(newVert);
                    }
                }

            }

        }

        return heightfield;
    }

    void OnDrawGizmos()
    {
        if (sceneBounds == null)
            return;

        Gizmos.DrawWireCube(sceneBounds.center, sceneBounds.size);


    }
}

