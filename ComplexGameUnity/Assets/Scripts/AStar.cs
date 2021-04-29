using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Im making this a information system - eg the ai will ask the system to find the nodes 
//and it will return the needed node paths (i think giving it a vector array or referencing the static is the best option here
public class AStar : MonoBehaviour
{
    class PathNode
    {
        public float m_totalCost = 0;
        public float m_gCost = 0;
        public float m_hCost = 0;
        public float m_fCost {get { return m_gCost + m_hCost; } }
        public Node m_node = null;
        public PathNode m_parent = null;

        public PathNode(Node a_node, PathNode a_parent)
        {
            m_node = a_node;
            m_parent = a_parent;
        }
    }
    public static Node FindClosestNode(Vector3 a_position)
    {
        if (NodeManager.m_nodeGraph == null)
            return null;

        Node closestNode = new Node(0, new Vector3(), new Vector3());
        float distance = 1000000;
        foreach (Node node in NodeManager.m_nodeGraph)
        {
            float nodeDist = Vector3.Distance(node.m_position, a_position);
            if (nodeDist < distance)
            {
                distance = nodeDist;
                closestNode = node;
            }
        }
        return closestNode;
    }
    public static List<Vector3> Pathfind(Vector3 a_startPos, Vector3 a_endPos)
    {
        List<PathNode> openNodes = new List<PathNode>();
        List<PathNode> closedNodes = new List<PathNode>();
        
        //find the closest node from the start and finish, after the path is found there will be an additional
        //check that can be performed to see if the point can be reached after the path (eg the closest may be at the
        //start of a mountain but there are no nodes and the end point is the peak)
        Node startNode = FindClosestNode(a_startPos);
        Node endNode = FindClosestNode(a_endPos);

        //giving it null as it is the starting node
        PathNode start = new PathNode(startNode, null);
        start.m_gCost = 0;
        start.m_hCost = Vector3.Distance(start.m_node.m_position, endNode.m_position);
        openNodes.Add(start);

       
        while(openNodes.Count > 0)
        {
            //to avoid a length and smallest before setting check gscore of current, by the end
            //the smallest will be the current
            PathNode currentNode = openNodes[0];
            for(int i = 0; i < openNodes.Count; i++)
                if (openNodes[i].m_fCost < currentNode.m_fCost || (openNodes[i].m_fCost == currentNode.m_fCost && openNodes[i].m_hCost < currentNode.m_hCost))
                    currentNode = openNodes[i];


            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode.m_node == endNode)
            {
                //We found the end node pathfinding is done
                List<Vector3> path = new List<Vector3>();
                //add the current/end node so that its just the parents being added in the loop
                path.Add(currentNode.m_node.m_position);
                while (currentNode.m_parent != null)
                {
                    path.Add(new Vector3(
                        currentNode.m_parent.m_node.m_position.x,
                        currentNode.m_parent.m_node.m_position.y,
                        currentNode.m_parent.m_node.m_position.z));
                    currentNode = currentNode.m_parent;
                }
                return path;
            }

            foreach(Node connection in currentNode.m_node.m_connectedNodes)
            {
                if (connection == null)
                    continue;
                //we need a travelable setting (nevermind this was a layermask for obsticles)

                //this is a check to see if the node is in the closed or open list
                bool isClosedNode = false;
                foreach (PathNode closed in closedNodes)
                {
                    if (closed.m_node == connection)
                    { 
                        isClosedNode = true;
                        break;
                    } 
                }
                if (isClosedNode)
                    continue;

                bool isOpen = false;
                foreach (PathNode open in openNodes)
                {
                    if (open.m_node == connection)
                    {
                        isOpen = true;
                        break;
                    }
                }
                if (isOpen)
                    continue;

                PathNode node = new PathNode(connection, currentNode);
                node.m_gCost = Vector3.Distance(node.m_node.m_position, currentNode.m_node.m_position) + currentNode.m_gCost;

                float distanceToConnection = currentNode.m_gCost + Vector3.Distance(currentNode.m_node.m_position, connection.m_position);
                node.m_hCost = Vector3.Distance(node.m_node.m_position, endNode.m_position);
                openNodes.Add(node);

            }
            //foreach connection to the current node DDDDD
            //  if the connection is not travelable or it is in closed DDDD
            //    skip to the next connection DDDDDDD

            // so from the looks of things each node carries the distance (fcost) and 
            // when it is checking connections it will also update to see if node to node is shorter than
            // that nodes previous parent path thing
            //
            //  if new path to connection is shorter OR connection is not in open
            //    set fcost of connection
            //    set connections parent to be currentNode
            //    if connection is not in open
            //      add it to open
            
        }
        //if it gets to this then we have failed
        return null;
    }
}
