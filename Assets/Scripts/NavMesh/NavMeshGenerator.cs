using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshGenerator : MonoBehaviour
{
    public GameObject worldFloor;
    public List<GameObject> worldBoundary;
    public List<GameObject> worldObstacles;
    Mesh currentNavMesh;
    public float floorLevel = 0f;
    public float wallSize = 1f;
    public float maxWalkableSlopeAngle = 45f;
    
    private void Start()
    {
        currentNavMesh = new Mesh();
        CreateFloorNavMesh();
    }

    void CreateFloorNavMesh()
    {
        List<Vector3> floorSurface = new List<Vector3>();

        var verts = worldFloor.GetComponent<MeshFilter>().mesh.vertices;
        var triangles = worldFloor.GetComponent<MeshFilter>().mesh.triangles;
        var normals = worldFloor.GetComponent<MeshFilter>().mesh.normals;
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
                
                //create vertices for the triangle and add them to the mesh

                MeshVertex newVert1 = new MeshVertex();
                newVert1.position = worldFloor.transform.TransformPoint(verts[triangles[i]]);
                newVert1.normal = normals[triangles[i]];
                newVert1.meshVertexArrayIndex = triangles[i];
                CreateMeshVertexWithDebugSphere(ref currentNavMesh, newVert1);

                MeshVertex newVert2 = new MeshVertex();
                newVert2.position = worldFloor.transform.TransformPoint(verts[triangles[i+1]]);
                newVert2.normal = normals[triangles[i+1]];
                newVert2.meshVertexArrayIndex = triangles[i + 1];
                CreateMeshVertexWithDebugSphere(ref currentNavMesh, newVert2);

                MeshVertex newVert3 = new MeshVertex();
                newVert3.position = worldFloor.transform.TransformPoint(verts[triangles[i+2]]);
                newVert3.normal = normals[triangles[i+2]];
                newVert3.meshVertexArrayIndex = triangles[i + 2];
                CreateMeshVertexWithDebugSphere(ref currentNavMesh, newVert3);
            }
        }
    }

    void CreateMeshVertexWithDebugSphere(ref Mesh meshToAddVertexTo, MeshVertex vertexToAdd)
    {
        List<Vector3> currentVerts = new List<Vector3>(meshToAddVertexTo.vertices);
        currentVerts.Add(vertexToAdd.position);
        meshToAddVertexTo.SetVertices(currentVerts);


        List<Vector3> currentNormals = new List<Vector3>(meshToAddVertexTo.normals);
        if (currentNormals.Count < currentVerts.Count)
        {
            currentNormals.Add(vertexToAdd.normal);
        }
        else
        {
            currentNormals[meshToAddVertexTo.vertexCount - 1] = vertexToAdd.normal;
        }
        meshToAddVertexTo.SetNormals(currentNormals);
        
        var debugVert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugVert.transform.position = vertexToAdd.position;
        debugVert.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    struct MeshVertex
    {
        /// <summary>
        /// World space position of the vertex
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Vertex Normal
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// Index of vertex in the mesh's array of vertices
        /// </summary>
        public int meshVertexArrayIndex;
    }
}


