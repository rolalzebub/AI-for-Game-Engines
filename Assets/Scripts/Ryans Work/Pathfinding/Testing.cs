using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{

    private int Range = 5000;
    public Camera Camera;
    public RaycastHit RayHit;
    public LayerMask WhatIsGrid;

    private Pathfinding BossRoom;
    private Pathfinding MainRoom;
    private Pathfinding SideRoom;
    private Pathfinding SpawnerRoom;
    private Pathfinding SideAlley;

    private void Start()
    {
        BossRoom = new Pathfinding(10, 16, 1, new Vector3(-13, 0, -12));
        MainRoom = new Pathfinding(12, 12, 1, new Vector3(-11, 0, 12));
        SideRoom = new Pathfinding(8, 8, 1, new Vector3(5, 0, 2));
        SideAlley = new Pathfinding(4, 6, 1, new Vector3(3, 0, 22));
        SpawnerRoom = new Pathfinding(8, 8, 1, new Vector3(-11, 0, 34));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(" Left Mouse clicked");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RayHit, Range, WhatIsGrid))
            {
                BossRoom.GetGrid().GetXZ(RayHit.point, out int x, out int z);
                List<PathfindingNode> UsedPath = BossRoom.FindPath(0, 0, x, z);
                if (UsedPath != null)
                {
                    Debug.Log("FoundPath!");
                    for (int i = 0; i < UsedPath.Count - 1; i++)
                    {
                        Debug.DrawLine(new Vector3(UsedPath[i].X, UsedPath[i].Z) * 10f + Vector3.one * 5f, new Vector3(UsedPath[i + 1].X, UsedPath[i + 1].Z) * 10f + Vector3.one * 5f, Color.black);
                        Debug.Log(UsedPath[i].X + " " + UsedPath[i].Z);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(" Left Mouse clicked");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RayHit, Range, WhatIsGrid))
            {
                BossRoom.GetGrid().GetXZ(RayHit.point, out int x, out int z);
                BossRoom.GetNode(x, z).SetIsWalkable(!BossRoom.GetNode(x, z).IsWalkable);
            }
        }
    }
}
