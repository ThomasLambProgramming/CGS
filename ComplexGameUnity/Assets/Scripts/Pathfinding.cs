using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
/*
* File: Pathfinding.cs
*
* Author: Thomas Lamb (s200498@students.aie.edu.au)
* Date Created: 10th May 2021
* Date Last Modified: 12th June 2021
*
* Pathfinding script to do the main pathfinding called from the enemies and
* hammer 
* 
*/
[Serializable]
public class PathNode : IHeapItem<PathNode>
{
    //the costs are the distances to the start and end node
    public float m_totalCost = 0;
    //dist to start
    public float m_gCost = 0;
    //dist to end
    public float m_hCost = 0;
    public float m_fCost { get { return m_gCost + m_hCost; } }
    
    public Vector3 m_position = Vector3.zero;
    public Edge[] connections = new Edge[NodeManager.m_nodeConnectionAmount];
    public PathNode m_parent = null;
    int itemIndex;
    public PathNode(Node a_node1, PathNode a_parent = null)
    {
        m_position = a_node1.m_position;
        connections = a_node1.connections;
        m_parent = a_parent;
    }

    public PathNode(PathNode toCopy)
    {
        m_position = toCopy.m_position;
        connections = toCopy.connections;
        m_parent = null;
        
    }
    //item index is used to keep track of the heap 
    //the item index and compare to are required to use the heap container
    //we put the index in here so we can access it from the variable
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
public struct PathFindJob : IJob
{
    
    //index of the agents start and end nodes
    [ReadOnly] public NativeArray<int> startEndPos;
    
    public NativeArray<Vector3> pathResult;
    public PathNode[] NodeData;
    
    public void Execute()
    {
        Heap<PathNode> openNodes = new Heap<PathNode>(NodeData.Length);
        HashSet<PathNode> closedNodes = new HashSet<PathNode>();

        //find the closest node from the start and finish, after the path is found there will be an additional
        //check that can be performed to see if the point can be reached after the path (eg the closest may be at the
        //start of a mountain but there are no nodes and the end point is the peak)
        PathNode startNode1 = NodeData[startEndPos[0]];
        
        PathNode endNode1 = NodeData[startEndPos[1]];
        
        PathNode start = startNode1;
        
        start.m_gCost = 0;
        start.m_hCost = Vector3.Distance(start.m_position, endNode1.m_position);
        start.m_parent = null;
        openNodes.Add(start);

        int loops = 0;

        while (openNodes.Count > 0)
        {
            loops++;
            //to avoid a length and smallest before setting check gscore of current, by the end
            //the smallest will be the current
            PathNode currentNode = openNodes.RemoveFirst();
            closedNodes.Add(currentNode);

            if (currentNode == endNode1)
            {
                //We found the end node pathfinding is done
                List<Vector3> path = new List<Vector3>();
                //add the current/end node so that its just the parents being added in the loop
                path.Add(currentNode.m_position);
                while (currentNode.m_parent != null)
                {
                    path.Add(new Vector3(
                        currentNode.m_parent.m_position.x,
                        currentNode.m_parent.m_position.y,
                        currentNode.m_parent.m_position.z));
                    currentNode = currentNode.m_parent;
                }
                pathResult = new NativeArray<Vector3>(path.Count, Allocator.Temp);
                for (int i = 0; i < path.Count; i++)
                {
                    pathResult[i] = path[i];
                }
                return;
            }
            
            //check neighbours for if its in the closed (already checked) and if its already in the open list
            foreach (Edge connection in currentNode.connections)
            {
                if (connection == null || connection.to == -1 || closedNodes.Contains(NodeData[connection.to]))
                    continue;
                
                bool isOpen = false;
                for (int i = 0; i < openNodes.Count; i++)
                {
                    if (openNodes.items[i] == NodeData[connection.to])
                    {
                        isOpen = true;
                        break;
                    }
                }

                float newCostToNeighbour = currentNode.m_gCost +
                                           (Vector3.Distance(currentNode.m_position,
                                               NodeData[connection.to].m_position));

                if (newCostToNeighbour < NodeData[connection.to].m_gCost || !isOpen)
                {
                    NodeData[connection.to].m_gCost = newCostToNeighbour;
                    NodeData[connection.to].m_hCost =
                        Vector3.Distance(NodeData[connection.to].m_position, endNode1.m_position);
                    NodeData[connection.to].m_parent = currentNode;
                    
                    if (!isOpen)
                        openNodes.Add(NodeData[connection.to]);
                }
            }
        }
        //add a end case if a path cannot be found
    }
}



