using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
public struct PathFindJob : IJob
{
    class PathNode : IHeapItem<PathNode>
    {
        public float m_totalCost = 0;
        public float m_gCost = 0;
        public float m_hCost = 0;
        public float m_fCost { get { return m_gCost + m_hCost; } }
        public Node node = null;
        public PathNode m_parent = null;
        int itemIndex;
        public PathNode(Node a_node1, PathNode a_parent)
        {
            node = a_node1;
            m_parent = a_parent;
        }
        public int ItemIndex
        {
            get { return itemIndex; }
            set { itemIndex = value; }
        }
        public int CompareTo(PathNode other)
        {
            //get the compare to the f costs
            int compare = m_fCost.CompareTo(other.m_fCost);
            //if they are the same then we go off hcost (closer to endnode)
            if (compare == 0)
                compare = m_hCost.CompareTo(other.m_hCost);
            //as we want the priority(smaller fcost) rather than which one is bigger
            //we give the - to reverse
            return -compare;
        }
    }
    //index of the agents start and end nodes
    [ReadOnly] public NativeArray<Vector3> startEndPos;
    
    public NativeArray<Vector3> pathResult;
    public void Execute()
    {
        Heap<PathNode> openNodes = new Heap<PathNode>(NodeManager.m_nodeGraph.Length);
        HashSet<Node> closedNodes = new HashSet<Node>();

        //find the closest node from the start and finish, after the path is found there will be an additional
        //check that can be performed to see if the point can be reached after the path (eg the closest may be at the
        //start of a mountain but there are no nodes and the end point is the peak)
        Node startNode1 = NodeManager.m_nodeGraph[NodeUtility.FindClosestNode(startEndPos[0])];
        Node endNode1 = NodeManager.m_nodeGraph[NodeUtility.FindClosestNode(startEndPos[1])];
        
        //giving it null as it is the starting node
        PathNode start = new PathNode(startNode1, null);
        start.m_gCost = 0;
        start.m_hCost = Vector3.Distance(start.node.m_position, endNode1.m_position);
        openNodes.Add(start);


        while (openNodes.Count > 0)
        {
            //to avoid a length and smallest before setting check gscore of current, by the end
            //the smallest will be the current
            PathNode currentNode = openNodes.RemoveFirst();
            closedNodes.Add(currentNode.node);

            if (currentNode.node == endNode1)
            {
                //We found the end node pathfinding is done
                List<Vector3> path = new List<Vector3>();
                //add the current/end node so that its just the parents being added in the loop
                path.Add(currentNode.node.m_position);
                while (currentNode.m_parent != null)
                {
                    path.Add(new Vector3(
                        currentNode.m_parent.node.m_position.x,
                        currentNode.m_parent.node.m_position.y,
                        currentNode.m_parent.node.m_position.z));
                    currentNode = currentNode.m_parent;
                }
                pathResult = new NativeArray<Vector3>(path.Count, Allocator.Temp);
                for (int i = 0; i < path.Count; i++)
                {
                    pathResult[i] = path[i];
                }
                return;
            }

            foreach (Edge connection in currentNode.node.connections)
            {
                if (connection == null || closedNodes.Contains(NodeManager.m_nodeGraph[connection.to]))
                    continue;

                bool isOpen = false;
                for (int i = 0; i < openNodes.Count; i++)
                {
                    if (openNodes.items[i].node == NodeManager.m_nodeGraph[connection.to])
                    {
                        isOpen = true;
                        break;
                    }
                }
                if (isOpen)
                    continue;

                PathNode node = new PathNode(NodeManager.m_nodeGraph[connection.to], currentNode);
                node.m_gCost = Vector3.Distance(node.node.m_position, currentNode.node.m_position) + currentNode.m_gCost;

                //float distanceToConnection = currentNode.m_gCost + Vector3.Distance(currentNode.node.m_position, NodeManager.m_nodeGraph[connection.to].m_position);
                node.m_hCost = Vector3.Distance(node.node.m_position, endNode1.m_position);
                openNodes.Add(node);

            }
        }
        //add a end case if a path cannot be found
    }
}
public class NodeUtility : MonoBehaviour
{
    //probably need to fix all this
    public static int FindClosestNode(Vector3 a_position)
    {
        if (NodeManager.m_nodeGraph == null)
            return -1;

        int closestNode = -1;
        float distance = 1000000;

        for (int i = 0; i < NodeManager.m_nodeGraph.Length; i++)
        {
            float nodeDist = Vector3.Distance(NodeManager.m_nodeGraph[i].m_position, a_position);
            if (nodeDist < distance)
            {
                distance = nodeDist;
                closestNode = i;
            }
        }
        return closestNode;
    }
}
//public class AgentUtility
//{

    //public static Vector3[] FindPath(Vector3 a_startPosition, Vector3 a_endPosition)
    //{
    //    if (NodeManager.m_nodeGraph == null)
    //    {
    //        Debug.Log("There is no node graph! Please create one from the node window" +
    //            "Window/NodeGraph.");
    //        return null;
    //    }

    //    Vector3[] path;
    //    PathFindJob pathfind = new PathFindJob();
    //    Vector3[] tempVectors = { a_startPosition, a_endPosition };
    //    NativeArray<Vector3> startEndPos = new NativeArray<Vector3>(tempVectors, Allocator.Temp);
    //    pathfind.startEndPos = startEndPos;

    //    //test for path finding time will need to improve the memory grabbing though its a bit slow and can be grouped
    //    //float time = Time.realtimeSinceStartup;
    //    pathfind.Execute();
    //    //Debug.Log(Time.realtimeSinceStartup - time);
    //    if (!pathfind.pathResult.IsCreated)
    //    {
    //        if (startEndPos.IsCreated)
    //            startEndPos.Dispose();
    //        Debug.Log("Pathresult was null");
    //        return null;
    //    }
    //    path = pathfind.pathResult.ToArray();
    //    pathfind.pathResult.Dispose();
    //    startEndPos.Dispose();

    //    return path;
    //}
    //public static Vector3[] GetRandomPath(Vector3 a_objectPosition)
    //{
    //    //yes this is horrible
    //    return FindPath(a_objectPosition,
    //        NodeManager.m_nodeGraph[UnityEngine.Random.Range(0, NodeManager.m_nodeGraph.Length)].m_position);
    //}
//}
//this is a tree like container for pathfinding, its purpose is to have the nodes sorted by cost
//rather than the original version of pathfinding where it cycled through the openset 
//most of this is copied from a tutorial as there wasnt much i could change without breaking the implementation
//it was hand copied not ctrl c and i added comments to show that i do know what its doing
public class Heap<T> where T : IHeapItem<T>
{
    public T[] items;
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
        while (true)
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

