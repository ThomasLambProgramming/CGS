using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ShaderTesting : MonoBehaviour
{
    public ComputeShader shader = null;
    // Start is called before the first frame update

    public struct Edge
    {
        //index into node array of which node
        public int to;
        //cost to move from node to said node
        public float cost;
    }
    public struct Node
    {
        public Vector3 m_position;
        public Edge m_connection1;
        public Edge m_connection2;
        public Edge m_connection3;
        public Edge m_connection4;
        public Edge m_connection5;
        public Edge m_connection6;
    }
    
    
    
    
    
    
    
    
    
    void Update()
    {
        int size = sizeof(float) * 3 + (sizeof(int) + sizeof(float)) * 6;
        if (Input.GetKeyDown(KeyCode.A))
        {
            Node[] nodes = new Node[5];
            
            
            ComputeBuffer buffer = new ComputeBuffer(5, size);
            buffer.SetData(nodes);
            
            shader.SetBuffer(0, "nodes", buffer);
            shader.Dispatch(0,1,1,1);
            buffer.Release();
            
            
        }
    }
}
