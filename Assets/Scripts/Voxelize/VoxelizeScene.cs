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

    void Start()
    {
        allSceneMeshes = new List<SceneMeshObject>(FindObjectsOfType<SceneMeshObject>());
        ReplaceSceneMeshesWithVoxels();
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


        var go = Instantiate(new GameObject());
        go.AddComponent<MeshFilter>().sharedMesh = CreateHeightField(sceneMesh.bounds);
        go.AddComponent<MeshRenderer>();
    }

    Mesh CreateHeightField(Bounds sceneBounds)
    {
        //set up heightfield mesh for debug rendering
        Mesh heightfieldMesh = new Mesh();
        heightfieldMesh.bounds = sceneBounds;
        Bounds voxelBound = new Bounds();
        voxelBound.size = new Vector3(XZCellSize, YCellSize, XZCellSize);


        int XCellsCount = Mathf.CeilToInt(sceneBounds.size.x / XZCellSize);
        int ZCellsCount = Mathf.CeilToInt(sceneBounds.size.z / XZCellSize);
        int YCellsCount = Mathf.CeilToInt(sceneBounds.size.y / YCellSize);

        List<Vector3> newVertsForGrid = new List<Vector3>();
        List<int> newTrisForGrid = new List<int>();

        //need this to be a 32 bit index buffer because we are gonna create MANY verts here before simplifying
        heightfieldMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        for (int i = 0; i < YCellsCount - 1; i++)
        {
            for (int j = 0; j < ZCellsCount - 1; j++)
            {
                for(int k = 0; k < XCellsCount - 1; k++)
                {
                    //place verts for voxelizing space within bounds
                    //i.e. create verts for a cube
                    Vector3 newVert = new Vector3()
                    {
                        x = k * XZCellSize,
                        y = i * YCellSize,
                        z = j * XZCellSize
                    };

                    Vector3 newVert2 = new Vector3()
                    {
                        x = k * XZCellSize,
                        y = (i + 1) * YCellSize,
                        z = j * XZCellSize
                    };

                    Vector3 newVert3 = new Vector3()
                    {
                        x = (k + 1) * XZCellSize,
                        y = (i + 1) * YCellSize,
                        z = j * XZCellSize
                    };

                    Vector3 newVert4 = new Vector3()
                    {
                        x = (k + 1) * XZCellSize,
                        y = i * YCellSize,
                        z = j * XZCellSize
                    };

                    Vector3 newVert5 = new Vector3()
                    {
                        x = k * XZCellSize,
                        y = i * YCellSize,
                        z = (j + 1) * XZCellSize
                    };

                    Vector3 newVert6 = new Vector3()
                    {
                        x = k * XZCellSize,
                        y = (i + 1) * YCellSize,
                        z = (j + 1) * XZCellSize
                    };

                    Vector3 newVert7 = new Vector3()
                    {
                        x = (k + 1) * XZCellSize,
                        y = (i + 1) * YCellSize,
                        z = (j + 1) * XZCellSize
                    };

                    Vector3 newVert8 = new Vector3()
                    {
                        x = (k + 1) * XZCellSize,
                        y = i * YCellSize,
                        z = (j + 1) * XZCellSize
                    };

                    newVertsForGrid.Add(newVert);
                    int firstVert = newVertsForGrid.Count - 1;
                    newVertsForGrid.Add(newVert2);
                    newVertsForGrid.Add(newVert3);
                    newVertsForGrid.Add(newVert4);
                    newVertsForGrid.Add(newVert5);
                    newVertsForGrid.Add(newVert6);
                    newVertsForGrid.Add(newVert7);
                    newVertsForGrid.Add(newVert8);
                    
                    //front face of quad
                    newTrisForGrid.Add(0 + firstVert);
                    newTrisForGrid.Add(1 + firstVert);
                    newTrisForGrid.Add(2 + firstVert);

                    newTrisForGrid.Add(2 + firstVert);
                    newTrisForGrid.Add(3 + firstVert);
                    newTrisForGrid.Add(0 + firstVert);

                    //right face of quad
                    newTrisForGrid.Add(3 + firstVert);
                    newTrisForGrid.Add(2 + firstVert);
                    newTrisForGrid.Add(6 + firstVert);

                    newTrisForGrid.Add(6 + firstVert);
                    newTrisForGrid.Add(7 + firstVert);
                    newTrisForGrid.Add(3 + firstVert);

                    //back face of quad
                    newTrisForGrid.Add(7 + firstVert);
                    newTrisForGrid.Add(6 + firstVert);
                    newTrisForGrid.Add(5 + firstVert);

                    newTrisForGrid.Add(5 + firstVert);
                    newTrisForGrid.Add(4 + firstVert);
                    newTrisForGrid.Add(7 + firstVert);

                    //left face of quad
                    newTrisForGrid.Add(4 + firstVert);
                    newTrisForGrid.Add(5 + firstVert);
                    newTrisForGrid.Add(1 + firstVert);

                    newTrisForGrid.Add(1 + firstVert);
                    newTrisForGrid.Add(0 + firstVert);
                    newTrisForGrid.Add(4 + firstVert);

                    //top face of quad
                    newTrisForGrid.Add(1 + firstVert);
                    newTrisForGrid.Add(5 + firstVert);
                    newTrisForGrid.Add(6 + firstVert);

                    newTrisForGrid.Add(6 + firstVert);
                    newTrisForGrid.Add(2 + firstVert);
                    newTrisForGrid.Add(1 + firstVert);

                    //bottom face of quad
                    newTrisForGrid.Add(4 + firstVert);
                    newTrisForGrid.Add(0 + firstVert);
                    newTrisForGrid.Add(3 + firstVert);

                    newTrisForGrid.Add(3 + firstVert);
                    newTrisForGrid.Add(7 + firstVert);
                    newTrisForGrid.Add(4 + firstVert);

                }
            }
        }
        heightfieldMesh.SetVertices(newVertsForGrid);
        heightfieldMesh.SetTriangles(newTrisForGrid, 0);
        heightfieldMesh.RecalculateBounds();
        heightfieldMesh.RecalculateNormals();

        return heightfieldMesh;
    }
}

public struct Voxel
{
    Vector3[] vertices;
    int[] triangles;
}
