using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarPath : MonoBehaviour
{

    public float MoveSpeed;
    public float TurnSpeed;
    public float Acceleration;

    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public List<Vector3> VisitedNodes;
    public List<Vector3> ToVisitedNodes;

    public GameObject Player;

    //Nodes from start
    public int GCost;
    //GCost + HCost
    public int FCost;
    //Nodes from Target
    public int HCost;

    private void Awake()
    {
        StartPosition = GetComponent<Transform>().position;
        EndPosition = Player.GetComponent<Transform>().position;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
