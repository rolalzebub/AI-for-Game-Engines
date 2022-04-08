using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will take in a heightfield and produce a connection graph of open spans
/// </summary>
public class RegionAnalyzer: MonoBehaviour
{
    SpanGraph graph;

    public float maxTraversableHeight;
    public float maxStepDistance;
    public float agentHeight;
    public float agentRadius;
    public float maxSlopeAngle;
    
    [System.NonSerialized]
    public bool graphCreated = false;

    public void CreateSpanGraphFromHeightfield(Heightfield heightfield)
    {
        var spanMap = heightfield.GetHeightSpans();

        for(int i = 0; i < heightfield.gridRows - 1; i++)
        {
            if (graph != null)
            {
                graph.allNodes.Add(new List<List<SpanGraphNode>>());
            }

            for(int j = 0; j < heightfield.gridRows - 1; j++)
            {
                if (graph != null)
                {
                    graph.allNodes[i].Add(new List<SpanGraphNode>());
                }

                for (int k = 0; k < spanMap[i,j].Count; k++)
                {

                    var currentSpan = spanMap[i, j][k];

                    SpanGraphNode currentNode = new SpanGraphNode(currentSpan);
                    
                    //mark span as walkable (for now) if the opening is tall enough for the player to step onto it, and it is an open span
                    //exception for if the span is the last vertical open span, then it is assumed there is no roof or there would be a closed span above it
                    currentNode.isWalkable = (currentSpan.type == HeightFieldVoxelType.Open) && ((currentSpan.GetSpanHeight() >= agentHeight) || k == spanMap[i, j].Count - 1);

                    //initialize graph
                    if (graph == null)
                    {
                        graph = new SpanGraph(currentNode);
                    }
                    else
                    {
                        graph.allNodes[i][j].Add(currentNode);
                    }
                }
            }
        }
    }

    List<HeightfieldSpan> FindSpanNeighbours(List<HeightfieldSpan>[,] field, int X_spanIndex, int Z_SpanIndex, int Y_SpanIndex)
    {
        List<HeightfieldSpan> neighboursFound = new List<HeightfieldSpan>();

        HeightfieldSpan spanToCheck = field[X_spanIndex, Z_SpanIndex][Y_SpanIndex];

        //check the straight left
        if(X_spanIndex != 0)
        {
            var leftSpans = field[X_spanIndex - 1, Z_SpanIndex];

            //check all spans in the neighbouring column
            foreach(var item in leftSpans)
            {
                if(item.type == spanToCheck.type)
                {
                    //if((item.SpanBounds.Max - spanToCheck.SpanBounds.Min).magnitude < maxStepDistance)
                    {
                        neighboursFound.Add(item);
                    }
                }
            }
        }

        //check straight right
        if(X_spanIndex < field.GetLength(0) - 1)
        {
            var rightSpans = field[X_spanIndex + 1, Z_SpanIndex];
            
            foreach (var item in rightSpans)
            {
                if (item.type == spanToCheck.type)
                {
                    //if((item.SpanBounds.Max - spanToCheck.SpanBounds.Min).magnitude < maxStepDistance)
                    {
                        neighboursFound.Add(item);
                    }
                }
            }
        }

        //check back
        if(Z_SpanIndex != 0)
        {
            var backSpans = field[X_spanIndex, Z_SpanIndex - 1];
            
            foreach (var item in backSpans)
            {
                if (item.type == spanToCheck.type)
                {
                    //if((item.SpanBounds.Max - spanToCheck.SpanBounds.Min).magnitude < maxStepDistance)
                    {
                        neighboursFound.Add(item);
                    }
                }
            }
        }

        //check front
        if(Z_SpanIndex < field.GetLength(0) - 1)
        {
            var frontSpans = field[X_spanIndex, Z_SpanIndex + 1];

            foreach (var item in frontSpans)
            {
                if (item.type == spanToCheck.type)
                {
                    //if((item.SpanBounds.Max - spanToCheck.SpanBounds.Min).magnitude < maxStepDistance)
                    {
                        neighboursFound.Add(item);
                    }
                }
            }
        }

        return neighboursFound;
    }

    private void OnDrawGizmosSelected()
    {
        if (!graphCreated)
            return;

        for (int i = 0; i < graph.allNodes.Count; i++)
        {
            for (int j = 0; j < graph.allNodes[i].Count; j++)
            {
                for (int k = 0; k < graph.allNodes[i][j].Count; k++)
                {
                    var currentNode = graph.allNodes[i][j][k];

                    if (currentNode.isWalkable)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }                    


                    int cubeVertSize = currentNode.span.spanVoxels.Count;

                    Vector3 min = currentNode.span.spanVoxels[0].VoxelBounds.Min, max = Vector3.zero;

                    foreach (var voxel in currentNode.span.spanVoxels)
                    {
                        min = Vector3.Min(min, voxel.VoxelBounds.Min);
                        max = Vector3.Max(max, voxel.VoxelBounds.Max);
                    }

                    AABB bounds = new AABB(min, max);

                    Vector3 nodePosition = bounds.Center;
                    nodePosition.y = bounds.Min.y;

                    Gizmos.DrawWireSphere(nodePosition, 0.1f);
                }
            }
        }
    }

    private void DrawAllNeighbourGizmos(SpanGraphNode neighbour)
    {
        while(neighbour.NeighbourNodes != null)
        {
            for (int i = neighbour.NeighbourNodes.Count - 1; i >= 0; i--)
            {
                var item = neighbour.NeighbourNodes[i];
                DrawLineToPreviousNode(item, neighbour);
                DrawAllNeighbourGizmos(item);
            }
        }

        DrawSelfGizmo(neighbour);
    }

    private void DrawLineToPreviousNode(SpanGraphNode item, SpanGraphNode previous)
    {
        Gizmos.DrawLine(previous.span.SpanBounds.Center, item.span.SpanBounds.Center);
    }

    private void DrawSelfGizmo(SpanGraphNode nodeToDraw)
    {
        Gizmos.DrawWireSphere(nodeToDraw.span.SpanBounds.Center, 0.2f);
    }
}

