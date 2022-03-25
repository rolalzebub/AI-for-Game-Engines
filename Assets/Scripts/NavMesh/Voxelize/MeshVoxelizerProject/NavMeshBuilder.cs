using MeshVoxelizerProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshBuilder : MonoBehaviour
{
    public int size = 16;

    public bool drawAABBTree;

    public List<MeshFilter> objectsPartOfNavmesh;

    private void OnRenderObject()
    {
        var camera = Camera.current;

        if (drawAABBTree)
        {
            Matrix4x4 m = transform.localToWorldMatrix;

            foreach (Box3 box in MeshVoxelizer.Bounds)
            {
                DrawLines.DrawBounds(camera, Color.red, box, m);
            }
        }

    }

    public void DoYaThing()
    {

        foreach (var item in objectsPartOfNavmesh)
        {
            MeshFilter filter = item.GetComponent<MeshFilter>();
            MeshRenderer renderer = item.GetComponent<MeshRenderer>();

            if (filter == null || renderer == null)
            {
                filter = GetComponentInChildren<MeshFilter>();
                renderer = GetComponentInChildren<MeshRenderer>();
            }

            if (filter == null || renderer == null) return;

            renderer.enabled = false;

            Mesh mesh = filter.sharedMesh;
            Material mat = renderer.sharedMaterial;

            Box3 bounds = new Box3(mesh.bounds.min, mesh.bounds.max);

            MeshVoxelizer.Init(size, size, size);
            MeshVoxelizer.Voxelize(mesh.vertices, mesh.triangles, bounds);

            Vector3 scale = new Vector3(bounds.Size.x / size, bounds.Size.y / size, bounds.Size.z / size);
            Vector3 m = new Vector3(bounds.Min.x, bounds.Min.y, bounds.Min.z);
            mesh = MeshVoxelizer.CreateMesh(MeshVoxelizer.Voxels, scale, m);

            GameObject go = new GameObject("Voxelized");
            go.transform.parent = transform;
            go.transform.localPosition = filter.gameObject.transform.localPosition;
            go.transform.localScale = filter.gameObject.transform.localScale;
            go.transform.localRotation = filter.gameObject.transform.rotation;

            filter = go.AddComponent<MeshFilter>();
            renderer = go.AddComponent<MeshRenderer>();

            filter.sharedMesh = mesh;
            renderer.sharedMaterial = mat;
        }
    }
}
