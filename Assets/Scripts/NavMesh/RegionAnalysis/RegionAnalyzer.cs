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
    
    [System.NonSerialized]
    public bool graphCreated = false;

    public void CreateSpanGraphFromHeightfield(Heightfield heightfield)
    {
        if(graph != null)
        {
            graph = null;
        }

        AgentSettings settings = new AgentSettings();
        settings.maxTraversableHeight = maxTraversableHeight;
        settings.maxStepDistance = maxStepDistance;
        settings.agentHeight = agentHeight;
        settings.agentRadius = agentRadius;
        
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
                                if (heightSpans[i, j][k + 1].GetSpanHeight() >= agentHeight)
                                {
                                    currentNode.isWalkable = true;
                                }
                            }
                        }
                    }

                    //initialize graph
                    if (graph == null)
                    {
                        graph = new SpanGraph(currentNode, settings);
                    }
                    else
                    {
                        graph.allNodes[i][j].Add(currentNode);
                    }
                }
            }
        }

        graph.MakeAllNeighbourConnections();
    }

    public void ClearGraph()
    {
        graphCreated = false;
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
                        Gizmos.DrawLine(spanCeiling.bottomLeft, spanCeiling.topLeft);
                        Gizmos.DrawLine(spanCeiling.topLeft, spanCeiling.topRight);
                        Gizmos.DrawLine(spanCeiling.topRight, spanCeiling.bottomRight);
                        Gizmos.DrawLine(spanCeiling.bottomRight, spanCeiling.bottomLeft);

                        var currentSpanCentre = currentNode.span.GetSpanCeiling().topRight - currentNode.span.GetSpanCeiling().bottomLeft;
                        currentSpanCentre = currentNode.span.GetSpanCeiling().topRight - (currentSpanCentre/2);
                        foreach (var nbr in currentNode.NeighbourNodes)
                        {
                            Gizmos.color = Color.grey;
                            var nbrCentre = nbr.span.GetSpanCeiling().topRight - nbr.span.GetSpanCeiling().bottomLeft;
                            nbrCentre = nbr.span.GetSpanCeiling().topRight - (nbrCentre/2);
                            Gizmos.DrawSphere(currentSpanCentre, 0.2f);
                            Gizmos.DrawLine(currentSpanCentre, nbrCentre);
                        }
                    }
                }
            }
        }
    }
}