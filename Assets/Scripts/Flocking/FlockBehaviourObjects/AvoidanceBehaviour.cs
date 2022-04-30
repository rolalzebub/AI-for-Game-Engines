using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Avoidance")]
public class AvoidanceBehaviour : FlockBehaviour
{
    [Range(0.1f, 10f)]
    public float neighbourRadius = 0.1f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;

    float squaredAvoidanceRadius;

    public AvoidanceBehaviour()
    {
        squaredAvoidanceRadius = neighbourRadius * neighbourRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
    }

    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context)
    {
        Vector3 avoidanceMove = Vector3.zero;
        int nAvoid = 0;
        //if no neighbours, maintain course
        if (context.Count == 0)
        {
            return avoidanceMove;
        }

        //add all points together and average if there are
        foreach (var item in context)
        {
            if(Vector3.SqrMagnitude(item.position-agent.transform.position) < squaredAvoidanceRadius)
            {
                nAvoid++;
                avoidanceMove += agent.transform.position - item.position ;
            }
        }

        if(nAvoid > 0)
        {
            avoidanceMove /= nAvoid;
        }

        return avoidanceMove;
    }
}
