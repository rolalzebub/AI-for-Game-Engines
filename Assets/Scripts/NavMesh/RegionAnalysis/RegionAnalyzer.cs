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
        if(graph != null)
        {
            graph = null;
        }

        var heightSpans = heightfield.GetHeightSpans();

        for(int i = 0; i < heightfield.gridRows - 1; i++)
        {
            if (graph != null)
            {
                graph.allNodes.Add(new List<List<SpanGraph.SpanGraphNode>>());
            }

            for(int j = 0; j < heightfield.gridRows - 1; j++)
            {
                if (graph != null)
                {
                    graph.allNodes[i].Add(new List<SpanGraph.SpanGraphNode>());
                }

                for (int k = 0; k < heightSpans[i,j].Count; k++)
                {
                    var currentSpan = heightSpans[i, j][k];

                    SpanGraph.SpanGraphNode currentNode = new SpanGraph.SpanGraphNode(currentSpan);
                    if(currentSpan.type == HeightFieldVoxelType.Closed)
                    {
                        //check the span above it
                        if(k < heightSpans[i,j].Count - 1)
                        {
                            if(heightSpans[i,j][k+1].type == HeightFieldVoxelType.Open)
                            {
                                //if (heightSpans[i, j][k + 1].ContainsWalkableVoxels())
                                {
                                    if (heightSpans[i, j][k + 1].GetSpanHeight() >= agentHeight)
                                    {
                                        currentNode.isWalkable = true;
                                    }
                                }
                            }
                        }
                    }
                    //mark span as walkable (for now) if the opening is tall enough for the player to step onto it, and it is an open span
                    //exception for if the span is the last vertical open span, then it is assumed there is no roof or there would be a closed span above it
                    //currentNode.isWalkable = (currentSpan.type == HeightFieldVoxelType.Open) && ((currentSpan.GetSpanHeight() >= agentHeight) || k == heightSpans[i, j].Count - 1);

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

        //graph.MakeAllNeighbourConnections();
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
                        var spanCeiling = currentNode.span.GetSpanCeiling();

                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(spanCeiling[0], spanCeiling[1]);
                        Gizmos.DrawLine(spanCeiling[1], spanCeiling[2]);
                        Gizmos.DrawLine(spanCeiling[2], spanCeiling[3]);
                        Gizmos.DrawLine(spanCeiling[3], spanCeiling[0]);
                    }
                }
            }
        }
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
            if (NeighbourNodes == null)
            {
                NeighbourNodes = new List<SpanGraphNode>();
            }

            NeighbourNodes.Add(newNeighbour);
        }

        public bool IsConnectedTo(HeightfieldSpan nb)
        {
            foreach (var node in NeighbourNodes)
            {
                if (node.span == nb)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// returns a vector3 that is the middle of the floorspace covered by this span
        /// </summary>
        /// <returns></returns>
        public Vector3 GetSpanFloor()
        {
            var spanFloor = span.GetSpanFloor();


            Vector3 nodePosition = spanFloor[3] - spanFloor[0];

            return nodePosition;
        }
    }

    public struct SpanGraphNodeQuad
    {
        public Vector3 bottomLeft, topLeft, topRight, bottomRight;
        public Vector3 quadNormal;
    }

}