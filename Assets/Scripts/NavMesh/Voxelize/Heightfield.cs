using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class Heightfield
{
    #region Debugging Data

    public Bounds sceneBounds;


    #endregion

    #region Raw pre-heightfield grid data

    public float XZCellSize;
    public float YCellSize;

    public int gridRowsX, gridRowsZ;
    public int gridColumns;

    #endregion

    Vector3[,,] verts;
    

    /// <summary>
    /// 2D Array of list of vertical spans
    /// </summary>
    List<HeightfieldSpan>[,] HeightFieldSpans;

    HeightfieldVoxel[,,] voxelGrid;

    SpanGraph heightSpanGraph;

    public Heightfield(float _XZCellSize, float _YCellSize)
    {
        XZCellSize = _XZCellSize;
        YCellSize = _YCellSize;

        HeightFieldSpans = new List<HeightfieldSpan>[0, 0];

    }

    #region Heightfield generation Functions

    public void ConvertHeightfieldGridToSpans(Mesh sceneMesh)
    {
        HeightFieldSpans = new List<HeightfieldSpan>[gridRowsX - 1, gridRowsZ - 1];
        
        for (int xIndex = 0; xIndex < gridRowsX - 1; xIndex++)
        {
            for(int zIndex = 0; zIndex < gridRowsZ - 1; zIndex++)
            {
                HeightFieldSpans[xIndex, zIndex] = new List<HeightfieldSpan>();
                
                var currentSpanColumn = HeightFieldSpans[xIndex, zIndex];

                for (int yIndex = 0; yIndex < gridColumns - 1; yIndex++)
                {
                    if (currentSpanColumn.Count < 1)
                    {
                        //create new span to start a new column
                        var newSpan = new HeightfieldSpan(voxelGrid[xIndex, yIndex, zIndex]);
                        currentSpanColumn.Add(newSpan);
                    }
                    else
                    {
                        //if we're continuing the same span as before
                        if (voxelGrid[xIndex, yIndex, zIndex].type == currentSpanColumn[currentSpanColumn.Count - 1].type)
                        {
                            currentSpanColumn[currentSpanColumn.Count - 1].AddVoxelToSpan(voxelGrid[xIndex, yIndex, zIndex]);
                        }
                        //else start a new span
                        else
                        {
                            var newSpan = new HeightfieldSpan(voxelGrid[xIndex, yIndex, zIndex]);
                            currentSpanColumn.Add(newSpan);
                        }
                    }

                    HeightFieldSpans[xIndex, zIndex] = currentSpanColumn;
                }
            }
        }
    }

    public void CheckHeightfieldAgainstTriangles(Triangle[] walkableTriangles, Mesh sceneMesh)
    {
        //init voxel grid
        voxelGrid = new HeightfieldVoxel[gridRowsX - 1, gridColumns - 1, gridRowsZ - 1];

        //jobs method of doing it
        //create triangles collection
        NativeArray<Triangle> sceneTriangles = new NativeArray<Triangle>(sceneMesh.triangles.Length, Allocator.TempJob);
        for (int i = 0; i < sceneMesh.triangles.Length; i += 3)
        {
            sceneTriangles[i] = new Triangle(sceneMesh.vertices[sceneMesh.triangles[i]], sceneMesh.vertices[sceneMesh.triangles[i + 1]], sceneMesh.vertices[sceneMesh.triangles[i + 2]]);
        }

        NativeArray<Triangle> walkableTris = new NativeArray<Triangle>(walkableTriangles, Allocator.TempJob);

        //create voxels collection
        NativeArray<HeightfieldVoxel> voxelGridFlatPacked = new NativeArray<HeightfieldVoxel>((gridRowsX - 1) * (gridRowsZ - 1) * (gridColumns - 1), Allocator.TempJob);

        for (int xIndex = 0; xIndex < gridRowsX - 1; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns - 1; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRowsZ - 1; zIndex++)
                {
                    Vector3[] voxelVerts = new Vector3[8]
                    {
                        verts[xIndex, yIndex, zIndex], verts[xIndex, yIndex + 1, zIndex],
                        verts[xIndex+1, yIndex + 1, zIndex], verts[xIndex+1, yIndex, zIndex],
                        verts[xIndex, yIndex, zIndex + 1], verts[xIndex, yIndex+1, zIndex + 1],
                        verts[xIndex+1, yIndex+1, zIndex+1], verts[xIndex+1, yIndex+1, zIndex + 1]
                    };

                    //flat pack voxels
                    //formula for 3d index to 1d index conversion = x + (y * gridRows) + (z * gridRows * gridColumns)
                    int currentIndex = xIndex + (yIndex * (gridRowsX - 1)) + (zIndex * (gridRowsX - 1) * (gridColumns - 1));
                    HeightfieldVoxel voxel = new HeightfieldVoxel(voxelVerts, XZCellSize, YCellSize);
                    voxelGridFlatPacked[currentIndex] = voxel;
                }
            }
        }

        //create job
        VoxelFieldGenerationJob vJob = new VoxelFieldGenerationJob()
        {
            voxelField = voxelGridFlatPacked,
            triangles = sceneTriangles,
            walkableTriangles = walkableTris
        };

        //randomly selected a number for num of threads
        int threadCount = 16;
        int innerLoopBatchCount = voxelGridFlatPacked.Count() / threadCount;
        //clamp lower limit of value to 1
        innerLoopBatchCount = innerLoopBatchCount < 1 ? 1 : innerLoopBatchCount;

        JobHandle vJobHandle = vJob.Schedule(voxelGridFlatPacked.Count(), voxelGridFlatPacked.Count() / threadCount);

        vJobHandle.Complete();

        //unpack voxels into grid
        for (int xIndex = 0; xIndex < gridRowsX - 1; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns - 1; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRowsZ - 1; zIndex++)
                {
                    int voxelIndex = xIndex + (yIndex * (gridRowsX - 1)) + (zIndex * (gridRowsX - 1) * (gridColumns - 1));

                    voxelGrid[xIndex, yIndex, zIndex] = vJob.voxelField[voxelIndex];
                }
            }
        }

        //dispose native arrays
        voxelGridFlatPacked.Dispose();
        sceneTriangles.Dispose();
        walkableTris.Dispose();

        #region old method single threaded and main thread blocking way of creating voxel field
        //this code has been left here for documenting reasons
        //new method is 61% faster
        //old method: 8.6 seconds
        //new method: 3.3 seconds

        //List<Triangle> trianglesList = new List<Triangle>(walkableTriangles);

        //for (int xIndex = 0; xIndex < gridRows - 1; xIndex++)
        //{
        //    for (int yIndex = 0; yIndex < gridColumns - 1; yIndex++)
        //    {
        //        for (int zIndex = 0; zIndex < gridRows - 1; zIndex++)
        //        {
        //            Vector3[] voxelVerts = new Vector3[8]
        //            {
        //                verts[xIndex, yIndex, zIndex], verts[xIndex, yIndex + 1, zIndex],
        //                verts[xIndex+1, yIndex + 1, zIndex], verts[xIndex+1, yIndex, zIndex],
        //                verts[xIndex, yIndex, zIndex + 1], verts[xIndex, yIndex+1, zIndex + 1],
        //                verts[xIndex+1, yIndex+1, zIndex+1], verts[xIndex+1, yIndex+1, zIndex + 1]
        //            };

        //            HeightfieldVoxel voxel = new HeightfieldVoxel(voxelVerts, XZCellSize, YCellSize);

        //            for (int i = 0; i < sceneMesh.triangles.Length; i += 3)
        //            {
        //                Triangle tri = new Triangle(sceneMesh.vertices[sceneMesh.triangles[i]], sceneMesh.vertices[sceneMesh.triangles[i + 1]], sceneMesh.vertices[sceneMesh.triangles[i + 2]]);

        //                bool result = Intersections.Intersects(tri, voxel.VoxelBounds);

        //                voxel.isWalkable = trianglesList.Contains(tri);

        //                if (result)
        //                {
        //                    voxel.type = HeightFieldVoxelType.Closed;
        //                    voxel.intersectingTriangleNormal = tri.Normal;

        //                    voxelGrid[xIndex, yIndex, zIndex] = voxel;
        //                    break;
        //                }
        //                else
        //                {
        //                    voxel.type = HeightFieldVoxelType.Open;
        //                }

        //                voxelGrid[xIndex, yIndex, zIndex] = voxel;
        //            }

        //        }
        //    }
        //}
        #endregion
    }

    public void CreateHeightFieldGrid(Bounds _sceneBounds)
    {
        sceneBounds = _sceneBounds;

        Bounds voxelBound = new Bounds();
        voxelBound.size = new Vector3(XZCellSize, YCellSize, XZCellSize);

        int XCellsCount = Mathf.CeilToInt(_sceneBounds.size.x / XZCellSize);
        int ZCellsCount = Mathf.CeilToInt(_sceneBounds.size.z / XZCellSize);
        int YCellsCount = Mathf.CeilToInt(_sceneBounds.size.y / YCellSize);

        gridRowsX = XCellsCount + 1;
        gridRowsZ = ZCellsCount + 1;
        gridColumns = YCellsCount + 1;


        verts = new Vector3[gridRowsX, gridColumns, gridRowsZ];

        Vector3 startPos = new Vector3();
        bool firstVert = false;

        //create vertices to represent the heightfield grid
        for (int xIndex = 0; xIndex < gridRowsX; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRowsZ; zIndex++)
                {
                    if (!firstVert)
                    {
                        startPos = _sceneBounds.min - new Vector3(XZCellSize / 2, YCellSize / 2, XZCellSize / 2);
                        verts[0, 0, 0] = startPos;
                        firstVert = true;

                        UnityEngine.Debug.Log(_sceneBounds.min);
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
                        verts[xIndex,yIndex,zIndex] = newVert;
                    }
                }

            }

        }
    }

    #endregion
    
    #region Getters for debugging
    public HeightfieldVoxel[,,] GetVoxelGrid()
    {
        return voxelGrid;
    }

    public List<HeightfieldSpan>[,] GetHeightSpans()
    {
        return HeightFieldSpans;
    }
    #endregion
}

