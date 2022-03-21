using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heightfield
{
    public float XZCellSize;
    public float YCellSize;

    public List<List<List<Vector3>>> verts;
    public int gridRows;
    public int gridColumns;

    public List<List<HeightfieldVoxel>> heightfieldData;
    public float maxWalkableSlopeAngle;
    public Heightfield(float _XZCellSize, float _YCellSize, float _maxWalkableSlopeAngle)
    {
        XZCellSize = _XZCellSize;
        YCellSize = _YCellSize;
        maxWalkableSlopeAngle = _maxWalkableSlopeAngle;
    }

    public void CheckHeightfieldAgainstMesh(Mesh meshToCheck)
    {
        List<Triangle> walkableTris = new List<Triangle>();
        //check each triangle against each cube
        for (int i = 0; i < meshToCheck.triangles.Length; i += 3)
        {
            Triangle tri = new Triangle(meshToCheck.vertices[meshToCheck.triangles[i]],
                    meshToCheck.vertices[meshToCheck.triangles[i + 1]],
                    meshToCheck.vertices[meshToCheck.triangles[i + 2]]);

            //var angle = Vector3.Angle(tri.Normal, Vector3.up);

            //mark triangle as walkable if it is
            //if (angle >= 0 && angle < maxWalkableSlopeAngle)
            {
                walkableTris.Add(tri);
            }
        }

        var head = new GameObject();
        for (int xIndex = 0; xIndex < gridRows  - 1; xIndex++)
        {
            for(int yIndex = 0; yIndex < gridColumns - 1; yIndex++)
            {
                for(int zIndex = 0; zIndex < gridRows - 1; zIndex++)
                {
                    Vector3[] voxelVerts = new Vector3[8]
                    {
                        verts[xIndex][yIndex][zIndex], verts[xIndex][yIndex][zIndex + 1],
                        verts[xIndex+1][yIndex][zIndex+1], verts[xIndex+1][yIndex][zIndex],
                        verts[xIndex][yIndex+1][zIndex], verts[xIndex][yIndex+1][zIndex + 1],
                        verts[xIndex+1][yIndex+1][zIndex+1], verts[xIndex+1][yIndex+1][zIndex]
                    };

                    AABB voxel = new AABB(verts[xIndex][yIndex][zIndex], verts[xIndex + 1][yIndex + 1][zIndex + 1]);

                    foreach (var tri in walkableTris)
                    {
                        //check for triangle voxel intersection#
                        bool result = (Intersections.Intersects(tri, voxel));

                        if (result)
                        {
                            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            go.transform.localScale = new Vector3(XZCellSize, YCellSize, XZCellSize);
                            
                            go.transform.position = voxel.Center;
                            go.transform.parent = head.transform;
                        }

                    }
                }
            }
        }
    }
}

public class HeightfieldCell
{
    public List<HeightfieldSpan> heightfieldSpans;
}

public class HeightfieldSpan
{
    public HeightFieldVoxelType type;
    public int spanVoxelCount;
    public int spanStartingRow;
}

/// <summary>
/// One singular voxel in the heightfield
/// </summary>
public class HeightfieldVoxel
{
    public Vector3[] vertices;
    public Bounds bounds;
    public HeightFieldVoxelType type;

    public HeightfieldVoxel(Vector3[] _vertices, float XZSize, float YSize)
    {
        vertices = _vertices;
        bounds = new Bounds();

        bounds.min = bounds.max = vertices[0];
        
        foreach(var item in vertices)
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
    }
}

public enum HeightFieldVoxelType
{
    Solid,
    Open
}