using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All the infroamtion each node in the grid should store
public class PathfindingNode
{
    // Coordinates and grid
    private Grid<PathfindingNode> grid;
    public int X;
    public int Z;

    // Calculation values
    public int gCost;
    public int hCost;
    public int fCost;

    // Stores previous node and allows you to see if something is walkable
    public bool IsWalkable;
    public PathfindingNode CameFromNode;

    // Sets all the default values
    public PathfindingNode(Grid<PathfindingNode> grid, int X, int Z)
    {
        this.grid = grid;
        this.X = X;
        this.Z = Z;
        IsWalkable = true;
    }
    // gets the coordinate of the box
    public override string ToString()
    {
        return X + "," + Z;
    }

    // gets a Fcost
    public void CalculateFcost()
    {
        fCost = gCost + hCost;
    }

    // Toggles if a coordiante can be used in the pathfinding
    public void SetIsWalkable(bool IsWalkable)
    {
        this.IsWalkable = IsWalkable;
        grid.TriggerGridChange(X, Z);
    }

}

