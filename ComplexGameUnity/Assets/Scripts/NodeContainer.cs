using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NodeData", menuName = "ScriptableObjects/NodeData", order = 1)]
public class NodeContainer : ScriptableObject
{
    public Node[] NodeGraph = null;
    void Start()
    {
        if (NodeGraph != null && NodeManager.m_nodeGraph == null)
        {
            NodeManager.m_nodeGraph = NodeGraph;
        }
    }
}