public class SpanGraphNode
{
    public bool isWalkable;

    public HeightfieldSpan span;

    public List<SpanGraphNode> NeighbourNodes;

    public SpanGraphNode(HeightfieldSpan _span)
    {
        span = _span;
        NeighbourNodes = new List<SpanGraphNode>();
    }

    public void AddNeighbour(SpanGraphNode newNeighbour)
    {
        if(NeighbourNodes == null)
        {
            NeighbourNodes = new List<SpanGraphNode>();
        }

        NeighbourNodes.Add(newNeighbour);
    }

    public bool IsConnectedTo(HeightfieldSpan nb)
    {
        foreach(var node in NeighbourNodes)
        {
            if(node.span == nb)
            {
                return true;
            }
        }

        return false;
    }
}

public class SpanGraph
{
    public SpanGraphNode rootNode;

    //xz-y ordered list of spans
    public List<List<List<SpanGraphNode>>> allNodes;

    public SpanGraph(HeightfieldSpan rootSpan)
    {
        rootNode = new SpanGraphNode(rootSpan);

        allNodes = new List<List<List<SpanGraphNode>>>();
        allNodes.Add(new List<List<SpanGraphNode>>());
        allNodes[0].Add(new List<SpanGraphNode>());
        allNodes[0][0].Add(rootNode);
    }

    public SpanGraph(SpanGraphNode _rootNode)
    {
        rootNode = _rootNode;

        allNodes = new List<List<List<SpanGraphNode>>>();
        allNodes.Add(new List<List<SpanGraphNode>>());
        allNodes[0].Add(new List<SpanGraphNode>());
        allNodes[0][0].Add(rootNode);
    }

    public void MakeAllNeighbourConnections()
    {
        for(int i = 0; i < allNodes.Count; i++)
        {
            for(int j = 0; j < allNodes[i].Count; j++)
            {
                for(int k = 0; k < allNodes[i][j].Count; k++)
                {
                    var currentNode = allNodes[i][j][k];

                    if(!currentNode.isWalkable)
                    {
                        continue;
                    }

                    ConnectToNeighbours(currentNode, i, j, k);
                }
            }
        }
    }

    void ConnectToNeighbours(SpanGraphNode node, int xIndex, int zIndex, int yIndex)
    {
        //check the left
        if (xIndex != 0)
        {
            var leftNode = allNodes[xIndex - 1][zIndex] [yIndex];

            if(leftNode.isWalkable)
            {
                if(!node.NeighbourNodes.Contains(leftNode))
                {
                    node.NeighbourNodes.Add(leftNode);
                }
                if (!leftNode.NeighbourNodes.Contains(node))
                {
                    leftNode.NeighbourNodes.Add(node);
                    allNodes[xIndex - 1][zIndex][yIndex] = leftNode;
                }
            }
        }

        //check the right
        if(xIndex < allNodes.Count - 1)
        {
            var rightNode = allNodes[xIndex + 1][zIndex][yIndex];

            if (rightNode.isWalkable)
            {
                if (!node.NeighbourNodes.Contains(rightNode))
                {
                    node.NeighbourNodes.Add(rightNode);
                }
                if (!rightNode.NeighbourNodes.Contains(node))
                {
                    rightNode.NeighbourNodes.Add(node);
                    allNodes[xIndex + 1][zIndex][yIndex] = rightNode;
                }
            }
        }

        //check the front
        if(zIndex < allNodes[xIndex].Count - 1)
        {
            var frontNode = allNodes[xIndex][zIndex + 1][yIndex];

            if (frontNode.isWalkable)
            {
                if (!node.NeighbourNodes.Contains(frontNode))
                {
                    node.NeighbourNodes.Add(frontNode);
                }
                if (!frontNode.NeighbourNodes.Contains(node))
                {
                    frontNode.NeighbourNodes.Add(node);
                    allNodes[xIndex][zIndex + 1][yIndex] = frontNode;
                }
            }

        }

        //check the back

        if(zIndex != 0)
        {
            var backNode = allNodes[xIndex][zIndex - 1][yIndex];

            if (backNode.isWalkable)
            {
                if (!node.NeighbourNodes.Contains(backNode))
                {
                    node.NeighbourNodes.Add(backNode);
                }
                if (!backNode.NeighbourNodes.Contains(node))
                {
                    backNode.NeighbourNodes.Add(node);
                    allNodes[xIndex][zIndex - 1][yIndex] = backNode;
                }
            }
        }
    }

}