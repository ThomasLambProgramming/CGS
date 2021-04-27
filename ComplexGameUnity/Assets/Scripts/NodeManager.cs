using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Node
{
    //by making a preset i can make these nodes into an array 
    public Node[] m_connectedNodes = null;
    public Vector3 m_position;
    public Node(int a_nodeConnectionLimit, Vector3 a_position)
    {
        m_position = a_position;
        m_connectedNodes = new Node[a_nodeConnectionLimit];
    }
}
public class NodeManager : MonoBehaviour
{
    private ComputeShader m_NodeLinkingShader;
    
    //Static variables for use by the whole system
    public static int m_nodeDistance = 5;
    public static int m_nodeConnectionAmount = 4;
    public static int m_maxNodes = 1000;
    
    //this is the result of baking and linking the nodes
    //This is an array so the memory is compact and can be randomly accessed by the 
    //pathfinding algorithm so for loops can be avoided
    public static Node[] m_nodeGraph = null;
    
    //list of all the node positions for the linking process
    private static List<Node> nodePositions = new List<Node>();
    //list of all the object positions so that the system knows what objects have already 
    //been processed (this does imply no objects are allowed to overlap perfectly)
    private static List<Vector3> objectPositions = new List<Vector3>();
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
            if (objectPositions.Contains(currentObject.transform.position))
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
                    //DEBUG----------------------------------------------
                    GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //this is debug but will also be used for the main system
                    nodeObj.transform.position = vert + currentObject.transform.position;
                    nodeObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    nodeObj.tag = "Node";
                    nodeObj.transform.parent = currentObject.transform;
                    //Debug End-----------------------------------------------------
                    
                    //add the vert to check for overlaps
                    overlapCheck.Add(vert);
                    //vert + gameobject to get world position, must add scaling later
                    nodePositions.Add(new Node(m_nodeConnectionAmount, nodeObj.transform.position));
                }
            }
            //add the checked object for later to stop double baking
            objectPositions.Add(currentObject.transform.position);
        }
    }

    public static void LinkNodes()
    {
        //this list is so it can be dynamically added to then converted into an array
        List<Node> nodes = new List<Node>();
        foreach (var node1 in nodePositions)
        {
            foreach (var node2 in nodePositions)
            {
                //if same node goto next
                if (node1 == node2)
                    continue;
                
                //if distance between nodes less than set distance then we can add
                if (Vector3.Distance(node1.m_position, node2.m_position) < m_nodeDistance)
                {
                    //this is to make sure that each node can be added too forwards and backwards
                    //with added error checking incase of one node being able to take in another connection
                    //while the other node cannot
                    
                    
                    //save the index of the first node so we can add both at the same time
                    int firstIndexToAdd = -1;
                    for (int i = 0; i < m_nodeConnectionAmount - 1; i++)
                    {
                        if (node1.m_connectedNodes[i] == null)
                        {
                            firstIndexToAdd = i;
                            
                        }
                    }
                    //if the index is 0 or above we know it has an avaiable position
                    if (firstIndexToAdd >= 0)
                    {
                        bool canAddToSecond = false;
                        for (int i = 0; i < m_nodeConnectionAmount - 1; i++)
                        {
                            if (node2.m_connectedNodes[i] == null)
                            {
                                node2.m_connectedNodes[i] = node1;
                                node1.m_connectedNodes[firstIndexToAdd] = node2;
                            }
                        }
                    }
                }
            }
        }

        if (nodes.Count > 0)
        {
            //make an array to fit nodes with added redundancy 
            m_nodeGraph = new Node[nodes.Count + 4];
            
            //add all nodes into the array
            //This is not efficent to recreate entire containers but having the ability to random access
            //nodes is a big performance increase (especially with the compute shaders) 
            for (int i = 0; i < nodes.Count + 3; i++)
            {
                m_nodeGraph[i] = nodes[i];
            }
        }
    }

    //This is a debug option
    public static void DrawNodes()
    {
        
    }
}
