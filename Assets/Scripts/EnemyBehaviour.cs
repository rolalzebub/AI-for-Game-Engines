using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyBehaviour : MonoBehaviour
{
    BehaviourTree tree;
    //public GameObject diamond;
    //public GameObject van;
    //public GameObject door;
    public List<Leaf> LeafList;

    NavMeshAgent agent;


    private Pathfinding MainRoom;
    public GameObject Player;
    public List<Vector3> UsedPath;
    public Vector3 UsedPathCurrentPoint;
    public float SightRange;
    public LayerMask IsPlayer;
    public bool CanSeePlayer;
    public Sequence ToPlayer;
    public bool GeneratePath = true;

    public enum ActionState { IDLE, WORKING}
    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;

    // Start is called before the first frame update
    void Start()
    {
        MainRoom = new Pathfinding(12, 12, 1, new Vector3(-11, 0, 12));

        agent = this.GetComponent<NavMeshAgent>();


        // tree = new BehaviourTree();
        //Sequence steal = new Sequence("steal Something");
        //Leaf gotodoor = new Leaf("go to door", GoToDoor);        
        //Leaf gotoDiamond = new Leaf("Go to Diamond", GoToDiamond);
        //Leaf gotovan = new Leaf("Go to Van", GoToVan);


        //steal.AddChild(gotoDiamond);
        //steal.AddChild(gotodoor);
        //steal.AddChild(gotovan);
        // tree.AddChild(steal);

        // tree.printTree();   

 
    }

    /*
    public Node.Status GoToDiamond()
    {
        //return GoToLoction(diamond.transform.position);


    }

    //public Node.Status GoToDoor()
    {
        //return GoToLoction(door.transform.position);


    }
    public Node.Status GoToVan()
    {
        return GoToLoction(van.transform.position);

    }
    */

    public Node.Status GoToPoint()
    {
        return GoToLoction(UsedPathCurrentPoint);
    }
    

    Node.Status GoToLoction(Vector3 destination)
    {
        Debug.Log("gotolocation is called");
        float distanceToTarget = Vector3.Distance(destination, this.transform.position);
        if(state == ActionState.IDLE)
        {
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, destination) >= 1)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }
        else if (distanceToTarget < 0.1)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;

        }
        return Node.Status.RUNNING;


        
    }

    // Update is called once per frame
    void Update()
    {

        CanSeePlayer = Physics.CheckSphere(transform.position, SightRange, IsPlayer);
        if (CanSeePlayer )
        {
            //GeneratePath = false;
            FindPathToPlayer();
            CreateTreeToPlayer();
        }



        if (treeStatus == Node.Status.RUNNING)
            /*treeStatus =*/tree.Process();

    }



    void FindPathToPlayer()
    {
        // If player is in sight range
        MainRoom.GetGrid().GetXZ(Player.transform.position, out int x, out int z);
        int PlayerX = x;
        int PlayerZ = z;
        MainRoom.GetGrid().GetXZ(this.transform.position, out int Ex, out int Ez);
        int EnemyX = Ex;
        int EnemyZ = Ez;
        UsedPath = MainRoom.FindPath(transform.position, Player.transform.position);
        Debug.Log("Found Path To Player");

        for (int i = 0; i < UsedPath.Count; i++)
        {
            Debug.Log(UsedPath[i].x + " " + UsedPath[i].z);
        }

    }

    void CreateTreeToPlayer()
    {

        tree = new BehaviourTree();

        ToPlayer = new Sequence("Points To Get To Player");

        for (int i = 0; i < UsedPath.Count; i++)
        {
            UsedPathCurrentPoint = UsedPath[i];
            Debug.Log("Before");
            Leaf GoToLeaf = new Leaf("MovePosition", GoToPoint);
            Debug.Log("Middle");
            LeafList.Add(GoToLeaf); // It Doesnt like Something in this line tim 
            Debug.Log("After");
            ToPlayer.AddChild(LeafList[i]);
        }

        tree.AddChild(ToPlayer);
        tree.printTree();
    }

    // Draws visual spheres for the attack and sight for play testing.
    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SightRange);
    }

}
