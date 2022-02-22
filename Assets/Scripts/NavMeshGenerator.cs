using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshGenerator : MonoBehaviour
{
    public GameObject worldFloor;
    public List<GameObject> worldBoundary;
    public List<GameObject> worldObstacles;

    public float floorLevel = 0f;
    public float wallSize = 1f;
    public float maxWalkableSlopeAngle = 45f;

    private void Start()
    {
        GetFloorConvexHull();
    }

    void GetFloorConvexHull()
    {
        List<Vector3> floorSurface = new List<Vector3>();

        var verts = worldFloor.GetComponent<MeshFilter>().mesh.vertices;
        var triangles = worldFloor.GetComponent<MeshFilter>().mesh.triangles;
        for (int i = 0; i < triangles.Length; i+=3)
        {
            var edge1 = worldFloor.transform.TransformPoint(verts[triangles[i + 1]]) - worldFloor.transform.TransformPoint(verts[triangles[i]]);
            var edge2 = worldFloor.transform.TransformPoint(verts[triangles[i + 2]]) - worldFloor.transform.TransformPoint(verts[triangles[i]]);

            var triangleNormal = Vector3.Cross(edge1, edge2);
            triangleNormal = triangleNormal.normalized;

            //place verts for all upward facing triangles
            var angle = Vector3.Angle(triangleNormal, Vector3.up);
            if (angle >= 0 && 
                angle < maxWalkableSlopeAngle)
            {
                var vert1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var vert2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var vert3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                
                vert1.transform.position = worldFloor.transform.TransformPoint(verts[triangles[i]]);
                vert2.transform.position = worldFloor.transform.TransformPoint(verts[triangles[i + 1]]);
                vert3.transform.position = worldFloor.transform.TransformPoint(verts[triangles[i + 2]]);
                


                vert1.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                vert2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                vert3.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
    }
}


