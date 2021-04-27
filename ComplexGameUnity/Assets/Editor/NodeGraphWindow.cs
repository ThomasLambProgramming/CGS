using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeGraphWindow : EditorWindow
{
    //default mask is layer one only 
    private int m_layerMask = 0 << 1;
   //private string m_tagMask = "";
    private string[] m_maskOptions = new string[] 
    {   "1","2","3","4","5", "6","7","8","9",
        "10","11","12","13","14","15","16","17","18",
        "19","20","21","22","23","24","25","26","27","28",
        "29","30","31","32"
    };

    [MenuItem("Window/NodeGraph")] 
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NodeGraphWindow));
    }
    void OnGUI()
    {
        GUILayout.Label("Node Settings", EditorStyles.boldLabel);
        NodeManager.m_nodeDistance = EditorGUILayout.IntField("Node Join Distance", NodeManager.m_nodeDistance);
        NodeManager.m_nodeConnectionAmount = EditorGUILayout.IntField("Max connections", NodeManager.m_nodeConnectionAmount);
        NodeManager.m_maxNodes = EditorGUILayout.IntField("Max nodes",NodeManager.m_maxNodes);
        
        m_layerMask = EditorGUILayout.MaskField("Mask layers", m_layerMask, m_maskOptions);
        
        if (GUILayout.Button("Bake Nodes"))
        {
            NodeManager.CreateNodes(m_layerMask);
        }

        if (GUILayout.Button("Link Nodes"))
        {
            NodeManager.LinkNodes();
        }
        
        
        
        
        
    }
}
