using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBoundingBox
{
    #region Bounding Box Properties

    public Bounds bounds { get; private set; }

    public float Width { get { return bounds.max.x - bounds.min.x; } }

    public float Height { get { return bounds.max.y - bounds.min.y; } }

    public float Depth { get { return bounds.max.z - bounds.min.z; } }

    public float Area
    {
        get
        {
            return (bounds.max.x - bounds.min.x) * (bounds.max.y - bounds.min.y) * (bounds.max.z - bounds.min.z);
        }
    }

    public float SurfaceArea
    {
        get
        {
            Vector3 d = bounds.max - bounds.min;
            return 2.0f * (d.x * d.y + d.x * d.z + d.y * d.z);
        }
    }

    #endregion

    #region Constructors

    /// <param name="center">World coordinate center point of the AABB</param>
    /// <param name="size">Intended size as Vector3 of the AABB</param>
    public MeshBoundingBox(Vector3 center, Vector3 size)
    {
        bounds = new Bounds(center, size);
    }

    #endregion

    #region Utilities
    public void GetCorners(IList<Vector4> corners)
    {
        corners[0] = new Vector4(bounds.min.x, bounds.min.y, bounds.min.z, 1);
        corners[1] = new Vector4(bounds.min.x, bounds.min.y, bounds.max.z, 1);
        corners[2] = new Vector4(bounds.max.x, bounds.min.y, bounds.max.z, 1);
        corners[3] = new Vector4(bounds.max.x, bounds.min.y, bounds.min.z, 1);

        corners[4] = new Vector4(bounds.min.x, bounds.max.y, bounds.min.z, 1);
        corners[5] = new Vector4(bounds.min.x, bounds.max.y, bounds.max.z, 1);
        corners[6] = new Vector4(bounds.max.x, bounds.max.y, bounds.max.z, 1);
        corners[7] = new Vector4(bounds.max.x, bounds.max.y, bounds.min.z, 1);
    }

    #endregion
}