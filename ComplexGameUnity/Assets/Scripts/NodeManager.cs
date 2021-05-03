using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
//NOTES FOR WHAT TO ADD OR REMOVE LATER
//Include option for objects to be selected for normal or for y axis checks
//get the object point (hopefully middle or bottom and do a direction dot product check for if the node will be above or below to remove verts


/*
    Optimization ideas
    sort the nodes by position (since the node graph has random access knowing the bounds of the x,z,y we can use binary search for the finding of closest nodes)

    dot product for finding angles on objects instead of normal / square check

    possible griding of the nodegraph into sections with x,z bounds to tell if it needs to search the whole graph or not (can also use binary search as well here for finding grid distances and etc)

*/
public class Node
{
    //by making a preset i can make these nodes into an array 
    public Node[] m_connectedNodes = null;
    public Vector3 m_position = new Vector3(0,0,0);
    public int m_connectionAmount = 0;
    //this is the nodes normal for checking if other nodes need to be deleted
    public Vector3 m_normal = new Vector3(0, 0, 0);
    public Node(int a_nodeConnectionLimit, Vector3 a_position, Vector3 a_normal)
    {
        m_position = a_position;
        m_connectedNodes = new Node[a_nodeConnectionLimit];
        m_normal = a_normal;
        m_connectionAmount = 0;
    }
}
public class NodeManager : MonoBehaviour
{
    private static ComputeShader m_NodeLinkingShader = null;
    
    //Static variables for use by the whole system
    private static float m_nodeDistance = 5;
    private static int m_nodeConnectionAmount = 4;
    private static int m_maxNodes = 1000;
    private static float m_ySpaceLimit = 1;
    
    public static Node[] m_nodeGraph = null;
    
    private static List<Node> m_createdNodes = new List<Node>();
    
    public static void ResetValues()
    {
        m_createdNodes = new List<Node>();
        m_nodeGraph = null;
    }
    public static void ChangeValues(float a_nodeDistance, int a_connectionAmount, int a_maxNodes, float a_yLimit)
    {
        m_nodeDistance = a_nodeDistance;
        m_nodeConnectionAmount = a_connectionAmount;
        m_maxNodes = a_maxNodes;
        m_ySpaceLimit = a_yLimit;
    }
    public class nodeCheck
    {
        public Vector3 position;
        public Vector3 normal;
    }
    
    public static void CreateNodes(int a_layerMask)
    {
        //gets every gameobject (change later to be a selection of some sort, possibly layers or manual selection   
        GameObject[] foundObjects = FindObjectsOfType<GameObject>();
        List<nodeCheck> nodes = new List<nodeCheck>();
        
        foreach (GameObject currentObject in foundObjects)
        {
            //if its a node goto next object
            //this must be changed later to allow for masking and etc
            if (currentObject.CompareTag("Node"))
                continue;

            //if the object doesnt have a mesh next
            MeshFilter objectMesh = currentObject.GetComponent<MeshFilter>();
            if (objectMesh == null)
                continue;

            //we get the y default and get the transforms version so we get the correct y axis of the gameobject
            Vector3 newNormal = currentObject.transform.TransformDirection(new Vector3(0, 1, 0));
            
            //vertex positions of the current object to avoid overlaps and reduce entire list for loops
            List<Vector3> objectVerts = new List<Vector3>();
            foreach (var vert in objectMesh.sharedMesh.vertices)
            {
                bool canAdd = true;
                nodeCheck node = new nodeCheck();
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

    public static void Overlap(ref List<nodeCheck> nodes)
    {
        List<nodeCheck> nodesToDelete = new List<nodeCheck>();
        foreach (var nodeAlpha in nodes)
        {
            foreach (var nodeBeta in nodes)
            {
                //get the direction to beta from alpha and normalize for a directional vector
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
        {
            nodes.Remove(deletionNode);
        }
        foreach (var VARIABLE in nodes)
        {
            m_createdNodes.Add(new Node(m_nodeConnectionAmount, VARIABLE.position, VARIABLE.normal));
        }
    }
    //Im going to have to change this to use the dot product so its not doing as many distance checks
    public static void LinkNodes(float a_nodeDistance, bool a_firstRun = true)
    {
        if (m_createdNodes == null)
            return;

        

        //loop each node over the whole collection for joining and deleting as needed
        //this needs to be changed to the graphics system soon instead of cpu loop
        foreach (var node1 in m_createdNodes)
        {
            foreach (var node2 in m_createdNodes)
            {
                //if same node goto next
                if (node1 == node2)
                    continue;
                
                //Double up check, checks both current node's connections to see if they are already linked
                bool hasDupe = false;
                foreach (var VARIABLE in node2.m_connectedNodes)
                {
                    if (VARIABLE == null)
                        continue;
                    if (VARIABLE == node1)
                        hasDupe = true;
                }
                foreach (var VARIABLE in node1.m_connectedNodes)
                {
                    if (VARIABLE == null)
                        continue;
                    if (VARIABLE == node2)
                        hasDupe = true;
                }
                if (hasDupe)
                    continue;

                //if distance between nodes less than set distance then we can add
                //add a_nodeDistance tothe distance check when done
                if (Vector3.Distance(node1.m_position, node2.m_position) > 1.3 && node1.m_position.y - node2.m_position.y == 0)
                    continue; 

                if (Vector3.Distance(node1.m_position, node2.m_position) < a_nodeDistance)
                {
                    //if the nodes have a spare slot then add eachother (for making it easy nodes have a current index
                    //used amount so its easy to tell if all the connection amounts are full
                    if (node1.m_connectionAmount < m_nodeConnectionAmount &&
                        node2.m_connectionAmount < m_nodeConnectionAmount)
                    {
                        //checks to see what part of the array is null so we dont overwrite or add to something that doesnt have space.
                        for (int i = 0; i < m_nodeConnectionAmount; i++)
                        {
                            if (node1.m_connectedNodes[i] == null)
                            {
                                //im making the weight for now have a heigher weight based on how much 
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
                //if the connection isnt null then draw a line of it this whole function is self explaining
                if (node.m_connectedNodes[i] != null)
                    Debug.DrawLine(node.m_position,node.m_connectedNodes[i].m_position);
            }
        }
        Debug.Log("DRAW PASSED");
    }
    
}
