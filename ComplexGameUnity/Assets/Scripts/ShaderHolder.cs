using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
struct Node
{
    public Vector3 position;
    public int nodeId;
    public float cost;
}
public class ShaderHolder : MonoBehaviour
{
    public static ComputeShader shader;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {

            
        }

    }
    public static int FindClosestNode(Vector3 a_position)
    {
        //position + edge size
        int sizeOfNode = sizeof(float) * 3 + sizeof(int) + sizeof(float);
        Node temp = new Node();
        temp.nodeId = 10;
        temp.cost = 10;

        Node[] test = new Node[1];
        test[0] = temp;
        
        
        ComputeBuffer buffer = new ComputeBuffer(1, sizeOfNode);
        buffer.SetData(test);
        int kernal = shader.FindKernel("FindClosest");
        shader.SetBuffer(kernal, "nodes", buffer);
        shader.Dispatch(kernal, NodeManager.m_nodeGraph.Length, 1, 1);
        buffer.Release();
        buffer.Dispose();
        return 5;
    }
}
