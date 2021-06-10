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
    [ReadOnly] public NativeArray<int> startEndPos;
    
    public NativeArray<Vector3> pathResult;
    public Node[] NodeData;
    
    public void Execute()
    {
        Heap<PathNode> openNodes = new Heap<PathNode>(NodeData.Length);
        HashSet<Node> closedNodes = new HashSet<Node>();

        //find the closest node from the start and finish, after the path is found there will be an additional
        //check that can be performed to see if the point can be reached after the path (eg the closest may be at the
        //start of a mountain but there are no nodes and the end point is the peak)
        Node startNode1 = NodeData[startEndPos[0]];
        Node endNode1 = NodeData[startEndPos[1]];
      
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
                if (connection == null || connection.to == -1 || closedNodes.Contains(NodeData[connection.to]))
                    continue;

                bool isOpen = false;
                for (int i = 0; i < openNodes.Count; i++)
                {
                    if (openNodes.items[i].node == NodeData[connection.to])
                    {
                        isOpen = true;
                        break;
                    }
                }
                //UPDATE THIS TO RECALULATE THE CONNECTION COST IT COULD BE LOWER REEEE
                if (isOpen)
                    continue;

                PathNode node = new PathNode(NodeData[connection.to], currentNode);
                node.m_gCost = Vector3.Distance(node.node.m_position, currentNode.node.m_position) + currentNode.m_gCost;

                //float distanceToConnection = currentNode.m_gCost + Vector3.Distance(currentNode.node.m_position, NodeManager.m_nodeGraph[connection.to].m_position);
                node.m_hCost = Vector3.Distance(node.node.m_position, endNode1.m_position);
                openNodes.Add(node);

            }
        }
        //add a end case if a path cannot be found
    }
}



