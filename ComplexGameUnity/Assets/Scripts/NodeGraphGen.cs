using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using System.Runtime.InteropServices;
using System;
[SerializeField]
public class Node
{
    public Vector3 m_position = new Vector3(0,0,0);
    public Edge[] connections = new Edge[NodeManager.m_nodeConnectionAmount];
    public Node(Vector3 a_position)
    {
        m_position = a_position;

    }
}
[SerializeField]
public class Edge
{
    //index of the nodegraph array
    public int to = -1;
    public float cost = 0;
    public Edge(int a_to, float a_cost = 0)
    {
        to = a_to;
        cost = a_cost;
    }
}
public class NodeManager : MonoBehaviour
{
    //Static variables for use by the whole system
    public static float m_nodeDistance = 5;
    public static int m_nodeConnectionAmount = 50000;
    public static int m_maxNodes = 1000;
    public static float m_ySpaceLimit = 1;
    
    public static Node[] m_nodeGraph = null;
        
    public static void ChangeValues(float a_nodeDistance, int a_connectionAmount, int a_maxNodes, float a_yLimit)
    {
        m_nodeDistance = a_nodeDistance;
        m_nodeConnectionAmount = a_connectionAmount;
        m_maxNodes = a_maxNodes;
        m_ySpaceLimit = a_yLimit;
    }

    public static void CreateNodes(int a_layerMask)
    {
        GameObject[] foundObjects = FindObjectsOfType<GameObject>();
        List<NodeCheck> nodes = new List<NodeCheck>();
        
        foreach (GameObject currentObject in foundObjects)
        {
            if (currentObject.CompareTag("Node"))
                continue;

            MeshFilter objectMesh = currentObject.GetComponent<MeshFilter>();
            if (objectMesh == null)
                continue;

            Vector3 newNormal = currentObject.transform.TransformDirection(new Vector3(0, 1, 0));
            List<Vector3> objectVerts = new List<Vector3>();
            foreach (var vert in objectMesh.sharedMesh.vertices)
            {
                bool canAdd = true;
                NodeCheck node = new NodeCheck();
                node.position = currentObject.transform.TransformPoint(vert);
                node.normal = newNormal;
                foreach (var VARIABLE in objectVerts)
                {
                    if (Vector3.Distance(VARIABLE, node.position) < 0.3f)
                    {
                        canAdd = false;
                        break;
                    }
                }
                if (canAdd)
                {
                    nodes.Add(node);
                    objectVerts.Add(node.position);
                }
            }
        }
        Overlap(ref nodes);
        Debug.Log("CREATION PASSED");
    }
    public static void Overlap(ref List<NodeCheck> nodes)
    {
        List<NodeCheck> nodesToDelete = new List<NodeCheck>();
        foreach (var nodeAlpha in nodes)
        {
            foreach (var nodeBeta in nodes)
            {
                if (nodeAlpha == nodeBeta)
                    continue;
                Vector3 alphaToBetaDir = Vector3.Normalize(nodeBeta.position - nodeAlpha.position);
                if (Vector3.Dot(-nodeAlpha.normal, alphaToBetaDir) > 0.9f &&
                    Mathf.Abs(nodeAlpha.position.y - nodeBeta.position.y) <= m_ySpaceLimit)
                {
                    if (nodeAlpha.position.y > nodeBeta.position.y)
                        nodesToDelete.Add(nodeBeta);
                    else
                        nodesToDelete.Add(nodeAlpha);
                }
            }
        }
        foreach (var deletionNode in nodesToDelete)
            nodes.Remove(deletionNode);

        m_nodeGraph = new Node[nodes.Count];
        int index = 0;
        foreach (var VARIABLE in nodes)
        {
            m_nodeGraph[index] = new Node(VARIABLE.position);
            index++;
        }
    }
    public static void LinkNodes(float a_nodeDistance, bool a_firstRun = true)
    {
        if (m_nodeGraph == null)
            return;
   
        for (int a = 0; a < m_nodeGraph.Length; a++)
        {
            for (int b = 0; b < m_nodeGraph.Length; b++)
            {
                if (m_nodeGraph[a] == m_nodeGraph[b])
                    continue;

                bool hasDupe = false;
                foreach (var VARIABLE in m_nodeGraph[b].connections)
                {
                    if (VARIABLE == null)
                        continue;
                    if (m_nodeGraph[VARIABLE.to] == m_nodeGraph[a])
                        hasDupe = true;
                }
                foreach (var VARIABLE in m_nodeGraph[a].connections)
                {
                    if (VARIABLE == null)
                        continue;
                    if (m_nodeGraph[VARIABLE.to] == m_nodeGraph[b])
                        hasDupe = true;
                }
                if (hasDupe)
                    continue;


                float distBetweenNodes = Vector3.Magnitude(m_nodeGraph[a].m_position - m_nodeGraph[b].m_position);
                if (distBetweenNodes < a_nodeDistance * a_nodeDistance)
                {
                    //checks to see what part of the array is null so we dont overwrite or add to something that doesnt have space.
                    for (int i = 0; i < m_nodeConnectionAmount; i++)
                    {
                        if (m_nodeGraph[a].connections[i] == null)
                        {
                            m_nodeGraph[a].connections[i] = new Edge(b);
                            break;
                        }
                    }
                    for (int i = 0; i < m_nodeConnectionAmount; i++)
                    {
                        if (m_nodeGraph[b].connections[i] == null)
                        {
                            m_nodeGraph[b].connections[i] = new Edge(a);
                            break;
                        }
                    }
                }
            }
        }
        Debug.Log("LINK PASSED");
    }
    public static void DrawNodes()
    {
        if (m_nodeGraph == null)
            return;

        foreach (var node in m_nodeGraph)
        {
            for(int i = 0; i < node.connections.Length - 1; i++)
            {
                //if the connection isnt null then draw a line of it this whole function is self explaining
                if (node.connections[i] != null)
                    Debug.DrawLine(node.m_position,m_nodeGraph[node.connections[i].to].m_position);
            }
        }
        Debug.Log("DRAW PASSED");
    }
    //when creating the nodegraph we need the normal to do a underneath check for overlapped/unwanted nodes
    //so we have a seperate class to do the checks then give the position so we arent storing a normal
    //for no reason in the main node class
    public class NodeCheck
    {
        public Vector3 position;
        public Vector3 normal;
    }
}
