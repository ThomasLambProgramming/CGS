using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public class Node
{
    //by making a preset i can make these nodes into an array 
    public Node[] m_connectedNodes = null;
    public Vector3 m_position = new Vector3(0,0,0);
    public int m_connectionAmount = 0;
    public Node(int a_nodeConnectionLimit, Vector3 a_position)
    {
        m_position = a_position;
        m_connectedNodes = new Node[a_nodeConnectionLimit];
        m_connectionAmount = 0;
    }
}

public class NodeManager : MonoBehaviour
{
    private ComputeShader m_NodeLinkingShader;

    //Static variables for use by the whole system
    private static float m_nodeDistance = 5;
    private static int m_nodeConnectionAmount = 4;
    private static int m_maxNodes = 1000;
    private static float m_ySpaceLimit = 1;

    //this is the result of baking and linking the nodes
    //This is an array so the memory is compact and can be randomly accessed by the 
    //pathfinding algorithm so for loops can be avoided
    public static Node[] m_nodeGraph = null;

    //list of all the node positions for the linking process
    private static List<Node> m_createdNodes = new List<Node>();

    //list of all the object positions so that the system knows what objects have already 
    //been processed (this does imply no objects are allowed to overlap perfectly)
    private static List<Vector3> m_objectPositions = new List<Vector3>();

