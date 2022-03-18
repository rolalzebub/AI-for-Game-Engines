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
    void Start()
    {
        allSceneMeshes = new List<SceneMeshObject>(FindObjectsOfType<SceneMeshObject>());
        //ReplaceSceneMeshesWithVoxels();
    }

    public void ReplaceSceneMeshesWithVoxels()
    {
        if(allSceneMeshes == null)
            allSceneMeshes = new List<SceneMeshObject>(FindObjectsOfType<SceneMeshObject>());

        MeshVoxelizer.Init(gridResolution, gridResolution, gridResolution);
        Vector3 min = Vector3.one * float.PositiveInfinity;
        Vector3 max = Vector3.one * float.NegativeInfinity;
        foreach (var mesh in allSceneMeshes)
        {
            var meshRef = mesh.GetMesh();

            meshRef.sharedMesh = MeshVoxelizer.Voxelize(meshRef.sharedMesh.vertices, meshRef.sharedMesh.triangles,
                new Box3(meshRef.sharedMesh.bounds.min, meshRef.sharedMesh.bounds.max));


            min.x = Mathf.Min(min.x, meshRef.transform.TransformPoint(meshRef.sharedMesh.bounds.min).x);
            min.y = Mathf.Min(min.y, meshRef.transform.TransformPoint(meshRef.sharedMesh.bounds.min).y);
            min.z = Mathf.Min(min.z, meshRef.transform.TransformPoint(meshRef.sharedMesh.bounds.min).z);
            
            max.x = Mathf.Max(max.x, meshRef.transform.TransformPoint(meshRef.sharedMesh.bounds.max).x);
            max.y = Mathf.Max(max.y, meshRef.transform.TransformPoint(meshRef.sharedMesh.bounds.max).y);
            max.z = Mathf.Max(max.z, meshRef.transform.TransformPoint(meshRef.sharedMesh.bounds.max).z);
            //meshRef.gameObject.SetActive(false);
        }

        Box3 sceneAABB = new Box3(min, max);
        Debug.Log(sceneAABB.Min);
        Debug.Log(sceneAABB.Max);

    }

    public void VoxelizeSceneByCombiningMeshes()
    {
        if (allSceneMeshes == null || allSceneMeshes.Count < 1)
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

    }

    Heightfield CreateHeightFieldGrid(Bounds sceneBounds)
    {
        //set up heightfield mesh for debug rendering
        
        Bounds voxelBound = new Bounds();
        voxelBound.size = new Vector3(XZCellSize, YCellSize, XZCellSize);


        int XCellsCount = Mathf.CeilToInt(sceneBounds.size.x / XZCellSize);
        int ZCellsCount = Mathf.CeilToInt(sceneBounds.size.z / XZCellSize);
        int YCellsCount = Mathf.CeilToInt(sceneBounds.size.y / YCellSize);

        Heightfield heightfield = new Heightfield(XZCellSize, YCellSize, walkableSlopeAngle);
        heightfield.verts = new List<Vector3>();
        heightfield.gridRows = XCellsCount;
        heightfield.gridColumns = YCellsCount;

        for (int i = 0; i < XCellsCount - 1; i+=2)
        {
            for (int j = 0; j < ZCellsCount - 1; j+=2)
            {
                for(int k = 0; k < YCellsCount; k+=2)
                {
                    //create a grid for "voxelizing" mesh
                    Vector3 newVert = new Vector3()
                    {
                        x = i * XZCellSize,
                        y = k * YCellSize,
                        z = j * XZCellSize
                    };

                    Vector3 newVert2 = new Vector3()
                    {
                        x = i * XZCellSize,
                        y = (k + 1) * YCellSize,
                        z = j * XZCellSize
                    };

                    Vector3 newVert3 = new Vector3()
                    {
                        x = (i + 1) * XZCellSize,
                        y = (k + 1) * YCellSize,
                        z = j * XZCellSize
                    };

                    Vector3 newVert4 = new Vector3()
                    {
                        x = (i + 1) * XZCellSize,
                        y = k * YCellSize,
                        z = j * XZCellSize
                    };

                    heightfield.verts.Add(newVert);
                    heightfield.verts.Add(newVert2);
                    heightfield.verts.Add(newVert3);
                    heightfield.verts.Add(newVert4);

                    Vector3 newVert5 = new Vector3()
                    {
                        x = i * XZCellSize,
                        y = k * YCellSize,
                        z = (j + 1) * XZCellSize
                    };

                    Vector3 newVert6 = new Vector3()
                    {
                        x = i * XZCellSize,
                        y = (k + 1) * YCellSize,
                        z = (j + 1) * XZCellSize
                    };

                    Vector3 newVert7 = new Vector3()
                    {
                        x = (i + 1) * XZCellSize,
                        y = (k + 1) * YCellSize,
                        z = (j + 1) * XZCellSize
                    };

                    Vector3 newVert8 = new Vector3()
                    {
                        x = (i + 1) * XZCellSize,
                        y = k * YCellSize,
                        z = (j + 1) * XZCellSize
                    };
                    heightfield.verts.Add(newVert5);
                    heightfield.verts.Add(newVert6);
                    heightfield.verts.Add(newVert7);
                    heightfield.verts.Add(newVert8);

                }
            }
        }

        return heightfield;

        
    }

    void CheckVoxelTriangleIntersection()
    {

    }

    #region Add Faces To Quads
    void AddFrontFaceQuad(ref List<int> newTrisForGrid, int firstVert)
    {
        //front face of quad
        newTrisForGrid.Add(0 + firstVert);
        newTrisForGrid.Add(1 + firstVert);
        newTrisForGrid.Add(2 + firstVert);

        newTrisForGrid.Add(2 + firstVert);
        newTrisForGrid.Add(3 + firstVert);
        newTrisForGrid.Add(0 + firstVert);
    }

    void AddRightFaceQuad(ref List<int> newTrisForGrid, int firstVert)
    {
        //right face of quad
        newTrisForGrid.Add(3 + firstVert);
        newTrisForGrid.Add(2 + firstVert);
        newTrisForGrid.Add(6 + firstVert);

        newTrisForGrid.Add(6 + firstVert);
        newTrisForGrid.Add(7 + firstVert);
        newTrisForGrid.Add(3 + firstVert);
    }

    void AddBackFaceQuad(ref List<int> newTrisForGrid, int firstVert)
    {
        //back face of quad
        newTrisForGrid.Add(7 + firstVert);
        newTrisForGrid.Add(6 + firstVert);
        newTrisForGrid.Add(5 + firstVert);

        newTrisForGrid.Add(5 + firstVert);
        newTrisForGrid.Add(4 + firstVert);
        newTrisForGrid.Add(7 + firstVert);
    }

    void AddLeftFaceQuad(ref List<int> newTrisForGrid, int firstVert)
    {
        //left face of quad
        newTrisForGrid.Add(4 + firstVert);
        newTrisForGrid.Add(5 + firstVert);
        newTrisForGrid.Add(1 + firstVert);

        newTrisForGrid.Add(1 + firstVert);
        newTrisForGrid.Add(0 + firstVert);
        newTrisForGrid.Add(4 + firstVert);
    }

    void AddTopFaceQuad(ref List<int> newTrisForGrid, int firstVert)
    {
        //top face of quad
        newTrisForGrid.Add(1 + firstVert);
        newTrisForGrid.Add(5 + firstVert);
        newTrisForGrid.Add(6 + firstVert);

        newTrisForGrid.Add(6 + firstVert);
        newTrisForGrid.Add(2 + firstVert);
        newTrisForGrid.Add(1 + firstVert);
    }

    void AddBottomFaceQuad(ref List<int> newTrisForGrid, int firstVert)
    {
        //bottom face of quad
        newTrisForGrid.Add(4 + firstVert);
        newTrisForGrid.Add(0 + firstVert);
        newTrisForGrid.Add(3 + firstVert);

        newTrisForGrid.Add(3 + firstVert);
        newTrisForGrid.Add(7 + firstVert);
        newTrisForGrid.Add(4 + firstVert);
    }
    #endregion
}


public struct Voxel
{
    public int[] gridIndex;

}

