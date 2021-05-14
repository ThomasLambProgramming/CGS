using UnityEngine;
using UnityEditor;

public class NodeGraphWindow : EditorWindow
{
    float m_nodeDistance = 1.6f;
    int m_nodeConnectionAmount = 16;
    int m_maxNodes = 1000;
    float m_ySpaceLimit = 1;
    bool loaded = false;
    NodeContainer nodedata = null;

    //default mask is layer one only 
    private int m_layerMask = 0 << 1;
   //private string m_tagMask = "";
    
    
    [MenuItem("Window/NodeGraph")] 
    public static void ShowWindow()
    {
        GetWindow(typeof(NodeGraphWindow));
    }

    void OnGUI()
    {
        if (loaded == false)
        {
            LoadFile();
            if (nodedata.NodeGraph != null && NodeManager.m_nodeGraph == null)
                NodeManager.m_nodeGraph = nodedata.NodeGraph;
            loaded = true;
        }
        
        SaveSystem.SaveData(m_nodeDistance, m_nodeConnectionAmount, m_maxNodes, m_ySpaceLimit, m_layerMask, Application.dataPath + "/Editor/Config.json");
        
        GUILayout.Label("Node Settings", EditorStyles.boldLabel);
        m_nodeDistance = EditorGUILayout.FloatField("Node Join Distance", m_nodeDistance);
        m_nodeConnectionAmount = EditorGUILayout.IntField("Max connections", m_nodeConnectionAmount);
        m_maxNodes = EditorGUILayout.IntField("Max nodes",m_maxNodes);
        m_ySpaceLimit = EditorGUILayout.FloatField("Minimum Y Distance", m_ySpaceLimit);
        
        nodedata = (NodeContainer)EditorGUILayout.ObjectField("NodeData", nodedata, typeof(NodeContainer), false);
        if (GUILayout.Button("Bake Nodes"))
        {
            //float time = Time.realtimeSinceStartup;
            NodeManager.ChangeValues(m_nodeDistance, m_nodeConnectionAmount, m_maxNodes,m_ySpaceLimit);
            NodeManager.CreateNodes(m_layerMask);
            nodedata.NodeGraph = NodeManager.m_nodeGraph;
            NodeManager.nodeScriptableObject = nodedata;
            //Debug.Log(Time.realtimeSinceStartup - time);
        }
        if (GUILayout.Button("Show Links"))
        {
            NodeManager.DrawNodes();
        }
    }
    private void LoadFile()
    {
        //load all the settings and if its null we can keep the defaults
        EditorValues loadedSettings = SaveSystem.LoadData(Application.dataPath + "/Editor/Config.json");
        if (loadedSettings == null)
            return;

        m_nodeDistance = loadedSettings.m_distance;
        m_nodeConnectionAmount = loadedSettings.m_nodeConnectionAmount;
        m_maxNodes = loadedSettings.m_maxNodes;
        m_ySpaceLimit = loadedSettings.m_ySpaceLimit;
        m_layerMask = loadedSettings.m_layerMask;
    }
}
