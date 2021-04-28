using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeGraphWindow : EditorWindow
{
    float m_nodeDistance = 1.6f;
    int m_nodeConnectionAmount = 16;
    int m_maxNodes = 1000;
    float m_ySpaceLimit = 1;
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
        m_nodeDistance = EditorGUILayout.FloatField("Node Join Distance", m_nodeDistance);
        m_nodeConnectionAmount = EditorGUILayout.IntField("Max connections", m_nodeConnectionAmount);
        m_maxNodes = EditorGUILayout.IntField("Max nodes",m_maxNodes);
        m_ySpaceLimit = EditorGUILayout.FloatField("Max Y Distance", m_ySpaceLimit);
        m_layerMask = EditorGUILayout.MaskField("Mask layers", m_layerMask, m_maskOptions);
        
        if (GUILayout.Button("Bake Nodes"))
        {
            NodeManager.ChangeValues(m_nodeDistance, m_nodeConnectionAmount, m_maxNodes,m_ySpaceLimit);
            NodeManager.CreateNodes(m_layerMask);
        }

        if (GUILayout.Button("Link Nodes"))
        {
            NodeManager.LinkNodes(m_nodeDistance);
        }

        if (GUILayout.Button("Show Links"))
        {
            NodeManager.DrawNodes();
        }

        if (GUILayout.Button("RESET"))
        {
            NodeManager.ResetValues();
        }


        
        
    }
}
