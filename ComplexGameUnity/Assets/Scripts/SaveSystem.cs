using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[SerializeField]
public class EditorValues
{
    public float m_distance = 0;
    public int m_nodeConnectionAmount = 0;
    public float m_ySpaceLimit = 0;
}
public class SaveSystem
{
    public static void SaveData(
        float a_nodeDistance,
        int a_nodeConnectionAmount,
        float a_ylimit,
        string a_filePath)
    {
        EditorValues toSave = new EditorValues();
        toSave.m_distance = a_nodeDistance;
        toSave.m_nodeConnectionAmount = a_nodeConnectionAmount;
        toSave.m_ySpaceLimit = a_ylimit;

        //this gets the json string and then adds it to the file specified
        StreamWriter stream = new StreamWriter(a_filePath);
        string json = JsonUtility.ToJson(toSave, true);
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
