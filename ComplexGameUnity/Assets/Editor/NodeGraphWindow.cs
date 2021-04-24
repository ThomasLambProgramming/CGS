using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeGraphWindow : EditorWindow
{
    private int m_nodeDistance = 5;
    private int m_nodeConnectionAmount = 4;
    private int m_maxNodes = 1000;

    private ComputeShader m_compute;
    
    //I want to be able to have the user give the layers or tags that
    //they want to select "walkable objects"
    //as a whole instead of individually selecting the objects
    private bool m_masksEnabled = false;
    //default mask is layer one only 
    private int m_layerMask = 0 << 1;
   //private string m_tagMask = "";
    private string[] m_maskOptions = new string[] {"1","2","3","4","5", "6","7","8","9","10","11","12"};

    [MenuItem("Window/NodeGraph")] 
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NodeGraphWindow));
    }
    void OnGUI()
    {
        GUILayout.Label("Node Settings", EditorStyles.boldLabel);
        m_nodeDistance = EditorGUILayout.IntField("Node Join Distance", m_nodeDistance);
        m_nodeConnectionAmount = EditorGUILayout.IntField("Max connections", m_nodeConnectionAmount);
        m_maxNodes = EditorGUILayout.IntField("Max nodes", m_maxNodes);

        m_masksEnabled = EditorGUILayout.BeginToggleGroup("Enable object Masking", m_masksEnabled);
        m_layerMask = EditorGUILayout.MaskField("Mask layers", m_layerMask, m_maskOptions);
        EditorGUILayout.EndToggleGroup();

        if (GUILayout.Button("Bake Nodes"))
        {
            if (m_masksEnabled)
            {
                
            }
            else
            {
                
            }
        }
    }
}
