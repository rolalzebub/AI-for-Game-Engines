using System.Collections.Generic;
using UnityEngine;

public class Heightfield
{

    #region Voxel granularity options

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

    Voxel[,,] generatedVoxels;
    HFSpansColumn[,] hField;

    public Heightfield(float _XZCellSize, float _YCellSize)
    {
        XZCellSize = _XZCellSize;
        YCellSize = _YCellSize;

        HeightFieldSpans = new List<HeightfieldSpan>[0, 0];
    }

    #region Heightfield generation Functions


    /// <summary>
    /// This function creates a Heightfield by combining all vertically neighbouring voxels that are identical into one column that spans the height of the collection of voxels.
    /// </summary>
    /// <param name="sceneMesh">The mesh containing all scene geometry.</param>
    public void ConvertHeightfieldGridToSpans(Mesh sceneMesh)
    {
        HeightFieldSpans = new List<HeightfieldSpan>[gridRows - 1, gridRows - 1];
        hField = new HFSpansColumn[gridRows - 1, gridRows - 1];
        for (int xIndex = 0; xIndex < gridRows - 1; xIndex++)
        {
            for(int zIndex = 0; zIndex < gridRows - 1; zIndex++)
            {
                HeightFieldSpans[xIndex, zIndex] = new List<HeightfieldSpan>();

                List<Voxel> currentSpanVoxels = new List<Voxel>();
                List<HFSpan> currentSpansColumnDynamic = new List<HFSpan>();

                List<HeightfieldSpan> currentColumnSpans = HeightFieldSpans[xIndex, zIndex];

                //create new span at the start of a new column
                HeightfieldSpan firstSpan = new HeightfieldSpan(generatedVoxels[xIndex, 0, zIndex]);
                currentColumnSpans.Add(firstSpan);
                currentSpanVoxels.Add(generatedVoxels[xIndex, 0, zIndex]);

                //remember to iterate on yIndex here
                for (int yIndex = 1; yIndex < gridColumns - 1; yIndex++)
                {
                    //check if the next voxel up is the same type as the span
                    if (voxelGrid[xIndex, yIndex, zIndex].type == currentColumnSpans[currentColumnSpans.Count - 1].type)
                    {
                        currentColumnSpans[currentColumnSpans.Count - 1].AddVoxelToSpan(voxelGrid[xIndex, yIndex, zIndex]);

                        currentSpanVoxels.Add(generatedVoxels[xIndex, yIndex, zIndex]);
                    }
                    //else start a new span
                    else
                    {
                        //create new span
                        var newSpan = new HeightfieldSpan(voxelGrid[xIndex, yIndex, zIndex]);
                        //add it to list of current column spans
                        currentColumnSpans.Add(newSpan);

                        HFSpan currentSpan = new HFSpan(currentSpanVoxels.ToArray());
                        currentSpansColumnDynamic.Add(currentSpan);
                    }

                    HeightFieldSpans[xIndex, zIndex] = currentColumnSpans;

                    HFSpansColumn column = new HFSpansColumn() { m_spans = currentSpansColumnDynamic.ToArray() };
                    hField[xIndex, zIndex] = column;
                }
            }
        }
    }

