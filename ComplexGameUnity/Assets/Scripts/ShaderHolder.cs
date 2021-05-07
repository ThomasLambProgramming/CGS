using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        int sizeOfNode = sizeof(float) * 3 + (sizeof(int) + sizeof(float)) * 6;
        ComputeBuffer buffer = new ComputeBuffer(NodeManager.m_nodeGraph.Length, sizeOfNode);
        buffer.SetData(NodeManager.m_nodeGraph);
        int kernal = shader.FindKernel("FindClosest");
        shader.SetBuffer(kernal, "nodes", buffer);
        shader.SetFloats("findPosition", a_position[0]);

        ComputeBuffer index = new ComputeBuffer(1, sizeof(int));
        index.SetData(new int[1]);

        shader.Dispatch(kernal, NodeManager.m_nodeGraph.Length, 1, 1);
        int[] closestIndex = new int[1];
        index.GetData(closestIndex);
        index.Release();
        buffer.Release();
        return closestIndex[0];
    }
}