public struct HeightfieldSpan
{
    public HeightFieldVoxelType type;

    List<HeightfieldVoxel> spanVoxels;

    AABB spanBounds;

    public AABB SpanBounds
    {
        get
        {
            CalculateSpanBounds();

            return spanBounds;
        }
    }

    public void AddVoxelToSpan(HeightfieldVoxel voxel)
    {
        spanVoxels.Add(voxel);
        CalculateSpanBounds();
    }

    public List<HeightfieldVoxel> GetSpanVoxels()
    {
        return spanVoxels;
    }

    public void CalculateSpanBounds()
    {
        Vector3 min = Vector3.one;
        Vector3 max = Vector3.zero;

        foreach (var voxel in spanVoxels)
        {
            min = Vector3.Min(min, voxel.VoxelBounds.Min);
            max = Vector3.Max(max, voxel.VoxelBounds.Max);
        }

        spanBounds = new AABB(min, max);
    }

    public HeightfieldSpan(HeightfieldVoxel startingVoxel)
    {
        spanVoxels = new List<HeightfieldVoxel>() { startingVoxel };

        spanBounds = startingVoxel.VoxelBounds;
        type = startingVoxel.type;
    }

    public float GetSpanHeight()
    {
        
        CalculateSpanBounds();

        return SpanBounds.Max.y - SpanBounds.Min.y;
    }

