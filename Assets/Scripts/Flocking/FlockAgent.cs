using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class FlockAgent : MonoBehaviour
{
    Collider agentCollider;
    public Vector3 currentVelocity;

    public Collider AgentCollider
    {
        get
        {
            return agentCollider;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        agentCollider = GetComponent<Collider>();
    }

    public void Move(Vector3 velocity)
    {
        currentVelocity += velocity;
        transform.forward = currentVelocity.normalized;
        transform.position += transform.forward * Time.deltaTime;
    }
}
