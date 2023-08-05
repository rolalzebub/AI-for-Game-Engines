using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoxelizeScene : MonoBehaviour
{
    List<SceneMeshObject> allSceneMeshes;
    public float XZCellSize;
    public float YCellSize;

    Heightfield sceneField;

    [System.NonSerialized]
    public bool sceneVoxed = false;

    public DebugHeightSpanDrawMode debugMode;

    /// <summary>
    /// The maximum incline angle (degrees) that can be considered walkable.
    /// </summary>
    public float maxWalkableSlope = 45f;


    /// <summary>
    /// Check which triangles in the combined scene geometry can be considered walkable by checking what direction the triangle faces. If the angle between the triangle's normal vector and Vector3.up is within maxWalkableSlope degrees, the triangle is considered walkable.
    /// </summary>
    /// <param name="combinedSceneMesh"></param>
    /// <returns></returns>
    private Triangle[] GetWalkableTriangles(Mesh combinedSceneMesh)
    {
        List<Triangle> walkableTriangles = new List<Triangle>();
        var trianglesToCheck = combinedSceneMesh.triangles;
        var vertices = combinedSceneMesh.vertices;

        for (int i = 0; i < trianglesToCheck.Length; i+=3)
        {
            Triangle tri = new Triangle(vertices[trianglesToCheck[i]], vertices[trianglesToCheck[i + 1]], vertices[trianglesToCheck[i + 2]]);
            if(Vector3.Angle(Vector3.up, tri.Normal) <= maxWalkableSlope && Vector3.Angle(Vector3.up, tri.Normal) >= -maxWalkableSlope)
            {
                walkableTriangles.Add(tri);
            }
        }

        return walkableTriangles.ToArray();
    }

    /// <summary>
    /// Create a voxel grid that encompasses all relevant scene geometry. This function retrieves all meshes in the scene that have the "SceneMeshObject" component attached, combines them all into a single mesh, and then uses that mesh for generating the voxel grid.
    /// </summary>
    public void VoxelizeSceneByCombiningMeshes()
    {
        //find all static meshes in the scene
        var sceneMeshCollection = FindObjectsOfType<MeshFilter>().Where(x => x.gameObject.isStatic);

        Mesh sceneMesh = new Mesh();

        //use Unity's own mesh combiner to generate final mesh
        CombineInstance[] meshesToCombine = new CombineInstance[allSceneMeshes.Count];
        
        for (int i = 0; i < sceneMeshCollection.Count(); i++)
        {
            meshesToCombine[i].mesh = sceneMeshCollection.ElementAt(i).sharedMesh;
            
            //mesh combiner lets us use the localToWorldMatrix in the mesh to convert its coordinates to world space automatically at "combine-time"
            meshesToCombine[i].transform = sceneMeshCollection.ElementAt(i).transform.localToWorldMatrix;
        }

        sceneMesh.CombineMeshes(meshesToCombine);

        //generate voxel grid a.k.a. Heightfield
        sceneField = new Heightfield(XZCellSize, YCellSize);

        sceneField.CreateVoxelGrid(sceneMesh.bounds);
        sceneField.CheckHeightfieldAgainstTriangles(GetWalkableTriangles(sceneMesh), sceneMesh);
        sceneField.ConvertHeightfieldGridToSpans(sceneMesh);
        sceneVoxed = true;
    }

    public Heightfield GetHeightfield()
    {
        return sceneField;
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
                                if (item.type == HeightFieldVoxelType.Closed)
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
                                if (item.type == HeightFieldVoxelType.Closed)
                                {
                                    drawCube = true;
                                }
                                break;
                            }
                        case DebugHeightSpanDrawMode.OpenSpans:
                            {
                                Gizmos.color = Color.green;
                                if (item.type == HeightFieldVoxelType.Open)
                                {
                                    drawCube = true;
                                }
                                break;
                            }
                    }
                    if (drawCube)
                    {
                        var spanVoxels = item.GetSpanVoxels();

                        //we want to find the minimum and maximum for the entire span here
                        Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;

                        foreach (var voxel in spanVoxels)
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

