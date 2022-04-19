using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    // Costs of movement
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    
    public static Pathfinding Instance { get; private set; }
    
    // Creates the grid and the lists we need
    private Grid<PathfindingNode> grid;
    private List<PathfindingNode> openList;
    private List<PathfindingNode> closedList;

    // Makes the actual grid
    public Pathfinding(int width, int height, int size, Vector3 Displacement)
    {
        Instance = this;
        grid = new Grid<PathfindingNode>(width, height, size, Displacement, (Grid<PathfindingNode> g, int x, int z) => new PathfindingNode(g, x, z));
    }

    public Grid<PathfindingNode> GetGrid()
    {
        return grid;
    }

    public List<Vector3> FindPath(Vector3 StartPostition, Vector3 EndPosition)
    {
        // Gets the frid positions of the world positions
        grid.GetXZ(StartPostition, out int Startx, out int Startz);
        grid.GetXZ(EndPosition, out int EndX, out int EndZ);
        
        // Finds path
        List<PathfindingNode> Path = FindPath(Startx, Startz, EndX, EndZ);

        // checks we found path
        if (Path == null)
        {
            return null;
        }
        else
        {
            // Gets the traveled path
            List<Vector3> VectorPath = new List<Vector3>();
            foreach(PathfindingNode pathfindingNode in Path)
            {
                VectorPath.Add(new Vector3(pathfindingNode.X, pathfindingNode.Z) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f);
            }
            return VectorPath;
        }
    }

    // Gets the found path
    public List<PathfindingNode> FindPath(int StartX, int StartZ, int EndX, int EndZ)
    {
        // Gets a start and end position
        PathfindingNode StartNode = grid.GetValue(StartX, StartZ);
        PathfindingNode EndNode = grid.GetValue(EndX, EndZ);

        // defines lists of nodes ive checked and that need checking
        openList = new List<PathfindingNode> { StartNode};
        closedList = new List<PathfindingNode>();

        // Initialise the grid gets the costs set
        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetHeight(); z++)
            {
                PathfindingNode pathfindingNode = grid.GetValue(x, z);
                pathfindingNode.gCost = int.MaxValue;
                pathfindingNode.CalculateFcost();
                pathfindingNode.CameFromNode = null;
            }
        }
        // costs from the start node
        StartNode.gCost = 0;
        StartNode.hCost = CalculateDistanceCost(StartNode, EndNode);
        StartNode.CalculateFcost();

        // Checks that there are nodes to check
        while (openList.Count > 0)
        {
            // Gets node with lowest F cost
            PathfindingNode CurrentNode = GetLowestFCostNode(openList);
            // Checks that we have reached the goal
            if (CurrentNode == EndNode)
            {
                return CalculatedPath(EndNode);
            }
            // Searched current node
            openList.Remove(CurrentNode);
            closedList.Add(CurrentNode);

            // Cycles through neighbours
            foreach(PathfindingNode NeighbourNode in GetNeighbours(CurrentNode))
            {
                // Have already searched the neighbour so leave it
                if (closedList.Contains(NeighbourNode))
                {
                    continue;
                }
                // If cant walk on neighbour complete its check
                if (!NeighbourNode.IsWalkable)
                {
                    closedList.Add(NeighbourNode);
                    continue;
                }
                // Cost from current node to this node
                int TentativeGCost = CurrentNode.gCost + CalculateDistanceCost(CurrentNode, NeighbourNode);
                // If this is a better path use this path update values to corrospond
                if (TentativeGCost < NeighbourNode.gCost)
                {
                    NeighbourNode.CameFromNode = CurrentNode;
                    NeighbourNode.gCost = TentativeGCost;
                    NeighbourNode.hCost = CalculateDistanceCost(NeighbourNode, EndNode);
                    NeighbourNode.CalculateFcost();
                    
                    // Adds the node to the openlist if its not already on it
                    if (!openList.Contains(NeighbourNode))
                    {
                        openList.Add(NeighbourNode);
                    }
                }
            }
        }

        // No Path
        return null;

    }

    // Calculates the quickest direct path
    private int CalculateDistanceCost(PathfindingNode a, PathfindingNode b)
    {
        int XDistance = Mathf.Abs(a.X - b.X);
        int ZDistance = Mathf.Abs(a.Z - b.Z);
        int Remaining = Mathf.Abs(XDistance - ZDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(XDistance, ZDistance) + MOVE_STRAIGHT_COST * Remaining;
    }

    // Returns a node with the lowest Fcost from a list of nodes
    private PathfindingNode GetLowestFCostNode(List<PathfindingNode> PathNodeList)
    {
        PathfindingNode LowestFCostNode = PathNodeList[0];
        for (int i = 0; i < PathNodeList.Count; i++)
        {
            if (PathNodeList[i].fCost < LowestFCostNode.fCost)
            {
                LowestFCostNode = PathNodeList[i];
            }
        }
        return LowestFCostNode;
    }

    // Gets the oath that was used to get from start to end
    private List<PathfindingNode> CalculatedPath(PathfindingNode EndNode)
    {
        List<PathfindingNode> UsedPath = new List<PathfindingNode>();
        // Adds the end node
        UsedPath.Add(EndNode);
        PathfindingNode CurrentNode = EndNode;
        // Cycles through each nodes stored came from node adding then to the used path list
        while (CurrentNode.CameFromNode != null)
        {
            UsedPath.Add(CurrentNode.CameFromNode);
            CurrentNode = CurrentNode.CameFromNode;
        }

        // Flips the path so its forward to back
        UsedPath.Reverse();
        return UsedPath;
    }

    // Gets all the neighbours of the current node
    private List<PathfindingNode> GetNeighbours(PathfindingNode CurrentNode)
    {
        // List of neighbours
        List<PathfindingNode> Neighbours = new List<PathfindingNode>();

        if (CurrentNode.X - 1 >= 0)
        {
            // W
            Neighbours.Add(GetNode(CurrentNode.X - 1, CurrentNode.Z));
            if (CurrentNode.Z - 1 >= 0)
            {
                // SW
                Neighbours.Add(GetNode(CurrentNode.X - 1, CurrentNode.Z - 1));
            }
            if (CurrentNode.Z + 1 < grid.GetHeight())
            {
                //NW
                Neighbours.Add(GetNode(CurrentNode.X - 1, CurrentNode.Z + 1));
            }
        }
        if (CurrentNode.X + 1 < grid.GetWidth())
        {
            //E
            Neighbours.Add(GetNode(CurrentNode.X + 1, CurrentNode.Z));
            if (CurrentNode.Z - 1 >= 0)
            {
                //SE
                Neighbours.Add(GetNode(CurrentNode.X + 1, CurrentNode.Z - 1));
            }
            if (CurrentNode.Z + 1 < grid.GetHeight())
            {
                // NE
                Neighbours.Add(GetNode(CurrentNode.X + 1, CurrentNode.Z + 1));
            }
        }
        if (CurrentNode.Z - 1 >= 0) 
        {
            // S
            Neighbours.Add(GetNode(CurrentNode.X, CurrentNode.Z - 1));
        }
        if (CurrentNode.Z + 1 < grid.GetHeight())
        {
            // N
            Neighbours.Add(GetNode(CurrentNode.X, CurrentNode.Z + 1));
        }

        return Neighbours;
    }

    // Returns the node at a certain coordiante
    public PathfindingNode GetNode(int X, int Z)
    {
        return grid.GetValue(X, Z);
    }

}
