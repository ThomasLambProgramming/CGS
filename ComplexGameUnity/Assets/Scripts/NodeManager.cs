using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using System.Runtime.InteropServices;

//get the object point (hopefully middle or bottom and do a direction dot product check for if the node will be above or below to remove verts

/*
    Optimization ideas
    sort the nodes by position (since the node graph has random access knowing the bounds of the x,z,y we can use binary search for the finding of closest nodes)
    possible griding of the nodegraph into sections with x,z bounds to tell if it needs to search the whole graph or not (can also use binary search as well here for finding grid distances and etc)

*/
[StructLayout(LayoutKind.Sequential)]
public class Node1
{
    //by making a preset i can make these nodes into an array 
    public Vector3 m_position = new Vector3(0,0,0);
    //since i cant do null checks now because of the memory layout of compute shaders and the like i have to give them a default of -1
    public int[] connectionID = {-1,-1,-1,-1,-1,-1};
    public int[] connectionCost = new int[6];
    public Node1(Vector3 a_position)
    {
        m_position = a_position;
    }
}
//For the gpu we need to seperate the class to not contain node itself as a connection as the gpu hates it
public class Edge
{
    //index of the nodegraph array
    public int to = 0;
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
    
    public static Node1[] m_nodeGraph = null;
        
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

        m_nodeGraph = new Node1[nodes.Count];
        int index = 0;
        foreach (var VARIABLE in nodes)
        {
            m_nodeGraph[index] = new Node1(VARIABLE.position);
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
                foreach (var VARIABLE in m_nodeGraph[b].connectionID)
                {
                    if (VARIABLE == -1)
                        continue;
                    if (m_nodeGraph[VARIABLE] == m_nodeGraph[a])
                        hasDupe = true;
                }
                foreach (var VARIABLE in m_nodeGraph[a].connectionID)
                {
                    if (VARIABLE == -1)
                        continue;
                    if (m_nodeGraph[VARIABLE] == m_nodeGraph[b])
                        hasDupe = true;
                }
                if (hasDupe)
                    continue;


                float distBetweenNodes = Vector3.Distance(m_nodeGraph[a].m_position, m_nodeGraph[b].m_position);
                //this is to stop diagonals (i completly forget how this random stuff works)
                if (distBetweenNodes > 1.3 && m_nodeGraph[a].m_position.y - m_nodeGraph[b].m_position.y == 0)
                    continue;

                if (distBetweenNodes < a_nodeDistance)
                {
                    //checks to see what part of the array is null so we dont overwrite or add to something that doesnt have space.
                    for (int i = 0; i < m_nodeConnectionAmount; i++)
                    {
                        if (m_nodeGraph[a].connectionID[i] == -1)
                        {
                            m_nodeGraph[a].connectionID[i] = b;
                            break;
                        }
                    }
                    for (int i = 0; i < m_nodeConnectionAmount; i++)
                    {
                        if (m_nodeGraph[b].connectionID[i] == -1)
                        {
                            m_nodeGraph[b].connectionID[i] = a;
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
            for(int i = 0; i < node.connectionID.Length - 1; i++)
            {
                //if the connection isnt null then draw a line of it this whole function is self explaining
                if (node.connectionID[i] != -1)
                    Debug.DrawLine(node.m_position,m_nodeGraph[node.connectionID[i]].m_position);
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
