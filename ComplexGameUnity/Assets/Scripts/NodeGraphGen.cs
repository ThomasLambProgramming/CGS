using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using System.Runtime.InteropServices;
using System;
using System.IO;
[Serializable]
public class Node
{
    public Vector3 m_position = new Vector3(0, 0, 0);
    public Edge[] connections = new Edge[NodeManager.m_nodeConnectionAmount];
    public Node(Vector3 a_position)
    {
        m_position = a_position;
    }
}
[Serializable]
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
    public static NodeContainer nodeScriptableObject = null;

    //Len is awesome below [] method thingy he showed its amazing
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void OnStart()
    {
        NodeContainer[] temparray = Resources.FindObjectsOfTypeAll<NodeContainer>();
        if (temparray != null)
            nodeScriptableObject = temparray[0];

        if (nodeScriptableObject != null)
            if (nodeScriptableObject.NodeGraph != null)
                m_nodeGraph = nodeScriptableObject.NodeGraph;
    }

    //Static variables for use by the whole system
    public static float m_nodeDistance = 5;
    public static int m_nodeConnectionAmount = 50000;

    public static float m_ySpaceLimit = 1;

    public static List<GameObject> ObjectsToProcess = new List<GameObject>();
    static List<Vector3> m_unwalkablePoints = new List<Vector3>();
    public static Node[] m_nodeGraph = null;

    public static void ChangeValues(float a_nodeDistance, int a_connectionAmount, float a_yLimit)
    {
        m_nodeDistance = a_nodeDistance;
        m_nodeConnectionAmount = a_connectionAmount;
    
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

            if (currentObject.layer == 9)
            {
                bool canAdd = true;
                List<Vector3> objectVerts = new List<Vector3>();

                foreach (var vert in objectMesh.sharedMesh.vertices)
                {
                    Vector3 vertWorldPos = currentObject.transform.TransformPoint(vert);
                    foreach (var VARIABLE in objectVerts)
                    {
                        if (Vector3.Distance(VARIABLE, vertWorldPos) < 0.3f)
                        {
                            canAdd = false;
                            break;
                        }
                    }
                    if (canAdd)
                    {
                        m_unwalkablePoints.Add(vertWorldPos);
                        objectVerts.Add(vertWorldPos);
                    }
                }
                //makes sure it isnt added to the list of actual nodes
                continue;
            }
            else if (currentObject.layer == 10)
            {
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
        }
        
        //removes overlaps and unneeded
        Overlap(ref nodes);
        //removes nodes that are too close to unwalkable objects
        UnWalkable(ref nodes);
        //links all nodes together
        LinkNodes(m_nodeDistance);

        List<Node> FinalArrayNode = new List<Node>();
        foreach (var nod in m_nodeGraph)
        {
            for (int i = 0; i < nod.connections.Length; i++)
            {
                if (nod.connections[i] != null)
                {
                    FinalArrayNode.Add(nod);
                    break;
                }
            }
        }
        m_nodeGraph = new Node[FinalArrayNode.Count];
        for(int i = 0; i < FinalArrayNode.Count; i++)
        {
            m_nodeGraph[i] = FinalArrayNode[i];
        }
        
        NodeContainer[] temparray = Resources.FindObjectsOfTypeAll<NodeContainer>();
        if (temparray != null)
            nodeScriptableObject = temparray[0];
        else
            return;


        if (nodeScriptableObject.NodeGraph != null)
        {
            nodeScriptableObject.NodeGraph = m_nodeGraph;
            EditorUtility.SetDirty(nodeScriptableObject);
            AssetDatabase.SaveAssets();
        }
    }

    private static void UnWalkable(ref List<NodeCheck> nodes)
    {
        List<NodeCheck> nodesToDelete = new List<NodeCheck>();
        foreach (var nodeAlpha in nodes)
        {
            foreach (var unwalkPoint in m_unwalkablePoints)
            {
                Vector2 nodepoint = new Vector2(nodeAlpha.position.x, nodeAlpha.position.z);
                Vector2 unwalk = new Vector2(unwalkPoint.x, unwalkPoint.z);

                if (Vector2.Distance(nodepoint, unwalk) < 1f)
                    if (Mathf.Abs(nodeAlpha.position.y - unwalkPoint.y) < m_ySpaceLimit)
                        nodesToDelete.Add(nodeAlpha);
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

    private static void Overlap(ref List<NodeCheck> nodes)
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
    }
    private static void LinkNodes(float a_nodeDistance)
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
                    //checks between non walkable for connection

                    bool isPassingThrough = false;
                    foreach (var position in m_unwalkablePoints)
                    {
                        Vector3 direction = Vector3.Normalize(m_nodeGraph[b].m_position - m_nodeGraph[a].m_position);
                        float distanceToUnwalk = Vector3.Distance(m_nodeGraph[a].m_position, position);
                        Vector3 positionOfCheck = m_nodeGraph[a].m_position + direction * distanceToUnwalk;
                        if (Vector3.Distance(positionOfCheck, position) < 1f)
                        {
                            isPassingThrough = true;
                            break;
                        }
                    }
                    //if its going through a unwalkable dont make the connection
                    if (isPassingThrough)
                        continue;
                    //----------------------------------------
                    int indexA = -1;
                    int indexB = -1;
                    float aMaxDist = -1;
                    float bMaxDist = -1;

                    for (int i = 0; i < m_nodeConnectionAmount; i++)
                    {

                        if (m_nodeGraph[a].connections[i] == null)
                        {
                            indexA = i;
                            aMaxDist = -1;
                            break;
                        }
                        float currentDist = Vector3.Magnitude(m_nodeGraph[a].m_position - m_nodeGraph[m_nodeGraph[a].connections[i].to].m_position);
                        if (currentDist > aMaxDist)
                        {
                            //this tells us which is the furthest node away
                            aMaxDist = currentDist;
                            indexA = i;
                        }
                    }
                    //we know the distance is too great
                    if (distBetweenNodes > aMaxDist && aMaxDist != -1)
                        continue;

                    //if it gets to this point we know its in range of a, we then have to check b with the same process

                    for (int i = 0; i < m_nodeConnectionAmount; i++)
                    {
                        if (m_nodeGraph[b].connections[i] == null)
                        {
                            indexB = i;
                            bMaxDist = -1;
                            break;
                        }
                        float currentDist = Vector3.Magnitude(m_nodeGraph[b].m_position - m_nodeGraph[m_nodeGraph[b].connections[i].to].m_position);
                        if (currentDist > bMaxDist)
                        {
                            //this tells us which is the furthest node away
                            bMaxDist = currentDist;
                            indexB = i;
                        }
                    }
                    if (distBetweenNodes > bMaxDist && bMaxDist != -1)
                        continue;

                    //if all that passes then we can add to both
                    m_nodeGraph[a].connections[indexA] = new Edge(b);
                    m_nodeGraph[b].connections[indexB] = new Edge(a);
                }
            }
        }
    }
    public static void DrawNodes()
    {
        if (m_nodeGraph == null)
            return;

        foreach (var node in m_nodeGraph)
        {
            for (int i = 0; i < node.connections.Length - 1; i++)
            {
                //if the connection isnt null then draw a line of it this whole function is self explaining
                if (node.connections[i] != null)
                    Debug.DrawLine(node.m_position, m_nodeGraph[node.connections[i].to].m_position);
            }
        }
    }
    //when creating the nodegraph we need the normal to do a underneath check for overlapped/unwanted nodes
    //so we have a seperate class to do the checks then give the position so we arent storing a normal
    //for no reason in the main node class
    private class NodeCheck
    {
        public Vector3 position;
        public Vector3 normal;
    }
    private class UnWalkableObject
    {
        //overall check so processing isnt wasted
        Vector3 m_boundingExtents;
        //use the centerpoint to see if a connection goes through the bounding box
        Vector3 m_centerPoint;

        //if it does go through the bounding box 
        Vector3[] checkPoints;

    }
}
