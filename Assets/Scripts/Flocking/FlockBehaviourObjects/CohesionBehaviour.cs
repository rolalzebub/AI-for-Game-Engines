using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Cohesion")]
public class CohesionBehaviour : FlockBehaviour
{
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context)
    {
        Vector3 cohesionMove = Vector3.zero;
        //if no neighbours, maintain course
        if (context.Count == 0)
        {
            return cohesionMove;
        }

        //add all points together and average if there are
        foreach(var item in context)
        {
            cohesionMove += item.position;
        }

        cohesionMove /= context.Count;

        //create offset from agent position
        cohesionMove -= agent.transform.position;

        return cohesionMove;
    }
}
