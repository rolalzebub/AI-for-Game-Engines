using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Composite")]
public class CompositeBehaviour : FlockBehaviour
{
    public FlockBehaviour[] behaviours;
    public float[] weights;

    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        if(weights.Length != behaviours.Length)
        {
            Debug.LogError("Data incorrect in " + name, this);
            return Vector3.zero;
        }

        //setup move
        Vector3 move = Vector3.zero;

        //iterate through behaviours
        for(int i = 0; i < behaviours.Length; i++)
        {
            Vector3 partialMove = behaviours[i].CalculateMove(agent, context, flock) * weights[i];

            if(partialMove != Vector3.zero)
            {
                if(partialMove.sqrMagnitude > weights[i] * weights[i])
                {
                    partialMove = partialMove.normalized * weights[i];
                }

                move += partialMove;
            }
        }

        return move;
        
    }

}
