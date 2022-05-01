using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class FlockAgent : MonoBehaviour
{
    Collider agentCollider;
    
    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(0.1f, 10f)]
    public float neighbourRadius = 0.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;

    float squaredMaxSpeed, squaredNeighbourRadius, squaredAvoidanceRadius;

    FlockBehaviour behaviour;

    NavMeshPath pathToFollow;
    bool isFollowingPath = false;
    int currentPathGoalIndex = 0;
    Vector3 currentPathGoal;
    public float goalToleranceDistance = 1f;

    float speed;

    public Collider AgentCollider
    {
        get
        {
            return agentCollider;
        }
    }

    public float SquaredAvoidanceRadius
    {
        get
        {
            return squaredAvoidanceRadius;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        speed = UnityEngine.Random.Range(2f, 5f);
        agentCollider = GetComponent<Collider>(); 
        squaredMaxSpeed = maxSpeed * maxSpeed;
        squaredNeighbourRadius = neighbourRadius * neighbourRadius;
        squaredAvoidanceRadius = squaredNeighbourRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
    }


    private void Update()
    {
        if(isFollowingPath)
        {
            Vector3 directionToMove = currentPathGoal - transform.position;
            Move(directionToMove);
        }
    }

    public void Move(Vector3 direction)
    {
        Vector3 move = direction;

        Vector3 behaviourMove = behaviour.CalculateMove(this, GetNearbyObjects());

        behaviourMove *= driveFactor;

        if (behaviourMove.sqrMagnitude > squaredMaxSpeed)
        {
            behaviourMove = behaviourMove.normalized * maxSpeed;
        }

        move += behaviourMove;
        move = Vector3.RotateTowards(move, behaviourMove, 0.01f, 0.1f);

        if (move != Vector3.zero)
        {
            transform.forward = move.normalized;

            transform.position += transform.forward * speed * Time.deltaTime;
        }

        CheckForDestination();
    }

    private void CheckForDestination()
    {
        float dst = Mathf.Abs((currentPathGoal - transform.position).magnitude);

        if(dst <= goalToleranceDistance)
        {
            if(currentPathGoalIndex < pathToFollow.corners.Length - 1)
            {
                currentPathGoalIndex++;
                currentPathGoal = pathToFollow.corners[currentPathGoalIndex];
            }
            else
            {
                DestinationReached();
            }
        }
    }


    private void DestinationReached()
    {
        isFollowingPath = false;
        StartCoroutine(GetRandomNewDestination());
    }

    private List<Transform> GetNearbyObjects()
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(transform.position, neighbourRadius);

        foreach (var c in contextColliders)
        {
            var nextAgent = c.GetComponentInParent<FlockAgent>();
            if (c != AgentCollider && nextAgent != null)
            {
                context.Add(c.transform);
            }
        }

        return context;
    }

    public void SetFlockingBehaviour(FlockBehaviour _behaviour)
    {
        behaviour = _behaviour;
    }

    public void SetNewDestination(Vector3 dest)
    {
        pathToFollow = new NavMeshPath();
        isFollowingPath = NavMesh.CalculatePath(transform.position, dest, NavMesh.AllAreas, pathToFollow);

        if(isFollowingPath)
        {
            currentPathGoal = pathToFollow.corners[0];
            currentPathGoalIndex = 0;
        }
        else
        {
            currentPathGoal = Vector3.zero;
        }
    }

    IEnumerator GetRandomNewDestination()
    {
        Vector3 newDest = FindObjectOfType<FlockTesting>().GetRandomPointOnNavmesh();

        while(newDest == Vector3.negativeInfinity)
        {
            newDest = FindObjectOfType<FlockTesting>().GetRandomPointOnNavmesh();
        }

        SetNewDestination(newDest);

        yield return null;
    }

}
