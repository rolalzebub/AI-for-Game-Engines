using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heightfield
{
    #region Debugging Data

    public Bounds sceneBounds;


    #endregion

    #region Raw pre-heightfield grid data

    public float XZCellSize;
    public float YCellSize;

    public int gridRows;
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
        HeightFieldSpans = new List<HeightfieldSpan>[gridRows - 1, gridRows - 1];
        
        for (int xIndex = 0; xIndex < gridRows - 1; xIndex++)
        {
            for(int zIndex = 0; zIndex < gridRows - 1; zIndex++)
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
        voxelGrid = new HeightfieldVoxel[gridRows - 1, gridColumns - 1, gridRows - 1];
        List<Triangle> trianglesList = new List<Triangle>(walkableTriangles);

        for (int xIndex = 0; xIndex < gridRows - 1; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns - 1; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRows - 1; zIndex++)
                {
                    Vector3[] voxelVerts = new Vector3[8]
                    {
                        verts[xIndex, yIndex, zIndex], verts[xIndex, yIndex + 1, zIndex],
                        verts[xIndex+1, yIndex + 1, zIndex], verts[xIndex+1, yIndex, zIndex],
                        verts[xIndex, yIndex, zIndex + 1], verts[xIndex, yIndex+1, zIndex + 1],
                        verts[xIndex+1, yIndex+1, zIndex+1], verts[xIndex+1, yIndex+1, zIndex + 1]
                    };

                    HeightfieldVoxel voxel = new HeightfieldVoxel(voxelVerts, XZCellSize, YCellSize);

                    for (int i = 0; i < sceneMesh.triangles.Length; i+=3)
                    {
                        Triangle tri = new Triangle(sceneMesh.vertices[sceneMesh.triangles[i]], sceneMesh.vertices[sceneMesh.triangles[i + 1]], sceneMesh.vertices[sceneMesh.triangles[i + 2]]);

                        bool result = Intersections.Intersects(tri, voxel.VoxelBounds);

                        voxel.isWalkable = trianglesList.Contains(tri);

                        if (result)
                        {
                            voxel.type = HeightFieldVoxelType.Closed;
                            voxel.intersectingTriangleNormal = tri.Normal;

                            voxelGrid[xIndex, yIndex, zIndex] = voxel;
                            break;
                        }
                        else
                        {
                            voxel.type = HeightFieldVoxelType.Open;
                        }

                        voxelGrid[xIndex, yIndex, zIndex] = voxel;
                    }

                }
            }
        }
    }

    public void CreateHeightFieldGrid(Bounds _sceneBounds)
    {
        sceneBounds = _sceneBounds;

        Bounds voxelBound = new Bounds();
        voxelBound.size = new Vector3(XZCellSize, YCellSize, XZCellSize);

        int XZCellsCount = Mathf.CeilToInt(_sceneBounds.size.x / XZCellSize);
        int YCellsCount = Mathf.CeilToInt(_sceneBounds.size.y / YCellSize);

        gridRows = XZCellsCount + 1;
        gridColumns = YCellsCount + 1;
        verts = new Vector3[gridRows, gridColumns, gridRows];

        Vector3 startPos = new Vector3();
        bool firstVert = false;

        //create vertices to represent the heightfield grid
        for (int xIndex = 0; xIndex < gridRows; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRows; zIndex++)
                {
                    if (!firstVert)
                    {
                        startPos = _sceneBounds.min - new Vector3(XZCellSize / 2, YCellSize / 2, XZCellSize / 2);
                        verts[0, 0, 0] = startPos;
                        firstVert = true;

                        Debug.Log(_sceneBounds.min);
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

public class HeightfieldSpan
{
    public HeightFieldVoxelType type;

    List<HeightfieldVoxel> spanVoxels;

    AABB spanBounds = null;

    public AABB SpanBounds
    {
        get
        {
            if (spanBounds == null)
            {
                CalculateSpanBounds();
            }

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
        spanVoxels = new List<HeightfieldVoxel>();
        spanVoxels.Add(startingVoxel);
        type = startingVoxel.type;
    }

    public float GetSpanHeight()
    {
        if (spanBounds == null)
        {
            CalculateSpanBounds();
        }

        return SpanBounds.Max.y - SpanBounds.Min.y;
    }

    public float GetSpanStartHeight()
    {
        if (spanBounds == null)
        {
            CalculateSpanBounds();
        }

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

public class HeightfieldVoxel
{
    Vector3[] vertices;

    public HeightFieldVoxelType type;

    public AABB VoxelBounds;

    public bool isWalkable;

    /// <summary>
    /// Vector3.zero if voxel is open
    /// </summary>
    public Vector3 intersectingTriangleNormal;

    public HeightfieldVoxel(Vector3[] _vertices, float XZSize, float YSize)
    {
        Bounds bounds = new Bounds();
        vertices = _vertices;

        bounds.min = bounds.max = vertices[0];

        foreach (var item in vertices)
        {
            bounds.min = Vector3.Min(item, bounds.min);
            bounds.max = Vector3.Max(item, bounds.max);
        }

        VoxelBounds = new AABB(bounds);

        intersectingTriangleNormal = Vector3.zero;
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