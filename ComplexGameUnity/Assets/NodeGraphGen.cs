using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using System.Runtime.InteropServices;
using System;
using System.IO;
/*
* File: NodeGraphGen.cs
*
* Author: Thomas Lamb (s200498@students.aie.edu.au)
* Date Created: 10th May 2021
* Date Last Modified: 12th June 2021
*
* Node generation functions for the editor window
* 
*/
[Serializable]
public class Node
{
    //This will be changed later but it was kept to be the position and connections to reduce memory
    public Vector3 m_position = new Vector3(0, 0, 0);
    public Edge[] connections = new Edge[NodeManager.m_nodeConnectionAmount];
    public Node(Vector3 a_position)
    {
        m_position = a_position;
    }
}
[Serializable]
public class Edge
//edge are the connections to each node in the array
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
    //objects from the editor window to perform the generation
    public static NodeContainer nodeScriptableObject = null;
    public static GameObject walkableObject = null;


    //Static variables for use by the whole system
    public static float m_nodeDistance = 5;
    public static int m_nodeConnectionAmount = 50000;
    public static float m_ySpaceLimit = 1;

    public static int walkableLayer = 0;
    private static int unwalkableLayer = 0;
    
    
    static List<Vector3> m_unwalkablePoints = new List<Vector3>();
    public static Node[] m_nodeGraph = null;

    //Update values from editor window function
    public static void ChangeValues(
        float a_nodeDistance,
        int a_connectionAmount,
        float a_yLimit,
        NodeContainer nodeContainer,
        GameObject a_walkableObject,
        int walkLayer,
        int unwalkLayer)
    {
        m_nodeDistance = a_nodeDistance;
        m_nodeConnectionAmount = a_connectionAmount;
        m_ySpaceLimit = a_yLimit;

        nodeScriptableObject = nodeContainer;
        walkableObject = a_walkableObject;
        walkableLayer = walkLayer;
        unwalkableLayer = unwalkLayer;
    }

    public static void CreateNodes(int a_layerMask)
    {
        //Note. I dont like how unity doesnt have a return all children
        //(or at least i couldn't see one)
        if (walkableObject == null)
            return;

        //I have to do this for checking all children purposes, because some art assets
        //could have 60 children in children so i needed a form of looping
        List<GameObject> foundObjectsList = new List<GameObject>();
        List<GameObject> objectsToSearch = new List<GameObject>();

        objectsToSearch.Add(walkableObject);
        foundObjectsList.Add(walkableObject);

        
        
        while (objectsToSearch.Count > 0)
        {
            //should have used a que but its too late now
            GameObject currentObject = objectsToSearch[0];
            objectsToSearch.RemoveAt(0);
            
            //get all children from the object and add it to the list
            for (int i = 0; i < currentObject.transform.childCount; i++)
            {
                GameObject child = currentObject.transform.GetChild(i).gameObject;
                objectsToSearch.Add(child);
                foundObjectsList.Add(child);
            }
        }
        //put it to an array
        GameObject[] foundObjects = new GameObject[foundObjectsList.Count];
        for (int i = 0; i < foundObjectsList.Count; i++)
        {
            foundObjects[i] = foundObjectsList[i];
        }
        //turn foundobjects into a array to check through

        List<NodeCheck> nodes = new List<NodeCheck>();

        foreach (GameObject currentObject in foundObjects)
        {
            
            if (currentObject.CompareTag("Node"))
                continue;
            
            //if it doesnt have a meshfilter then we cant find any verts so next object
            MeshFilter objectMesh = currentObject.GetComponent<MeshFilter>();
            if (objectMesh == null)
                continue;
            
            //if its on the obstacle layer add to unwalkable
            if (currentObject.layer == unwalkableLayer)
            {
                bool canAdd = true;
                List<Vector3> objectVerts = new List<Vector3>();

                foreach (var vert in objectMesh.sharedMesh.vertices)
                {
                    //this foreach just makes sure for that object we arent getting multiple
                    //points in the same location because of multiple normals for lighting
                    Vector3 vertWorldPos = currentObject.transform.TransformPoint(vert);

                    foreach (var VARIABLE in objectVerts)
                    {
                        if (Vector3.Distance(VARIABLE, vertWorldPos) < 0.3f)
                        {
                            canAdd = false;
                            break;
                        }
                    }
                    //if its all good then we can add to the overall list and that objects
                    //vert position list
                    if (canAdd)
                    {
                        m_unwalkablePoints.Add(vertWorldPos);
                        objectVerts.Add(vertWorldPos);
                    }
                }
                //makes sure it isnt added to the list of actual nodes
                continue;
            }
            //if its layer 12 then we know its walkable
            if (currentObject.layer == walkableLayer)
            {
                Vector3 newNormal = currentObject.transform.TransformDirection(new Vector3(0, 1, 0));
                List<Vector3> objectVerts = new List<Vector3>();
                if (objectMesh.sharedMesh == null)
                    continue;

                //we want to do the same check as the unwalkable for the walkable to not get multiple points
                //of same location because of normals for lighting
                foreach (var vert in objectMesh.sharedMesh.vertices)
                {
                    bool canAdd = true;
                    NodeCheck node = new NodeCheck();
                    node.position = currentObject.transform.TransformPoint(vert);
                    node.normal = newNormal;
                    if (currentObject.transform.CompareTag("Ground"))
                    {
                        node.normal = new Vector3(0, 1, 0);
                    }
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
        LinkNodes();


        //all of this below is horrid but it was made for the vr working properly as
        //actual development showed alot of flaws with this system with edge cases
        foreach (var node in m_nodeGraph)
        {
            for (int i = 0; i < m_nodeConnectionAmount; i++)
            {
                if (node.connections[i] == null)
                {
                    //because of scriptable objects the nodes that dont connect are
                    //all pointing to 0 so i have to give defaults or it will do mass overlaps
                    node.connections[i] = new Edge(-1);
                }
            }
        }

        //some nodes are kept that have no connections this is to remove them
        List<Node> validNodes = new List<Node>();
        foreach (Node node in m_nodeGraph)
        {
            for(int i = 0; i < m_nodeConnectionAmount; i++)
            {
                if (node.connections[i] != null)
                {
                    validNodes.Add(node);
                    break;
                    //break out of that individual nodes loop
                }
            }
        }
        //make the ending node graph the current valid
        m_nodeGraph = null;
        m_nodeGraph = new Node[validNodes.Count];
        for(int i = 0; i < validNodes.Count; i++)
        {
            m_nodeGraph[i] = validNodes[i];
        }
        //give the scriptable object the array
        nodeScriptableObject.NodeGraph = new PathNode[m_nodeGraph.Length];
        for (int i = 0; i < m_nodeGraph.Length; i++)
        {
            //for ease of calculating everything is processed with the basic node
            //then converted to the more memory heavy path node class
            nodeScriptableObject.NodeGraph[i] = new PathNode(m_nodeGraph[i]);
        }
    }

    private static void UnWalkable(ref List<NodeCheck> nodes)
    {
        //this just checks if a node is too close to an unwalkable point
        List<NodeCheck> nodesToDelete = new List<NodeCheck>();
        foreach (var nodeAlpha in nodes)
        {
            foreach (var unwalkPoint in m_unwalkablePoints)
            {
                //Vector2 nodepoint = new Vector2(nodeAlpha.position.x, nodeAlpha.position.z);
                //Vector2 unwalk = new Vector2(unwalkPoint.x, unwalkPoint.z);
            
                if (Vector3.Distance(nodeAlpha.position, unwalkPoint) < 2f)
                        nodesToDelete.Add(nodeAlpha);

                //this one line caused ALOT of issues
                    //if (Mathf.Abs(nodeAlpha.position.y - unwalkPoint.y) < m_ySpaceLimit)
            }
        }
        //we remove after because its a list and it will break if it has a removed element while looping
        foreach (var deletionNode in nodesToDelete)
            nodes.Remove(deletionNode);

        //new nodegraph without the waste nodes
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
        //this checks against every node to make sure that if
        //the node is too close to another one it will then make sure that the one with
        //the higher y value will always be the one that remains
        
        List<NodeCheck> nodesToDelete = new List<NodeCheck>();
        foreach (var nodeAlpha in nodes)
        {
            foreach (var nodeBeta in nodes)
            {
                if (nodeAlpha == nodeBeta)
                    continue;
                if (Vector3.Distance(nodeAlpha.position, nodeBeta.position) < 0.3f)
                {
                    if (nodeAlpha.position.y > nodeBeta.position.y)
                    {
                        if (nodesToDelete.Contains(nodeAlpha) == false)
                            nodesToDelete.Add(nodeBeta);
                    }
                    else
                    {
                        if (nodesToDelete.Contains(nodeBeta) == false)
                            nodesToDelete.Add(nodeAlpha);
                    }
                }
                //we do this check to make sure that it is not the y value that is checked to begin with
                //but that nodes normal position because of rotation just checking the y can lead to deleting the 
                //wrong node
                //so we do a check of the direction to the other node from the first one with a dot product of that
                //direction and the normal negated to see if its ok to delete
                //then remove
                Vector3 alphaToBetaDir = Vector3.Normalize(nodeBeta.position - nodeAlpha.position);
                if (Vector3.Dot(-nodeAlpha.normal, alphaToBetaDir) > 0.7f &&
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
    private static void LinkNodes()
    {
        if (m_nodeGraph == null)
            return;

        //this is to check that the nodes are not already linked or have to many connections already
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

                //if the distance is small enough we can see if a link is possible
                float distBetweenNodes = Vector3.Distance(m_nodeGraph[a].m_position, m_nodeGraph[b].m_position);
                if (distBetweenNodes < m_nodeDistance)
                {
                    //checks between non walkable for connection
                    
                    
                    //this massive extent check is to make sure that we are only checking against unwalkables that are in the range
                    //of the two nodes that we are checking because the unwalkable can be very process heavy
                    bool isPassingThrough = false;
                    foreach (var position in m_unwalkablePoints)
                    {
                        //this is doing an extent check so its not doing a massive normalize and distance check every time
                        float xMin = 0;
                        float zMax = 0;
                        float zMin = 0;
                        float xMax = 0;
                        if (m_nodeGraph[a].m_position.x > m_nodeGraph[b].m_position.x)
                        {
                            xMax = m_nodeGraph[a].m_position.x;
                            xMin = m_nodeGraph[b].m_position.x;
                        }
                        else
                        {
                            xMax = m_nodeGraph[b].m_position.x;
                            xMin = m_nodeGraph[a].m_position.x;
                        }
                        if (m_nodeGraph[a].m_position.z > m_nodeGraph[b].m_position.z)
                        {
                            zMax = m_nodeGraph[a].m_position.z;
                            zMin = m_nodeGraph[b].m_position.z;
                        }
                        else
                        {
                            zMax = m_nodeGraph[b].m_position.z;
                            zMin = m_nodeGraph[a].m_position.z;
                        }

                        if (position.x > xMax || position.x < xMin || position.z < zMin || position.z > zMax)
                            continue;
                        
                        //if the unwalkable is in the range of the nodes then we find if the unwalkable point 
                        //is on that line direction of the first node to the second
                        Vector3 direction = Vector3.Normalize(m_nodeGraph[b].m_position - m_nodeGraph[a].m_position);
                        float distanceToUnwalk = Vector3.Distance(m_nodeGraph[a].m_position, position);
                        Vector3 positionOfCheck = m_nodeGraph[a].m_position + direction * distanceToUnwalk;
                        //if it is close enough then we can not link the nodes
                        if (Vector3.Distance(positionOfCheck, position) < 2f)
                        {
                            isPassingThrough = true;
                            break;
                        }
                    }
                    //if its going through a unwalkable dont make the connection
                    if (isPassingThrough)
                        continue;
                    
                    
                    //This is actually aweful but it checks each nodes connections to find if the distance of the nodes
                    //is less than the current connections or if both have a empty slot
                    int indexA = -1;
                    int indexB = -1;
                    float aMaxDist = -1;
                    float bMaxDist = -1;
                    bool nullFound = false;
                    for (int i = 0; i < m_nodeConnectionAmount; i++)
                    {
                        if (m_nodeGraph[a].connections[i] == null)
                        {
                            indexA = i;
                            nullFound = true;
                            break;
                        }
                        float currentDist = Vector3.Distance(m_nodeGraph[a].m_position, m_nodeGraph[m_nodeGraph[a].connections[i].to].m_position);
                        if (currentDist > aMaxDist)
                        {
                            //this tells us which is the furthest node away
                            aMaxDist = currentDist;
                            indexA = i;
                        }
                    }

                    //we know the distance is too great
                    if (distBetweenNodes < aMaxDist || nullFound)
                    {
                        //if it gets to this point we know its in range of a, we then have to check b with the same process
                        nullFound = false;
                        for (int i = 0; i < m_nodeConnectionAmount; i++)
                        {
                            if (m_nodeGraph[b].connections[i] == null)
                            {
                                indexB = i;
                                nullFound = true;
                                break;
                            }

                            float currentDist = Vector3.Distance(m_nodeGraph[b].m_position, m_nodeGraph[m_nodeGraph[b].connections[i].to].m_position);
                            if (currentDist > bMaxDist)
                            {
                                //this tells us which is the furthest node away
                                bMaxDist = currentDist;
                                indexB = i;
                            }
                        }

                        if (distBetweenNodes < bMaxDist || nullFound)
                        {
                            //if all that passes then we can add to both
                            m_nodeGraph[a].connections[indexA] = new Edge(b);
                            m_nodeGraph[b].connections[indexB] = new Edge(a);
                        }
                    }
                }
            }
        }
    }
    //drawing node function
    public static void DrawNodes()
    {
        if (m_nodeGraph == null)
            return;

        foreach (var node in nodeScriptableObject.NodeGraph)
        {
            for (int i = 0; i < node.connections.Length - 1; i++)
            {
                //if the connection isnt null then draw a line of it this whole function is self explaining
                if (node.connections[i] != null)
                    if (node.connections[i].to != -1)
                        Debug.DrawLine(node.m_position, nodeScriptableObject.NodeGraph[node.connections[i].to].m_position);
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
}
