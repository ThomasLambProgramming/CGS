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

    private ComputeShader m_NodeCreationShader;
    
    //I want to be able to have the user give the layers that
    //they want to select "walkable objects"
    //as a whole instead of individually selecting the objects
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
        m_nodeDistance = EditorGUILayout.IntField("Node Join Distance", m_nodeDistance);
        m_nodeConnectionAmount = EditorGUILayout.IntField("Max connections", m_nodeConnectionAmount);
        m_maxNodes = EditorGUILayout.IntField("Max nodes", m_maxNodes);
        
        m_layerMask = EditorGUILayout.MaskField("Mask layers", m_layerMask, m_maskOptions);
        
        if (GUILayout.Button("Bake Nodes"))
        {
            
        }
        
        /*
            Foreach object that is selected / masked
            if meshrender get == true
            foreach vertex get its position 
                (do a check to only get the top parts)
                get the extents range, check the height of the object 
                check for any nodes in the range of that extents to delete (so there isnt overlap from other objects)
            create a node for each vertex and the middle point (do a check for the vertex to middle to add more nodes
            if the distance is greater (for stuff like very large cubes 4 verts and the middle point wont be enough
            
            this is not going to be efficent, the connections will be done with the gpu but the checking and etc
            has to be done by the cpu with all the new memory creation and checks its not worth it to be sending it back
            and forth to the gpu because of the delay.
            
            once all nodes are setup, then create a thread for each node on the gpu and then
            foreach node
                check against all other nodes in list
                if (distance < m_nodedistance && m_nodeConnectionAmount > currently connectednodes)
                {
                add to node connections
                }
        */
        
        
        
    }
}
