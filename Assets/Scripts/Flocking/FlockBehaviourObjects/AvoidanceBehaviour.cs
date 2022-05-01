using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Avoidance")]
public class AvoidanceBehaviour : FlockBehaviour
{
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
            if(Vector3.SqrMagnitude(item.position - agent.transform.position) < agent.SquaredAvoidanceRadius)
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
