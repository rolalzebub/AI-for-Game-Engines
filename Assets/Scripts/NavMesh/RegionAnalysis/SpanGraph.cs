using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpanGraph
{
    public SpanGraphNode rootNode;
    AgentSettings settings;
    

    //xz-y ordered list of spans
    public List<List<List<SpanGraphNode>>> allNodes;

    public SpanGraph(HeightfieldSpan rootSpan, AgentSettings _settings)
    {
        rootNode = new SpanGraphNode(rootSpan);

        allNodes = new List<List<List<SpanGraphNode>>>();
        allNodes.Add(new List<List<SpanGraphNode>>());
        allNodes[0].Add(new List<SpanGraphNode>());
        allNodes[0][0].Add(rootNode);

        settings = _settings;
    }

    public SpanGraph(SpanGraphNode _rootNode, AgentSettings _settings)
    {
        rootNode = _rootNode;

        allNodes = new List<List<List<SpanGraphNode>>>();
        allNodes.Add(new List<List<SpanGraphNode>>());
        allNodes[0].Add(new List<SpanGraphNode>());
        allNodes[0][0].Add(rootNode);

        settings = _settings;
    }

    public void MakeAllNeighbourConnections()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            for (int j = 0; j < allNodes[i].Count; j++)
            {
                for (int k = 0; k < allNodes[i][j].Count; k++)
                {
                    var currentNode = allNodes[i][j][k];

                    if (!currentNode.isWalkable)
                    {
                        continue;
                    }

                    ConnectToNeighbours(ref currentNode, i, j, k);

                    allNodes[i][j][k] = currentNode;
                }
            }
        }
    }

    void ConnectToNeighbours(ref SpanGraphNode node, int xIndex, int zIndex, int yIndex)
    {
        for(int xOffset = -1; xOffset < 2; xOffset++)
        {
            for (int zOffset = -1; zOffset < 2; zOffset++)
            {
                for (int yOffset = -1; yOffset < 2; yOffset++)
                {

                    if(xOffset == 0 && zOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }

                    if (xIndex + xOffset >= 0 && xIndex + xOffset < allNodes.Count)
                    {
                        if (zIndex + zOffset >= 0 && zIndex + zOffset < allNodes[xIndex + xOffset].Count)
                        {
                            if (yIndex + yOffset >= 0 && yIndex + yOffset < allNodes[xIndex + xOffset][zIndex + zOffset].Count)
                            {
                                var otherNode = allNodes[xIndex + xOffset][zIndex + zOffset][yIndex + yOffset];

                                if (otherNode.isWalkable)
                                {
                                    if (!node.NeighbourNodes.Contains(otherNode))
                                    {
                                        node.NeighbourNodes.Add(otherNode);
                                    }
                                    if (!otherNode.NeighbourNodes.Contains(node))
                                    {
                                        otherNode.NeighbourNodes.Add(node);

                                        allNodes[xIndex + xOffset][zIndex + zOffset][yIndex + yOffset] = otherNode;
                                    }
                                }
                            }
                        }
                    }
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

        //public bool IsConnectedTo(HeightfieldSpan nb)
        //{
        //    foreach (var node in NeighbourNodes)
        //    {
        //        if (node.span == nb)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        /// <summary>
        /// returns a vector3 that is the middle of the floorspace covered by this span
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNodeFloor()
        {
            var spanFloor = span.GetSpanFloor();

            Vector3 nodePosition = spanFloor.topRight - spanFloor.bottomLeft;

            return nodePosition;
        }
    }

    public struct SpanGraphNodeQuad
    {
        public Vector3 bottomLeft, topLeft, topRight, bottomRight;
        public Vector3 quadNormal;
    }

}

public struct AgentSettings
{
    public float maxTraversableHeight;
    public float maxStepDistance;
    public float agentHeight;
    public float agentRadius;
}