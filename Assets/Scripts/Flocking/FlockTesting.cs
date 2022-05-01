using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlockTesting : MonoBehaviour
{
    const float AgentDensity = 0.08f;

    [Range(10, 500)]
    public int startingCount = 250;
    [Range(0.1f, 10f)]
    public float spawnCircleMultiplier = 0.5f;

    public FlockAgent agentPrefab;
    List<FlockAgent> agents = new List<FlockAgent>();
    public FlockBehaviour behaviour;

    Queue<Vector3> randomPoints = new Queue<Vector3>();
    bool isComputeRunning = false;
    float randomPointMaxDistance = 10f;

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
            newAgent.SetFlockingBehaviour(behaviour);
            newAgent.name = "Agent " + i;
            agents.Add(newAgent);
        }
    }

    void ClearOldFlock()
    {
        foreach (var agent in agents)
        {
            Destroy(agent);
        }
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitData;
            if (Physics.Raycast(ray, out hitData, Camera.main.farClipPlane))
            {
                SetGoalForAllAgents(hitData.point);
            }
        }
    }

    private void Start()
    {
        SpawnFlock();
    }

    public Vector3 GetRandomPointOnNavmesh()
    {
        if(randomPoints.Count > 0)
        {
            return randomPoints.Dequeue();
        }
        else if(!isComputeRunning)
        {
            StartCoroutine(PrecomputeRandomPointsOnNavmesh());
            
            new WaitUntil(() => { return randomPoints.Count > 0; });

            return randomPoints.Dequeue();
        }

        return Vector3.negativeInfinity;
    }

    public void SetGoalForAllAgents(Vector3 mouseClickWorldPosition)
    {
        
        foreach(var agent in agents)
        {
            Vector3 destination = new Vector3(mouseClickWorldPosition.x, agent.transform.position.y, mouseClickWorldPosition.z);
            agent.SetNewDestination(destination);
        }
    }

    IEnumerator PrecomputeRandomPointsOnNavmesh()
    {
        isComputeRunning = true;

        while(randomPoints.Count < startingCount)
        {
            bool canReachPoint = false;

            while (!canReachPoint)
            {
                NavMeshHit hit = new NavMeshHit(); // NavMesh Sampling Info Container

                bool foundPosition = false;

                while (!foundPosition)
                {
                    foundPosition = NavMesh.SamplePosition(transform.position + Random.insideUnitSphere * randomPointMaxDistance, out hit, randomPointMaxDistance, NavMesh.AllAreas);
                }

                NavMeshPath path = new NavMeshPath();

                GetComponent<NavMeshAgent>().CalculatePath(hit.position, path);

                canReachPoint = path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial;

                if (canReachPoint)
                    randomPoints.Enqueue(hit.position);

            }
        }

        isComputeRunning = false;
        yield return null;
    }
}
