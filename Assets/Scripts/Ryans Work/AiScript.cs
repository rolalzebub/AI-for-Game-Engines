using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FlockAgent))]
public class AiScript : MonoBehaviour
{
    private float speed = 3f;
    public int CurrentIndex;

    private Pathfinding MainRoom;
    public GameObject Player;
    public List<PathfindingNode> UsedPath;
    public List<Vector3> PathList;

    public LayerMask IsPlayer;
    public float SightRange;
    public bool CanSeePlayer;
    public bool CanMove = true;


    public BoxCollider roomForPathfinding;

    #region Flocking behaviour settings
    [Header("Flocking settings")]
    FlockAgent selfAgent;
    public FlockBehaviour flockingBehaviour;
    [Range(0.1f, 10f)]
    public float neighbourRadius = 0.1f;

    #endregion
    private void Start()
    {
        // Makes Grid taking in X, Z, CellSize, displacement
        //BossRoom = new Pathfinding(10, 16, 1, new Vector3(-13, 0, -12));
        MainRoom = new Pathfinding(Mathf.RoundToInt(roomForPathfinding.size.x), Mathf.RoundToInt(roomForPathfinding.size.z), 1, roomForPathfinding.gameObject.transform.position);
        Player = GameObject.Find("Player");
        selfAgent = GetComponent<FlockAgent>();
        selfAgent.SetFlockingBehaviour(flockingBehaviour);
    }

    private void Update()
    {
        CanSeePlayer = Physics.CheckSphere(transform.position, SightRange, IsPlayer);

        if (CanSeePlayer)
        {
            Player = GameObject.Find("Player");
            CanMove = false;
            SetTarget(Player.transform.position);
            Movement();
        }
    }

    private void Movement()
    {
        if (PathList != null)
        {
            Vector3 TargetPosition = PathList[CurrentIndex];
            if (Vector3.Distance(transform.position, TargetPosition) > 1f)
            {
                
                Vector3 MoveDirection = (TargetPosition - transform.position).normalized;
                Debug.Log("Move Direction" + MoveDirection);
                float DistanceBefore = Vector3.Distance(transform.position, TargetPosition);
                MoveDirection.y = 0.0f;
                selfAgent.Move(MoveDirection * speed);
                //transform.position += MoveDirection * speed * Time.deltaTime;
            }
            else
            {
                Debug.Log("Enemy close to target position");
                CurrentIndex++;
                if (CurrentIndex >= PathList.Count)
                {
                    Stop();
                    CanMove = true;
                }
            }

        }
        
    }
    private List<Transform> GetNearbyObjects()
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(transform.position, neighbourRadius);

        foreach (var c in contextColliders)
        {
            if (c != this.GetComponent<Collider>())
            {
                context.Add(c.transform);
            }
        }
        return context;
    }

    private void Stop()
    {
        PathList = null;
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    public void SetTarget(Vector3 Target)
    {
        CurrentIndex = 0;
        PathList = MainRoom.FindPath(GetPosition(), Target);

        if (PathList != null && PathList.Count > 1)
        {
            PathList.RemoveAt(0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, SightRange);
    }
}
