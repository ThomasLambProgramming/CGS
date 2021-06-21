using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/*
* File: SaveSystem.cs
*
* Author: Thomas Lamb (s200498@students.aie.edu.au)
* Date Created: 10th May 2021
* Date Last Modified: Saturday 12 June 2021
*
* Save system for the nodegraph editor window
* 
*/

[SerializeField]
public class EditorValues
{
    public float m_distance = 0;
    public int m_nodeConnectionAmount = 0;
    public float m_ySpaceLimit = 0;
}
//class to save the values needed
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
        //if the file doesnt exist dont do anything
        if (!File.Exists(a_filePath))
            return null;
        //else open a streamreader and read everything needed
        StreamReader stream = new StreamReader(a_filePath);
        string jsonData = stream.ReadToEnd();
        
        EditorValues editorValues = JsonUtility.FromJson<EditorValues>(jsonData);
        stream.Close();
        return editorValues;
    }
}