    /// <summary>
    /// This function analyzes the created voxel grid against all triangles in the scene's geometry and marks all voxels either open or closed depending on if they intersect with any scene geometry. The first check for whether or not a voxel is walkable is done here. If a voxel intersects a triangle contained in the array of walkable triangles, the voxel is considered walkable.
    /// </summary>
    /// <param name="walkableTriangles">Array of walkable triangles to check against for walkable voxels</param>
    /// <param name="sceneMesh">The mesh containing all the geometry in the scene to check voxels against</param>
    public void CheckHeightfieldAgainstTriangles(Triangle[] walkableTriangles, Mesh sceneMesh)
    {
        voxelGrid = new HeightfieldVoxel[gridRows - 1, gridColumns - 1, gridRows - 1];
        generatedVoxels = new Voxel[gridRows - 1, gridColumns - 1, gridRows - 1];

        List<Triangle> trianglesList = new List<Triangle>(walkableTriangles);

        for (int xIndex = 0; xIndex < gridRows - 1; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns - 1; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRows - 1; zIndex++)
                {
                    //use the appropriate vertices from the grid to create a voxel
                    //a voxel/cube will always have 8 vertices, hence the 8 element size of array
                    Vector3[] voxelVerts = new Vector3[8]
                    {
                        //front-bottom left, front-top left
                        verts[xIndex, yIndex, zIndex], verts[xIndex, yIndex + 1, zIndex],
                        //front-top right, front-bottom right
                        verts[xIndex+1, yIndex + 1, zIndex], verts[xIndex+1, yIndex, zIndex],
                        //back-bottom left, back-top left
                        verts[xIndex, yIndex, zIndex + 1], verts[xIndex, yIndex+1, zIndex + 1],
                        //back-top right, back-bottom right
                        verts[xIndex+1, yIndex+1, zIndex+1], verts[xIndex+1, yIndex+1, zIndex + 1]
                    };

                    HeightfieldVoxel voxel = new HeightfieldVoxel(voxelVerts, XZCellSize, YCellSize);

                    Voxel nVoxel = new Voxel(voxelVerts, XZCellSize, YCellSize);

                    //check each triangle against the voxel we have just created
                    for (int i = 0; i < sceneMesh.triangles.Length; i += 3)
                    {
                        Triangle tri = new Triangle(sceneMesh.vertices[sceneMesh.triangles[i]], sceneMesh.vertices[sceneMesh.triangles[i + 1]], sceneMesh.vertices[sceneMesh.triangles[i + 2]]);

                        bool result = Intersections.Intersects(tri, voxel.VoxelBounds);

                        voxel.isWalkable = trianglesList.Contains(tri);

                        if (result)
                        {
                            nVoxel.isClosed = true;
                            nVoxel.intersectingTriangleNormal = tri.Normal;
                            generatedVoxels[xIndex,yIndex,zIndex] = nVoxel;

                            voxel.type = HeightFieldVoxelType.Closed;
                            voxel.intersectingTriangleNormal = tri.Normal;

                            voxelGrid[xIndex, yIndex, zIndex] = voxel;
                            //break out as soon as we intersect with a triangle
                            break;
                        }
                        else
                        {
                            voxel.type = HeightFieldVoxelType.Open;
                        }

                        voxelGrid[xIndex, yIndex, zIndex] = voxel;
                        generatedVoxels[xIndex, yIndex, zIndex] = nVoxel;
                    }
                }
            }
        }

    }

    /// <summary>
    /// This creates a voxel grid with all voxels marked open (no intersection checks are done)
    /// </summary>
    /// <param name="_sceneBounds">Bounds that encompass all the geometry to be encompassed by the voxel grid.</param>
    public void CreateVoxelGrid(Bounds _sceneBounds)
    {
        //establish per-voxel sizes
        Bounds voxelBound = new Bounds();
        voxelBound.size = new Vector3(XZCellSize, YCellSize, XZCellSize);

        //figure out how many voxels there will be so we can create appropriately sized arrays to contain the grid
        int XZCellsCount = Mathf.CeilToInt(_sceneBounds.size.x / XZCellSize);
        int YCellsCount = Mathf.CeilToInt(_sceneBounds.size.y / YCellSize);

        gridRows = XZCellsCount + 1;
        gridColumns = YCellsCount + 1;
        verts = new Vector3[gridRows, gridColumns, gridRows];

        Vector3 startPos = new Vector3();

        //set the start position for the grid
        startPos = _sceneBounds.min - new Vector3(XZCellSize / 2, YCellSize / 2, XZCellSize / 2);

        //set the position of each vertex in the grid
        for (int xIndex = 0; xIndex < gridRows; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRows; zIndex++)
                {
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
        spanVoxels = new List<HeightfieldVoxel>
        {
            startingVoxel
        };
        type = startingVoxel.type;
    }

    public HeightfieldSpan(Voxel startingVoxel)
    {
        spanVoxels = new List<HeightfieldVoxel>
        {
            new HeightfieldVoxel(startingVoxel.m_vertices, startingVoxel.m_Bounds.size.x, startingVoxel.m_Bounds.size.y)
        };

        type = startingVoxel.isClosed ? HeightFieldVoxelType.Closed : HeightFieldVoxelType.Open;
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

public struct HeightfieldVoxel
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

        type = HeightFieldVoxelType.Open;

        isWalkable = false;
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



public struct Voxel
{
    /// <summary>
    /// An closed voxel has at least one triangle intersecting with it.
    /// </summary>
    public bool isClosed;
    /// <summary>
    /// The normal of an intersecting triangle. This will be Vector3.zero if no intersecting triangle exists.
    /// </summary>
    public Vector3 intersectingTriangleNormal;
    public Vector3[] m_vertices;
    public Bounds m_Bounds;

    public Voxel(Vector3[] _vertices, float XZSize, float YSize)
    {
        Bounds bounds = new Bounds();
        m_vertices = _vertices;

        bounds.min = bounds.max = m_vertices[0];

        foreach (var item in m_vertices)
        {
            bounds.min = Vector3.Min(item, bounds.min);
            bounds.max = Vector3.Max(item, bounds.max);
        }

        m_Bounds = bounds;

        isClosed = false;
        intersectingTriangleNormal = Vector3.zero;
    }
}

/// <summary>
/// A span is a continuous column of open/closed voxels.
/// </summary>
public struct HFSpan
{
    public Voxel[] m_voxels;
    public bool isClosed;
    public Bounds m_Bounds;

    public HFSpan(Voxel[] voxels)
    {
        m_voxels = voxels;
        isClosed = false;
        m_Bounds = new Bounds();

        CalculateSpanBounds();
    }

    private void CalculateSpanBounds()
    {
        Vector3 min = Vector3.one;
        Vector3 max = Vector3.zero;

        foreach (var voxel in m_voxels)
        {
            min = Vector3.Min(min, voxel.m_Bounds.min);
            max = Vector3.Max(max, voxel.m_Bounds.max);
        }

        m_Bounds = new Bounds(min, max);
    }
}

public struct HFSpansColumn
{
    public HFSpan[] m_spans;
}

public struct HField
{
    public HFSpansColumn[,] m_columns;
}