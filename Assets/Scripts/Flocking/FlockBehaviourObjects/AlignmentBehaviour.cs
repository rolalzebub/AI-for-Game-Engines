using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Alignment")]
public class AlignmentBehaviour : FlockBehaviour
{
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context)
    {
        Vector3 alignmentMove = Vector3.zero;
        //if no neighbours, maintain course
        if (context.Count == 0)
        {
            return Vector3.forward;
        }

        //add all points together and average if there are
        foreach (var item in context)
        {
            alignmentMove += Vector3.forward * item.transform.position.z;
        }

        alignmentMove /= context.Count;


        return alignmentMove;
    }
}
