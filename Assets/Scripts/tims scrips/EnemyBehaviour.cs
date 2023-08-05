using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyBehaviour : MonoBehaviour
{
    BehaviourTree tree;
    public GameObject diamond;
    public GameObject van;
    public GameObject door;
    public GameObject backDoor;
    public List<Leaf> LeafList;
   /* 
    private Pathfinding MainRoom;
    public GameObject Player;
    public List<Vector3> UsedPath;
    public Vector3 UsedPathCurrentIndex;

    public float SightRange;
    public LayerMask IsPlayer;
    public bool CanSeePlayer;
    */


    NavMeshAgent agent;

    public enum ActionState { IDLE, WORKING}
    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;

    // Start is called before the first frame update
    void Start()
    {
       // MainRoom = new Pathfinding(12, 12, 1, new Vector3(-11, 0, 12));

        agent = this.GetComponent<NavMeshAgent>();


        tree = new BehaviourTree();
        Sequence steal = new Sequence("steal Something");
        Leaf gotodoor = new Leaf("go to door", GoToDoor);        
        Leaf gotoDiamond = new Leaf("Go to Diamond", GoToDiamond);
        Leaf gotoBackDoor = new Leaf("Go to Back", GoToBackDoor);
        Leaf gotovan = new Leaf("Go to Van", GoToVan);
        Selector opendoor = new Selector("Open Door");


        opendoor.AddChild(gotodoor);
        opendoor.AddChild(gotoBackDoor);


        steal.AddChild(opendoor);//now has a choice to make.
        steal.AddChild(gotoDiamond);
        //steal.AddChild(gotodoor);
        steal.AddChild(gotovan);
        //steal.AddChild(gotoBackDoor);
        tree.AddChild(steal);

        tree.printTree();   


    }

    public Node.Status GoToDiamond()
    {
        return GoToLoction(diamond.transform.position);


    }

    public Node.Status GoToDoor()
    {
        return GoToLoction(door.transform.position);


    }
    public Node.Status GoToVan()
    {
        return GoToLoction(van.transform.position);

    }
    public Node.Status GoToBackDoor()
    {
        return GoToLoction(backDoor.transform.position);

    }
    /*
    public Node.Status GoToPoint()
    {
        return GoToLoction(UsedPathCurrentIndex);

    }*/

    Node.Status GoToLoction(Vector3 destination)
    {
        float distanceToTarget = Vector3.Distance(destination, this.transform.position);
        if(state == ActionState.IDLE)
        {
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }
        else if (distanceToTarget < 2)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;

        }
        return Node.Status.RUNNING;


        
    }

    // Update is called once per frame
    void Update()
    {
        /*
            CanSeePlayer = Physics.CheckSphere(transform.position, SightRange, IsPlayer);
            if (CanSeePlayer)
            {
                FindPathToPlayer();
            }
            Sequence ToPlayer = new Sequence("Points To Get To Player");

            for (int i = 0; i < UsedPath.Count; i++)
            {
                UsedPathCurrentIndex = UsedPath[i];

                LeafList.Add(new Leaf("MovePosition", GoToPoint));
            }
            for (int i = 0; i < LeafList.Count; i++)
            {
                ToPlayer.AddChild(LeafList[i]);
            }
        */

        if (treeStatus == Node.Status.RUNNING)
        {//treeStatus = 
            tree.Process();
        }
    }
    /*
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
    }
    */
}
