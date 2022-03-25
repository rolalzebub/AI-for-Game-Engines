using RadicalForge.Blockout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public FlockAgent agentPrefab;
    List<FlockAgent> agents = new List<FlockAgent>();
    public FlockBehaviour behaviour;

    [Range(10, 500)]
    public int startingCount = 250;
    const float AgentDensity = 0.08f;

    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(0.1f, 10f)]
    public float neighbourRadius = 0.1f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    [Range(0.1f, 10f)]
    public float spawnCircleMultiplier = 0.5f;

    float squaredMaxSpeed, squaredNeighbourRadius, squaredAvoidanceRadius;
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
        squaredMaxSpeed = maxSpeed * maxSpeed;
        squaredNeighbourRadius = neighbourRadius * neighbourRadius;
        squaredAvoidanceRadius = squaredNeighbourRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        SpawnFlock();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var agent in agents)
        {
            List<Transform> context = GetNearbyObjects(agent);
            
            
            Vector3 move = behaviour.CalculateMove(agent, context, this);

            move *= driveFactor;

            if (move.sqrMagnitude > squaredMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }
            agent.Move(move);
        }
    }

    private List<Transform> GetNearbyObjects(FlockAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(agent.transform.position, neighbourRadius);

        foreach (var c in contextColliders)
        {
            if (c != agent.AgentCollider && c.GetComponent<BlockoutHelper>() == null)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }

    public void SpawnFlock()
    {
        ClearOldFlock();

        for (int i = 0; i < startingCount; i++)
        {
            Vector2 rngResult = Random.insideUnitCircle * spawnCircleMultiplier;
            Vector3 newAgentPosition = new Vector3
            {
                x = rngResult.x * startingCount * AgentDensity + transform.position.x,
                y = transform.position.y,
                z = rngResult.y * startingCount * AgentDensity + transform.position.z
            };
            FlockAgent newAgent = Instantiate(agentPrefab,
                newAgentPosition,
                Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)),
                transform);
            newAgent.name = "Agent " + i;
            agents.Add(newAgent);
        }
    }

    void ClearOldFlock()
    {
        foreach(var agent in agents)
        {
            Destroy(agent);
        }
    }
}
