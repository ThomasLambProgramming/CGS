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
