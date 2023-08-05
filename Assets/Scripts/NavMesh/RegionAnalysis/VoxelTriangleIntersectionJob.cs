using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VoxelTriangleIntersectionJob : IJobParallelFor
{
    public NativeArray<SimpleVoxel> voxelGrid;
    
    public NativeArray<SimpleTriangle> triangles;

    public void Execute(int index)
    {
        var voxelToCheck = voxelGrid[index];

        foreach(var tri in triangles)
        {
            var triangleToCheck = new Triangle(tri.A, tri.B, tri.C);

            if(Intersections.Intersects(triangleToCheck, new AABB(voxelToCheck.voxelBounds)))
            {
                //voxelToCheck.type = HeightFieldVoxelType.Closed;
                //voxelToCheck.intersectingTriangleNormal = triangleToCheck.Normal;

                voxelGrid[index] = voxelToCheck;
                break;
            }
        }
    }
}

public struct SimpleTriangle
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
}

public struct SimpleVoxel
{
    public NativeArray<Vector3> vertices;

    public bool isClosed;

    public Bounds voxelBounds;

    public Vector3 intersectingTriangleNormal;

}
