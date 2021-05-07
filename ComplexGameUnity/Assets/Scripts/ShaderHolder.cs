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

            ComputeBuffer nodeBuffer = new ComputeBuffer(1, sizeof(int));

            int[] test = new int[1];
            test[0] = 0; 
            nodeBuffer.SetData(test);
            int kernal = shader.FindKernel("Linker");
            shader.SetBuffer(kernal, "nodes", nodeBuffer);
            shader.SetInt("NodeConnectionLimit", NodeManager.m_nodeConnectionAmount);

            //shader.Dispatch(kernal, NodeManager.m_NodeGraph.Length, 1, 1);
            shader.Dispatch(kernal, 1, 1, 1);

            int[] hello = new int[1];
            nodeBuffer.GetData(hello);
            Debug.Log(hello[0]);
            nodeBuffer.Release();
        }

    }
}
