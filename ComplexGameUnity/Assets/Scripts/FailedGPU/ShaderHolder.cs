//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Runtime.InteropServices;



//struct NodeEdge
//{
//    public int connection1;
//    public int connection2;
//    public int connection3;
//    public int connection4;
//    public int connection5;
//    public int connection6;
//    public float connection1Cost;
//    public float connection2Cost;
//    public float connection3Cost;
//    public float connection4Cost;
//    public float connection5Cost;
//    public float connection6Cost;
//}

//public class ShaderHolder : MonoBehaviour
//{
//    public ComputeShader shader;
//    //these are public statics for now so all information doesnt need to be readded
//    //or reprocessed
//    public ComputeBuffer nodePositions;
//    public ComputeBuffer nodeConnections;
//    public int nodeAmount;

    



    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.G))
    //    {
    //        FillData();
    //    }
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        nodePositions.Release();
    //        nodeConnections.Release();
    //    }
    //}
    //private void FillData()
    //{
    //    if (NodeManager.m_nodeGraph == null)
    //        return;

    //    nodeAmount = NodeManager.m_nodeGraph.Length;
    //    nodePositions = new ComputeBuffer(nodeAmount, sizeof(float) * 3);
    //    nodeConnections = new ComputeBuffer(nodeAmount * 6, sizeof(int) * 6 + sizeof(float) * 6);

    //    Vector3[] positions = new Vector3[nodeAmount];
    //    NodeEdge[] connections = new NodeEdge[nodeAmount];

    //    //this is all because hlsl doesnt like arrays
    //    for (int i = 0; i < nodeAmount; i++)
    //    {
    //        positions[i] = NodeManager.m_nodeGraph[i].m_position;

    //        for (int z = 0; z < 6; z++)
    //        {
    //            if (NodeManager.m_nodeGraph[i].connections[z] != null)
    //            {
    //                connections[i] = new NodeEdge();
    //                if (z == 0)
    //                {
    //                    connections[i].connection1 = NodeManager.m_nodeGraph[i].connections[z].to;
    //                    connections[i].connection1Cost = 0;
    //                }
    //                if (z == 1)
    //                {
    //                    connections[i].connection2 = NodeManager.m_nodeGraph[i].connections[z].to;
    //                    connections[i].connection2Cost = 0;
    //                }
    //                if (z == 2)
    //                {
    //                    connections[i].connection3 = NodeManager.m_nodeGraph[i].connections[z].to;
    //                    connections[i].connection3Cost = 0;
    //                }
    //                if (z == 3)
    //                {
    //                    connections[i].connection4 = NodeManager.m_nodeGraph[i].connections[z].to;
    //                    connections[i].connection4Cost = 0;
    //                }
    //                if (z == 4)
    //                {
    //                    connections[i].connection5 = NodeManager.m_nodeGraph[i].connections[z].to;
    //                    connections[i].connection5Cost = 0;
    //                }
    //                if (z == 5)
    //                {
    //                    connections[i].connection6 = NodeManager.m_nodeGraph[i].connections[z].to;
    //                    connections[i].connection6Cost = 0;
    //                }
    //            }
    //        }
    //    }
        
    //    //set data
    //    nodePositions.SetData(positions);
    //    nodeConnections.SetData(connections);

    //    //set kernal
    //    int kernal = shader.FindKernel("CSMain");
        
    //    //set ints
    //    shader.SetInt("nodeAmount", nodeAmount);
    //    shader.SetInt("startNode", AStar.FindClosestNode(Agent.start));
    //    shader.SetInt("endNode", AStar.FindClosestNode(Agent.end));

    //    //set buffers and launch
    //    shader.SetBuffer(kernal, "nodePositions", nodePositions);
    //    shader.SetBuffer(kernal, "nodeConnections", nodeConnections);
    //    shader.Dispatch(kernal, 1, 0, 0);
        
    //    //release buffers
    //    nodePositions.Release();
//    //    nodeConnections.Release();
//    //}
    
    
//}