    public float GetSpanStartHeight()
    {
        CalculateSpanBounds();

        return SpanBounds.Min.y;
    }

    public HQuad GetSpanFloor()
    {
        HQuad toReturn = new HQuad();

        toReturn.bottomLeft = SpanBounds.Min;
        toReturn.topLeft = new Vector3(SpanBounds.Min.x, SpanBounds.Min.y, SpanBounds.Max.z);
        toReturn.bottomRight = new Vector3(SpanBounds.Max.x, SpanBounds.Min.y, SpanBounds.Min.z);
        toReturn.topRight = new Vector3(SpanBounds.Max.x, SpanBounds.Min.y, SpanBounds.Max.z);

        return toReturn;
    }

    public HQuad GetSpanCeiling()
    {
        HQuad toReturn = new HQuad();

        toReturn.bottomLeft = spanVoxels[spanVoxels.Count - 1].VoxelBounds.Min;
        toReturn.bottomLeft.y = spanVoxels[spanVoxels.Count - 1].VoxelBounds.Max.y;

        toReturn.topLeft = spanVoxels[spanVoxels.Count - 1].VoxelBounds.Max;
        toReturn.topLeft.x = spanVoxels[spanVoxels.Count - 1].VoxelBounds.Min.x;

        toReturn.topRight = spanVoxels[spanVoxels.Count - 1].VoxelBounds.Max;

        toReturn.bottomRight = spanVoxels[spanVoxels.Count - 1].VoxelBounds.Max;
        toReturn.bottomRight.z = spanVoxels[spanVoxels.Count - 1].VoxelBounds.Min.z;

        return toReturn;
    }

    public bool ContainsWalkableVoxels()
    {
        foreach(var vox in spanVoxels)
        {
           if (vox.isWalkable)
               return true;
        }
        return false;
    }
}

public struct HeightfieldVoxel
{    
    public HeightFieldVoxelType type;

    public AABB VoxelBounds;

    public bool isWalkable;

    /// <summary>
    /// Vector3.zero if voxel is open
    /// </summary>
    public Vector3 intersectingTriangleNormal;

    public float XZCellSize, YCellSize;

    public HeightfieldVoxel(Vector3[] _vertices, float XZSize, float YSize)
    {
        type = HeightFieldVoxelType.Open;
        isWalkable = false;

        Bounds bounds = new Bounds();
        //vertices = _vertices;

        bounds.min = bounds.max = _vertices[0];

        foreach (var item in _vertices)
        {
            bounds.min = Vector3.Min(item, bounds.min);
            bounds.max = Vector3.Max(item, bounds.max);
        }

        VoxelBounds = new AABB(bounds);

        intersectingTriangleNormal = Vector3.zero;

        XZCellSize = XZSize;
        YCellSize = YSize;
    }
}
public enum HeightFieldVoxelType
{
    Closed,
    Open
}

public struct HQuad
{
    public Vector3 bottomLeft, topLeft, topRight, bottomRight;
}

struct VoxelFieldGenerationJob : IJobParallelFor
{
    public NativeArray<HeightfieldVoxel> voxelField;

    public NativeArray<Triangle> triangles;

    public NativeArray<Triangle> walkableTriangles;

    public void Execute(int index)
    {
        foreach(Triangle triangle in triangles)
        {
            if (Intersections.Intersects(voxelField[index].VoxelBounds, triangle))
            {
                //need to do it this way because can't alter members of items in native array directly
                var v = voxelField[index];
                v.type = HeightFieldVoxelType.Closed;

                if(walkableTriangles.Contains(triangle))
                {
                    v.isWalkable = true;
                }

                voxelField[index] = v;

                return;
            }
        }
    }
}