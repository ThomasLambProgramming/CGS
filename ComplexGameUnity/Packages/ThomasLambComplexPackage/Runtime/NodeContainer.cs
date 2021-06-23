using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NodeData", menuName = "ScriptableObjects/NodeData", order = 1)]
public class NodeContainer : ScriptableObject
{
    //we just need to save the node data for use later
    public PathNode[] NodeGraph = null;
}