using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heightfield
{
    public float XZCellSize;
    public float YCellSize;

    public List<Vector3> verts;
    public int gridRows;
    public int gridColumns;

    public List<List<HeightfieldCell>> heightfieldData;
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

            var angle = Vector3.Angle(tri.Normal, Vector3.up);

            //mark triangle as walkable if it is
            if (angle >= 0 && angle < maxWalkableSlopeAngle)
            {
                walkableTris.Add(tri);
            }
        }


        for (int j = 0; j < verts.Count; j += 8)
        {
            Vector3[] voxelVerts = new Vector3[8]
            {
                verts[j], verts[j+1],
                verts[j+2], verts[j+3],
                verts[j+4], verts[j+5],
                verts[j+6], verts[j+7]
            };

            AABB voxel = new AABB(verts[j], verts[j + 6]);

            foreach (var tri in walkableTris)
            {
                //check for triangle voxel intersection
                Debug.Log(Intersections.Intersects(tri, voxel));

                //HeightfieldVoxel voxelTest = new HeightfieldVoxel(voxelVerts, XZCellSize, YCellSize);
                //bool result = voxelTest.CheckTriangleIntersection(tri.verts[0], tri.verts[1], tri.verts[2]);
                //Debug.Log(result);

                //if (result)
                //{
                //    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //    go.transform.position = voxelTest.bounds.center;
                //    go.transform.localScale = voxelTest.bounds.size;
                //}

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
class HeightfieldVoxel
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

    /// <summary>
    /// Check if the triangle defined in the params intersects this voxel
    /// </summary>
    public bool CheckTriangleIntersection(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        /*    use separating axis theorem to test overlap between triangle and box */
        /*    need to test for overlap in these directions: */
        /*    1) the {x,y,z}-directions (actually, since we use the AABB of the triangle */
        /*       we do not even need to test these) */
        /*    2) normal of the triangle */
        /*    3) crossproduct(edge from tri, {x,y,z}-directin) */
        /*       this gives 3x3=9 more tests */

        //simplest test
        //check if any of the points of the triangle lie inside the cube
        
        if (bounds.Contains(p1) || bounds.Contains(p2) || bounds.Contains(p3))
            return true;
     

        Vector3 v0, v1, v2;
        float min, max, rad, fex, fey, fez;
        Vector3 p;
        Vector3 normal, e0, e1, e2;

        //FIRST TEST
        //Check if the AABB of the triangle intersects with the voxel

        ////get the triangle points relative to box center
        v0 = bounds.center - p1;
        v1 = bounds.center - p2;
        v2 = bounds.center - p3;

        /* test in X-direction */
        //reset
        min = float.PositiveInfinity;
        max = float.NegativeInfinity;

        min = Mathf.Min(v0.x, v1.x, v2.x, min);
        max = Mathf.Max(v0.x, v1.x, v2.x, max);

        if (min > bounds.size.x || max < -bounds.size.x)
            return false;

        /* test in Y-direction */
        //reset
        min = float.PositiveInfinity;
        max = float.NegativeInfinity;

        min = Mathf.Min(v0.y, v1.y, v2.y, min);
        max = Mathf.Max(v0.y, v1.y, v2.y, max);

        if (min > bounds.size.y || max < -bounds.size.y)
            return false;

        /* test in Z-direction */
        //reset
        min = float.PositiveInfinity;
        max = float.NegativeInfinity;

        min = Mathf.Min(v0.z, v1.z, v2.z, min);
        max = Mathf.Max(v0.z, v1.z, v2.z, max);

        if (min > bounds.size.z || max < -bounds.size.z)
            return false;

        /*    2) */
        /*    test if the box intersects the plane of the triangle */
        /*    compute plane equation of triangle: normal*x+d=0 */


        //compute triangle data
        //edges
        e0 = v1 - v0;
        e1 = v2 - v1;
        e2 = v0 - v2;
        //normal
        normal = Vector3.Cross(e0, e1);

        if (!PlaneBoxOverlapTest(normal, -Vector3.Dot(normal, v0), bounds))
            return false;

        #region first edge Tests
            fex = Mathf.Abs(e0.x);
            fey = Mathf.Abs(e0.y);
            fez = Mathf.Abs(e0.z);

            //x axis
            p.x = e0.z * v0.y - e0.y * v0.z;
            p.z = e0.z * v2.y - e0.y * v2.z;

            if (p.x < p.z)
            {
                min = p.x;
                max = p.z;
            }
            else
            {
                min = p.z;
                max = p.x;
            }

            rad = fez * bounds.size.y + fey * bounds.size.z;

            if (min < rad || max < -rad)
                return false;

            //y axis
            p.x = -e0.z * v0.x + e0.y * v0.z;
            p.z = -e0.z * v2.x + e0.y * v2.z;
            if (p.x < p.z)
            {
                min = p.x;
                max = p.z;
            }
            else
            {
                min = p.z;
                max = p.x;
            }

            rad = fez * bounds.size.x + fex * bounds.size.z;

            if (min > rad || max < -rad)
                return false;


            //z axis
            p.y = e0.y * v1.x - e0.x * v1.y;
            p.z = e0.y * v2.x - e0.x * v2.y;

            if (p.z < p.y)
            {
                min = p.z;
                max = p.y;
            }
            else
            {
                min = p.y;
                max = p.z;
            }

            rad = fey * bounds.size.x + fex * bounds.size.y;

            if (min > rad || max < -rad)
                return false;

        #endregion

        #region second edge Tests
            fex = Mathf.Abs(e1.x);
            fey = Mathf.Abs(e1.y);
            fez = Mathf.Abs(e1.z);

            //x axis
            p.x = e1.z * v0.y - e1.y * v0.z;
            p.z = e1.z * v2.y - e1.y * v2.z;

            if (p.x < p.z)
            {
                min = p.x;
                max = p.z;
            }
            else
            {
                min = p.z;
                max = p.x;
            }

            rad = fez * bounds.size.y + fey * bounds.size.z;

            if (min < rad || max < -rad)
                return false;

            //y axis
            p.x = -e1.z * v0.x + e1.y * v0.z;
            p.z = -e1.z * v2.x + e1.y * v2.z;
            if (p.x < p.z)
            {
                min = p.x;
                max = p.z;
            }
            else
            {
                min = p.z;
                max = p.x;
            }

            rad = fez * bounds.size.x + fex * bounds.size.z;

            if (min > rad || max < -rad)
                return false;


            //z axis
            p.x = e1.y * v0.x - e1.x * v0.y;
            p.y = e1.y * v1.x - e1.x * v1.y;

            if(p.x < p.y)
            {
                min = p.x;
                max = p.y;
            }
            else
            {
                min = p.y;
                max = p.x;
            }

        #endregion

        #region third edge Tests
            fex = Mathf.Abs(e2.x);
            fey = Mathf.Abs(e2.y);
            fez = Mathf.Abs(e2.z);

            //x axis
            p.x = e2.z * v0.y - e2.y * v0.z;
            p.y = e2.z * v1.y - e2.y * v1.z;

            if(p.x < p.y)
            {
                min = p.x;
                max = p.y;
            }
            else
            {
                min = p.y;
                max = p.x;
            }
            rad = fez * bounds.size.y + fey * bounds.size.z;

            if (min > rad || max < -rad)
                return false;
        #endregion

        return true;
    }

    bool PlaneBoxOverlapTest(Vector3 normal, float d, Bounds bounds)
    {
        Vector3 vmin, vmax;
        if (normal.x > 0.0f)
        {
            vmin.x = -bounds.size.x;
            vmax.x = bounds.size.x;
        }
        else
        {
            vmin.x = bounds.size.x;
            vmax.x = -bounds.size.x;
        }

        if (normal.y > 0.0f)
        {
            vmin.y = -bounds.size.y;
            vmax.y = bounds.size.y;
        }
        else
        {
            vmin.y = bounds.size.y;
            vmax.y = -bounds.size.y;
        }

        if (normal.z > 0.0f)
        {
            vmin.z = -bounds.size.z;
            vmax.z = bounds.size.z;
        }
        else
        {
            vmin.z = bounds.size.z;
            vmax.z = -bounds.size.z;
        }

        if (Vector3.Dot(normal, vmin) + d > 0.0f)
            return false;

        if (Vector3.Dot(normal, vmax) + d >= 0.0f)
            return true;


        return false;
    }
}

public enum HeightFieldVoxelType
{
    Solid,
    Open
}