using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Constraint assumptions: All objects in the input are convex
//Each region must maintain owenrship of the free area it covers
public class ASFV3DNavMesh : MonoBehaviour
{
    public float AgentRadius = 0.5f;
    public float AgentHeight = 2f;
    public float MaxWalkableSlopeAngle = 45f;
    public float MaxStepHeight = 0.4f;

    public void FindWalkableSpaces()
    {

    }

    void SeedWorld()
    {
        var meshesInWorld = FindObjectsOfType<MeshRenderer>();

        var currentMaxBounds = meshesInWorld.FirstOrDefault().bounds;

        var furthestXObject = meshesInWorld.Where(x => x.bounds.max.x > currentMaxBounds.max.x).FirstOrDefault();
        var HighestYObject = meshesInWorld.Where(x => x.bounds.max.y > currentMaxBounds.max.y).FirstOrDefault();
        var FurthestZObject = meshesInWorld.Where(x => x.bounds.max.z > currentMaxBounds.max.z).FirstOrDefault();

        foreach(var mesh in meshesInWorld)
        {

        }
    }
}