    //debug purposes
    public static void ResetValues()
    {
        m_createdNodes = new List<Node>();
        m_objectPositions = new List<Vector3>();
        m_nodeGraph = null;
    }
    public static void ChangeValues(float a_nodeDistance, int a_connectionAmount, int a_maxNodes, float a_yLimit)
    {
        m_nodeDistance = a_nodeDistance;
        m_nodeConnectionAmount = a_connectionAmount;
        m_maxNodes = a_maxNodes;
        m_ySpaceLimit = a_yLimit;
    }
    public static void CreateNodes(int a_layerMask)
    {
        //gets every gameobject (change later to be a selection of some sort, possibly layers or manual selection   
        GameObject[] foundObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject currentObject in foundObjects)
        {

            //if its a node (debug currently creates spheres) goto next object
            if (currentObject.CompareTag("Node"))
                continue;

            //if the object doesnt have a mesh next
            MeshFilter objectMesh = currentObject.GetComponent<MeshFilter>();
            if (objectMesh == null)
                continue;

            //if the object has already been processed next
            if (m_objectPositions.Contains(currentObject.transform.position))
                continue;

            //this checks for objects that have multiple normals at the same position (lighting)
            List<Vector3> overlapCheck = new List<Vector3>();
            foreach (var vert in objectMesh.sharedMesh.vertices)
            {
                bool canAdd = true;
                foreach (var position in overlapCheck)
                {
                    if (Vector3.Distance(vert, position) < 0.01f)
                        canAdd = false;
                }

                if (canAdd)
                {
                    Vector3 vertScale = new Vector3(
                        vert.x * currentObject.transform.localScale.x,
                        vert.y * currentObject.transform.localScale.y,
                        vert.z * currentObject.transform.localScale.z);
                    Vector3 vertWorldPos = currentObject.transform.TransformPoint(vertScale);
                    
                    //DEBUG----------------------------------------------
                    GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //this is debug but will also be used for the main system
                    nodeObj.transform.position = vertWorldPos;
                    nodeObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    nodeObj.tag = "Node";
                    nodeObj.transform.parent = currentObject.transform;
                    //Debug End-----------------------------------------------------

                    //add the vert to check for overlaps
                    overlapCheck.Add(vert);
                    m_createdNodes.Add(new Node(m_nodeConnectionAmount, vertWorldPos));
                }
            }

            //add the checked object for later to stop double baking
            m_objectPositions.Add(currentObject.transform.position);
        }
        Debug.Log("CREATION PASSED");

    }
    public static void LinkNodes(float a_nodeDistance, bool a_firstRun = true)
    {
        if (m_createdNodes == null)
            return;

        foreach (var node1 in m_createdNodes)
        {
            foreach (var node2 in m_createdNodes)
            {
                //if same node goto next
                if (node1 == node2)
                    continue;
                
                //Double up check
                bool hasDupe = false;
                foreach (var VARIABLE in node2.m_connectedNodes)
                {
                    if (VARIABLE == node1)
                        hasDupe = true;
                }
                foreach (var VARIABLE in node1.m_connectedNodes)
                {
                    if (VARIABLE == node2)
                        hasDupe = true;
                }
                if (hasDupe)
                    continue;

                //if distance between nodes less than set distance then we can add
                if (Vector3.Distance(node1.m_position, node2.m_position) < a_nodeDistance
                    && Mathf.Abs(node1.m_position.y - node2.m_position.y) < m_ySpaceLimit)
                {
                    //if the nodes have a spare slot then add eachother (for making it easy nodes have a current index
                    //used amount so its easy to tell if all the connection amounts are full
                    if (node1.m_connectionAmount < m_nodeConnectionAmount &&
                        node2.m_connectionAmount < m_nodeConnectionAmount)
                    {
                        for (int i = 0; i < m_nodeConnectionAmount; i++)
                        {
                            if (node1.m_connectedNodes[i] == null)
                            {
                                node1.m_connectedNodes[i] = node2;
                                node1.m_connectionAmount++;
                                break;
                            }
                        }

                        for (int i = 0; i < m_nodeConnectionAmount; i++)
                        {
                            if (node2.m_connectedNodes[i] == null)
                            {
                                node2.m_connectedNodes[i] = node1;
                                node2.m_connectionAmount++;
                                break;
                            }
                        }
                    }
                }
            }
        }
        //we delete every overlap of nodes on the xz then run the link again
        
        if (a_firstRun)
        {
            DeleteOverlapNodes();
            LinkNodes(m_nodeDistance, false);
            return;
        }
        //make an array to fit nodes with added redundancy 
        m_nodeGraph = new Node[m_createdNodes.Count];

        //add all nodes into the array
        //This is not efficent to recreate entire containers but having the ability to random access
        //nodes is a big performance increase (especially with the compute shaders) 
        for (int i = 0; i < m_createdNodes.Count; i++)
        {
            m_nodeGraph[i] = m_createdNodes[i];
        }
        Debug.Log("LINK PASSED");
    }
    //This is a debug option
    public static void DrawNodes()
    {
        if (m_nodeGraph == null)
            return;

        foreach (var node in m_nodeGraph)
        {
            for(int i = 0; i < node.m_connectionAmount - 1; i++)
            {
                if (node.m_connectedNodes[i] != null)
                    Debug.DrawLine(node.m_position,node.m_connectedNodes[i].m_position);
            }
        }
        Debug.Log("DRAW PASSED");
    }
    private static void DeleteOverlapNodes()
    {
        foreach (var node1 in m_createdNodes)
        {
            foreach (var node2 in m_createdNodes)
            {
                if (node1 == node2)
                    continue;

                if (Mathf.Abs(node1.m_position.x - node2.m_position.x) < 0.1f && Mathf.Abs(node1.m_position.z - node2.m_position.z) < 0.1f)
                {
                    if (NodeRemoval(node1, node2))
                    {
                        DeleteOverlapNodes();
                        return;
                    }
                    else if (NodeRemoval(node2, node1))
                    {
                        DeleteOverlapNodes();
                        return;
                    }
                }
            }
        }
        
    }
    private static bool NodeRemoval(Node node1, Node node2)
    {
        if (node1.m_position.y > node2.m_position.y)
        {
            foreach (var connection in node2.m_connectedNodes)
            {
                if (connection == null)
                    continue;
               

                for(int i = 0; i < m_nodeConnectionAmount; i++)
                {
                    if (connection.m_connectedNodes[i] == null)
                        continue;

                    if (connection.m_connectedNodes[i] == node2)
                    {
                        connection.m_connectedNodes[i] = null;
                        connection.m_connectionAmount--;
                        break;
                    }
                    
                }
            }
            m_createdNodes.Remove(node2);
            return true;
        }
        return false;
    }
}
