using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderHolder : MonoBehaviour
{
    public ComputeShader shader;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {

            //shader.SetFloat("name", value);
            //xyz amounts of threads

            //all the variables of the node
            int sizeInBytes = sizeof(float) * 6 + sizeof(int);
            //since storing a max amount of nodeconnections size * amount then all the other variables
            int totalStride = sizeInBytes * NodeManager.m_nodeConnectionAmount + sizeInBytes;

            ComputeBuffer nodeBuffer = new ComputeBuffer(NodeManager.m_nodeGraph.Length, totalStride);
            nodeBuffer.SetData(NodeManager.m_nodeGraph);
            int kernal = shader.FindKernel("CSMain");
            shader.SetBuffer(kernal, "nodes", nodeBuffer);
            shader.SetInt("NodeConnectionLimit", NodeManager.m_nodeConnectionAmount);
            //shader.Dispatch(kernal, 1, 1, 1);
        }

    }
}
