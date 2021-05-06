using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

//get the object point (hopefully middle or bottom and do a direction dot product check for if the node will be above or below to remove verts

/*
    Optimization ideas
    sort the nodes by position (since the node graph has random access knowing the bounds of the x,z,y we can use binary search for the finding of closest nodes)
    possible griding of the nodegraph into sections with x,z bounds to tell if it needs to search the whole graph or not (can also use binary search as well here for finding grid distances and etc)

*/
public class Node
{
    //by making a preset i can make these nodes into an array 
    public Edge[] m_connectedNodes = null;
    public Vector3 m_position = new Vector3(0,0,0);
    
    //this is the nodes normal for checking if other nodes need to be deleted
    public Node(int a_nodeConnectionLimit, Vector3 a_position)
    {
        m_position = a_position;
        m_connectedNodes = new Edge[a_nodeConnectionLimit];
    }
}
//For the gpu we need to seperate the class to not contain node itself as a connection as the gpu hates it
public class Edge
{
    public Node to = null;
    public float cost = 0;
}
public class NodeManager : MonoBehaviour
{
    //Static variables for use by the whole system
    public static float m_nodeDistance = 5;
    public static int m_nodeConnectionAmount = 4;
    public static int m_maxNodes = 1000;
    public static float m_ySpaceLimit = 1;
    
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
    
    //when creating the nodegraph we need the normal to do a underneath check for overlapped/unwanted nodes
    //so we have a seperate class to do the checks then give the position so we arent storing a normal
    //for no reason in the main node class
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
                //get the world position of the vert from the main object
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
        //give a ref so we arent double spawning the nodes
        Overlap(ref nodes);
        Debug.Log("CREATION PASSED");
    }

    public static void Overlap(ref List<nodeCheck> nodes)
    {
        //making this second list allows us to cycle through the list without modifying it and breaking the
        //for loop
        List<nodeCheck> nodesToDelete = new List<nodeCheck>();
        foreach (var nodeAlpha in nodes)
        {
            foreach (var nodeBeta in nodes)
            {
                if (nodeAlpha == nodeBeta)
                    continue;
                
                //get the direction to beta from alpha and normalize for a directional vector
                Vector3 alphaToBetaDir = Vector3.Normalize(nodeBeta.position - nodeAlpha.position);

                //we get the -y axis (-node normal) to check if the direction to node2 is similar enough to the -y axis
                //which tells us if we have a unwanted node
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
            m_createdNodes.Add(new Node(m_nodeConnectionAmount, VARIABLE.position));
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
                    if (VARIABLE.to == node1)
                        hasDupe = true;
                }
                foreach (var VARIABLE in node1.m_connectedNodes)
                {
                    if (VARIABLE == null)
                        continue;
                    if (VARIABLE.to == node2)
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
                    //checks to see what part of the array is null so we dont overwrite or add to something that doesnt have space.
                        for (int i = 0; i < m_nodeConnectionAmount; i++)
                        {
                            if (node1.m_connectedNodes[i] == null)
                            {
                                //im making the weight for now have a heigher weight based on how much 
                                node1.m_connectedNodes[i] = new Edge();
                                node1.m_connectedNodes[i].to = node2;
                                break;
                            }
                        }
                        for (int i = 0; i < m_nodeConnectionAmount; i++)
                        {
                            if (node2.m_connectedNodes[i] == null)
                            {
                                node1.m_connectedNodes[i] = new Edge();
                                node1.m_connectedNodes[i].to = node1;
                                break;
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
            for(int i = 0; i < node.m_connectedNodes.Length - 1; i++)
            {
                //if the connection isnt null then draw a line of it this whole function is self explaining
                if (node.m_connectedNodes[i] != null)
                    Debug.DrawLine(node.m_position,node.m_connectedNodes[i].to.m_position);
            }
        }
        Debug.Log("DRAW PASSED");
    }
    
}
