using MeshVoxelizerProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelizeScene : MonoBehaviour
{
    List<SceneMeshObject> allSceneMeshes;
    public int gridResolution;
    public float XZCellSize;
    public float YCellSize;
    public float walkableSlopeAngle = 45.0f;

    Heightfield sceneField;

    public void VoxelizeSceneByCombiningMeshes()
    {
        allSceneMeshes = new List<SceneMeshObject>(FindObjectsOfType<SceneMeshObject>());

        Mesh sceneMesh = new Mesh();
        CombineInstance[] meshesToCombine = new CombineInstance[allSceneMeshes.Count];
        for(int i = 0; i < allSceneMeshes.Count; i++)
        {
            meshesToCombine[i].mesh = allSceneMeshes[i].GetMesh().sharedMesh;
            meshesToCombine[i].transform = allSceneMeshes[i].GetMesh().transform.localToWorldMatrix;

        }
        sceneMesh.CombineMeshes(meshesToCombine);

        sceneField = new Heightfield(XZCellSize, YCellSize, walkableSlopeAngle);
        sceneField.CreateHeightFieldGrid(sceneMesh.bounds, walkableSlopeAngle);
        sceneField.CheckHeightfieldAgainstMesh(sceneMesh);

    }

    private void OnDrawGizmos()
    {
        if (sceneField != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(sceneField.sceneBounds.center, sceneField.sceneBounds.size);
        }
       
    }

}

