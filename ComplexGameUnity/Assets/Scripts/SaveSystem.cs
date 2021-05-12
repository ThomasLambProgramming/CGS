using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[SerializeField]
public class EditorValues
{
    public float m_distance = 0;
    public int m_nodeConnectionAmount = 0;
    public int m_maxNodes = 0;
    public float m_ySpaceLimit = 0;
    public int   m_layerMask = 0;
}
[SerializeField]
class SaveNode
{
    public Vector3 position = new Vector3(0,0,0);
    public int[] connections = new int[NodeManager.m_nodeConnectionAmount];
    public float[] costs = new float[NodeManager.m_nodeConnectionAmount];
}

public class SaveSystem
{
    public static void SaveData(
        float a_nodeDistance,
        int a_nodeConnectionAmount,
        int a_maxNodes,
        float a_ylimit,
        int a_layerMask, string a_filePath)
    {
        EditorValues toSave = new EditorValues();
        toSave.m_distance = a_nodeDistance;
        toSave.m_nodeConnectionAmount = a_nodeConnectionAmount;
        toSave.m_maxNodes = a_maxNodes;
        toSave.m_ySpaceLimit = a_ylimit;
        toSave.m_layerMask = a_layerMask;

        //this gets the json string and then adds it to the file specified
        StreamWriter stream = new StreamWriter(a_filePath);
        string json = JsonUtility.ToJson(toSave);
        stream.Write(json);
        stream.Close();
    }
    public static void SaveNodes()
    {
        if (NodeManager.m_nodeGraph != null)
        {
            SaveNode[] saveNodes = new SaveNode[NodeManager.m_nodeGraph.Length];
            for (int i = 0; i < NodeManager.m_nodeGraph.Length; i++)
            {
                saveNodes[i] = new SaveNode();
                saveNodes[i].position = NodeManager.m_nodeGraph[i].m_position;
                for (int z = 0; z < NodeManager.m_nodeConnectionAmount; z++)
                {
                    if (NodeManager.m_nodeGraph[i].connections[z] != null)
                    {
                        saveNodes[i].connections[z] = NodeManager.m_nodeGraph[i].connections[z].to;
                        saveNodes[i].costs[z] = NodeManager.m_nodeGraph[i].connections[z].cost;
                    }
                }
            }
            StreamWriter stream = new StreamWriter(Application.dataPath + "/Editor/NodeInfo.json");
            string json = JsonUtility.ToJson(saveNodes);
            stream.Write(json);
            stream.Close();
        }
    }

    
    public static Node[] LoadNodes()
    {
        if (!File.Exists(Application.dataPath + "/Editor/NodeInfo"))
            return null;
        StreamReader stream = new StreamReader(Application.dataPath + "/Editor/NodeInfo.json");
        string jsonData = stream.ReadToEnd();

        Node[] values = JsonUtility.FromJson<Node[]>(jsonData);
        stream.Close();
        return values;
    }



    public static EditorValues LoadData(string a_filePath)
    {
        if (!File.Exists(a_filePath))
            return null;
        StreamReader stream = new StreamReader(a_filePath);
        string jsonData = stream.ReadToEnd();
        
        EditorValues editorValues = JsonUtility.FromJson<EditorValues>(jsonData);
        stream.Close();
        return editorValues;
    }
}
