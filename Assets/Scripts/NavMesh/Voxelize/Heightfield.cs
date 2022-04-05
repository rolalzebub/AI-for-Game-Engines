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
    public float maxWalkableSlopeAngle;

    public int gridRows;
    public int gridColumns;

    #endregion

    Vector3[,,] verts;

    /// <summary>
    /// 2D Array of list of vertical spans
    /// </summary>
    List<HeightfieldSpan>[,] HeightFieldSpans;

    HeightfieldVoxel[,,] voxelGrid;

    public Heightfield(float _XZCellSize, float _YCellSize, float _maxWalkableSlopeAngle)
    {
        XZCellSize = _XZCellSize;
        YCellSize = _YCellSize;
        maxWalkableSlopeAngle = _maxWalkableSlopeAngle;

        HeightFieldSpans = new List<HeightfieldSpan>[0, 0];
    }

    #region Heightfield generation Functions

    void ConvertHeightfieldGridToSpans()
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
                        var newSpan = new HeightfieldSpan();
                        newSpan.type = voxelGrid[xIndex, yIndex, zIndex].type;
                        newSpan.spanVoxels.Add(voxelGrid[xIndex, yIndex, zIndex]);
                        currentSpanColumn.Add(newSpan);
                    }
                    else
                    {
                        //if we're continuing the same span as before
                        if (voxelGrid[xIndex, yIndex, zIndex].type == currentSpanColumn[currentSpanColumn.Count - 1].type)
                        {
                            currentSpanColumn[currentSpanColumn.Count - 1].spanVoxels.Add(voxelGrid[xIndex, yIndex, zIndex]);
                        }
                        //else start a new span
                        else
                        {
                            var newSpan = new HeightfieldSpan();
                            newSpan.type = voxelGrid[xIndex, yIndex, zIndex].type;
                            newSpan.spanVoxels.Add(voxelGrid[xIndex, yIndex, zIndex]);
                            currentSpanColumn.Add(newSpan);
                        }
                    }
                    HeightFieldSpans[xIndex, zIndex] = currentSpanColumn;
                }
            }
        }
    }

    public void CheckHeightfieldAgainstMesh(Mesh meshToCheck)
    {
        voxelGrid = new HeightfieldVoxel[gridRows - 1, gridColumns - 1, gridRows - 1];

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


                    for(int i = 0; i < meshToCheck.triangles.Length; i+=3)
                    {
                        Triangle tri = new Triangle(meshToCheck.vertices[meshToCheck.triangles[i]], meshToCheck.vertices[meshToCheck.triangles[i + 1]], meshToCheck.vertices[meshToCheck.triangles[i + 2]]);
                        
                        bool result = Intersections.Intersects(tri, voxel.VoxelBounds);
                        
                        if (result)
                        {
                            voxel.type = HeightFieldVoxelType.Solid;
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

        ConvertHeightfieldGridToSpans();
    }

    public void CreateHeightFieldGrid(Bounds _sceneBounds, float walkableSlopeAngle)
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

    public class HeightfieldSpan
    {
        public HeightFieldVoxelType type;


        public List<HeightfieldVoxel> spanVoxels;

        public AABB GetSpanBounds()
        {
            Vector3 min = Vector3.one;
            Vector3 max = Vector3.zero;

            foreach(var voxel in spanVoxels)
            {
                min = Vector3.Min(min, voxel.VoxelBounds.Min);
                max = Vector3.Max(max, voxel.VoxelBounds.Max);
            }

            return new AABB(min, max);
        }

        public HeightfieldSpan()
        {
            spanVoxels = new List<HeightfieldVoxel>();
        }
    }

    public class HeightfieldVoxel
    {
        Vector3[] vertices;

        public HeightFieldVoxelType type;

        public AABB VoxelBounds;

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

            bounds.size = new Vector3()
            {
                x = XZSize,
                y = YSize,
                z = XZSize
            };

            VoxelBounds = new AABB(bounds);

            intersectingTriangleNormal = Vector3.zero;
        }
    }
    public enum HeightFieldVoxelType
    {
        Solid,
        Open
    }

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
