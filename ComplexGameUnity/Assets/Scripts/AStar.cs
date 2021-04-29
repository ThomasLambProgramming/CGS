using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Im making this a information system - eg the ai will ask the system to find the nodes 
//and it will return the needed node paths (i think giving it a vector array or referencing the static is the best option here
public class AStar : MonoBehaviour
{
    class PathNode
    {
        float m_startDistance = 0;
        float m_endDistance = 0;
        float m_fCost {get { return m_startDistance + m_endDistance; } }
        Node m_node = null;
        PathNode m_parent = null;

        PathNode(Node a_node, PathNode a_parent)
        {
            m_node = a_node;
            m_parent = a_parent;
        }
    }
    static Node FindClosestNode(Vector3 a_position)
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
    public static void Pathfind(Vector3 a_startPos, Vector3 a_endPos)
    {
        List<Node> openNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();
        
        Node startNode = FindClosestNode(a_startPos);
        Node endNode = FindClosestNode(a_endPos);

        openNodes.Add(startNode);

        bool isSearching = true;
        while(isSearching)
        {
            foreach(var node in openNodes)
            {
                //find the one with the lowest f cost
            }
            //current node = lowest one found
            Node currentNode = null;
            //openNodes.Remove(currentNode);
            //closedNodes.Add(currentNode);
            if (currentNode == endNode)
            {
                //We found the end node pathfinding is done
            }

            //foreach connection to the current node
            //  if the connection is not travelable or it is in closed
            //    skip to the next connection

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
    }
}

/*
 * for the nodes we need
 * distance from start node
 * distance from end node
 * f cost = startdis + enddis
 * parent (pathfind)node
 * actual node itself
 */
