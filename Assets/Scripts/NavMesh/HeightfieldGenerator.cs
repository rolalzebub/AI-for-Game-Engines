using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightfieldGenerator
{
    public static HField GenerateHeightfield(Mesh sceneMeshComposite, float XZCellSize, float YCellSize)
    {
        //Step 1: Generate a grid of vertices
        // These vertices will form our voxels
        Vector3[,,] gridVertices = CreateHeightFieldGrid(sceneMeshComposite.bounds, XZCellSize, YCellSize);



        HField generated = new HField();
        return generated;
    }

    private static Vector3[,,] CreateHeightFieldGrid(Bounds _sceneBounds, float XZCellSize, float YCellSize)
    {
        int XZCellsCount = Mathf.CeilToInt(_sceneBounds.size.x / XZCellSize);
        int YCellsCount = Mathf.CeilToInt(_sceneBounds.size.y / YCellSize);

        var gridRows = XZCellsCount + 1;
        var gridColumns = YCellsCount + 1;

        var verts = new Vector3[gridRows, gridColumns, gridRows];

        Vector3 startPos = new Vector3();
        bool firstVert = false;

        //create vertices to represent the heightfield grid
        for (int xIndex = 0; xIndex < gridRows; xIndex++)
        {
            for (int yIndex = 0; yIndex < gridColumns; yIndex++)
            {
                for (int zIndex = 0; zIndex < gridRows; zIndex++)
                {
                    //special case handling for first vertex of the grid
                    if (!firstVert)
                    {
                        startPos = _sceneBounds.min - new Vector3(XZCellSize / 2, YCellSize / 2, XZCellSize / 2);
                        verts[0, 0, 0] = startPos;
                        firstVert = true;
                    }
                    else
                    {
                        Vector3 newVert = new Vector3()
                        {
                            x = startPos.x + (xIndex * XZCellSize),
                            y = startPos.y + (yIndex * YCellSize),
                            z = startPos.z + (zIndex * XZCellSize)
                        };
                        verts[xIndex, yIndex, zIndex] = newVert;
                    }
                }
            }
        }

        return verts;
    }

    /*private static void CheckHeightfieldAgainstTriangles(Triangle[] walkableTriangles, Mesh sceneMesh)
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

                    for (int i = 0; i < sceneMesh.triangles.Length; i += 3)
                    {
                        Triangle tri = new Triangle(sceneMesh.vertices[sceneMesh.triangles[i]], sceneMesh.vertices[sceneMesh.triangles[i + 1]], sceneMesh.vertices[sceneMesh.triangles[i + 2]]);

                        bool result = Intersections.Intersects(tri, voxel.VoxelBounds);

                        voxel.isWalkable = trianglesList.Contains(tri);

                        if (result)
                        {
                            nVoxel.isClosed = true;
                            nVoxel.intersectingTriangleNormal = tri.Normal;
                            generatedVoxels[xIndex, yIndex, zIndex] = nVoxel;

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

    }*/
}