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


                float distBetweenNodes = Vector3.Distance(m_nodeGraph[a].m_position, m_nodeGraph[b].m_position);
                //this is to stop diagonals (i completly forget how this random stuff works)
                if (distBetweenNodes > 1.3 && m_nodeGraph[a].m_position.y - m_nodeGraph[b].m_position.y == 0)
                    continue;

                if (distBetweenNodes < a_nodeDistance)
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

//this is a tree like container for pathfinding, its purpose is to have the nodes sorted by cost
//rather than the original version of pathfinding where it cycled through the openset 
//most of this is copied from a tutorial as there wasnt much i could change without breaking the implementation
//it was hand copied not ctrl c and i added comments to show that i do know what its doing
public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    //keeps current "used" amount in the array
    int currentCount;
    public Heap(int a_maxSize)
    {
        items = new T[a_maxSize];
    }
    public void Add(T item)
    {
        //makes the index = current count (count = last index + 1 so no need to do checks)
        item.ItemIndex = currentCount;
        //as we tell the heap the max size out of range checks arent needed as it means the size was incorrectly made
        items[currentCount] = item;
        //sort the item to correct position
        SortUp(item);
        currentCount++;
    }
    public bool Contains(T a_item)
    {
        //as the items are forced to have the index in them then its index should equal itself (if not its not there)
        return Equals(items[a_item.ItemIndex], a_item);
    }
    public int Count
    {
        get { return currentCount; }
    }
    //this is for the pathfinding when a node is checked with a different parent the fcost might change and it needs
    //to be updated
    public void UpdateItem(T a_item)
    {
        SortUp(a_item);
    }
    public T RemoveFirst()
    {
        T rootItem = items[0];
        currentCount--;
        //make the last item the root item
        items[0] = items[currentCount];
        items[0].ItemIndex = 0;
        //sort the root down to fill in the removal of the first
        SortDown(items[0]);
        return rootItem;
    }
    public void SortDown(T a_item)
    {
        while(true)
        {
            //as this is a sort of binary tree the nodes are always twos (i dont know if there are edge cases it jsut seems to work)

            int childLeftIndex = a_item.ItemIndex * 2 + 1;
            int childRightIndex = a_item.ItemIndex * 2 + 2;
            int swapIndex;

            //if the index is in range
            if (childLeftIndex < currentCount)
            {
                swapIndex = childLeftIndex;

                //if the right index is also in range of the array
                if (childRightIndex < currentCount)
                {
                    //if the left index has lower priority then right then we set right
                    if (items[childLeftIndex].CompareTo(items[childRightIndex]) < 0)
                    {
                        swapIndex = childRightIndex;
                    }
                }
                //if our item has a lower prio then the child then we swap
                if (a_item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(a_item, items[swapIndex]);
                }
                else
                    return;
            }
            //as we always add and remove to the left if the index is out of range on the left then
            //we know there is no children of this node and can exit out as it is already sorted down
            else
                return;
        }
    }
    public void SortUp(T a_item)
    {
        //same as the sortdown the structure of this tree makes it so parent = (n-1) /2 to get index
        int parentIndex = (a_item.ItemIndex - 1) / 2;
        while (true)
        {
            T parent = items[parentIndex];
            //if the item has a higher priority then swap else its in the right position
            if (a_item.CompareTo(parent) > 0)
                Swap(a_item, parent);
            else
                break;

            parentIndex = (a_item.ItemIndex - 1) / 2;
        }
    }
    public void Swap(T item1, T item2)
    {
        items[item1.ItemIndex] = item2;
        items[item2.ItemIndex] = item1;

        //make a small buffer so it doesnt overwrite
        int item1Index = item1.ItemIndex;
        item1.ItemIndex = item2.ItemIndex;
        item2.ItemIndex = item1Index;
    }
}
public interface IHeapItem<T> : IComparable<T>
{
    int ItemIndex { get; set; }
}
