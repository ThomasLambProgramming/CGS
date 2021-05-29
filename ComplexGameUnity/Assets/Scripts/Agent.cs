using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Agent : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float viewDistance = 4f;
    public float goNextDist = 2f;
    private Vector3 velocity = Vector3.zero;
    
    Vector3[] path = null;
    //since its an array we need the index
    int currentIndex = 0;
    
    public void FixedUpdate()
    {
        if (path != null)
        {
            velocity = Vector3.Normalize(path[currentIndex] - transform.position);
            velocity.y = 0f;
            RaycastHit rayHit;
            Debug.DrawLine(transform.position, transform.position + velocity * viewDistance, Color.blue);
            if (Physics.Raycast(transform.position, velocity, out rayHit, viewDistance))
            {
                if (rayHit.transform.CompareTag("Agent") || rayHit.transform.CompareTag("Obstacle"))
                {
                   
                }
            }
            
            transform.position += velocity * (Time.deltaTime * moveSpeed);
            
            
            if (Vector3.Distance(path[currentIndex], transform.position) < goNextDist)
            {
                if (currentIndex < path.Length - 1)
                    currentIndex++;
                else
                    path = null;
            }
        }
    }
    public void Update()
    {
        if (path == null && NodeManager.m_nodeGraph != null)
        {
            GetNewPath();
        }

    }
    private void GetNewPath()
    {
        path = FindPath();
        currentIndex = 0;
    }
    private Vector3[] FindPath()
    {
        if (NodeManager.m_nodeGraph == null)
        {
            Debug.LogWarning("There is no node graph! Please create one from the node window" +
                " Window/NodeGraph.");
            return null;
        }

        Vector3[] path;
        PathFindJob pathfind = new PathFindJob();
        int[] StartEndIndex = {0, 0};

        StartEndIndex[0] = NodeUtility.FindClosestNode(transform.position);
        StartEndIndex[1] = Random.Range(0, NodeManager.m_nodeGraph.Length - 1);
        while(StartEndIndex[1] == StartEndIndex[0])
        {
            StartEndIndex[1] = Random.Range(0, NodeManager.m_nodeGraph.Length - 1);
        }
        NativeArray<int> startEndPos = new NativeArray<int>(StartEndIndex, Allocator.Temp);
        pathfind.startEndPos = startEndPos;

        //test for path finding time will need to improve the memory grabbing though its a bit slow and can be grouped
        //float time = Time.realtimeSinceStartup;
        pathfind.Execute();
        //Debug.Log(Time.realtimeSinceStartup - time);
        if (!pathfind.pathResult.IsCreated)
        {
            if (startEndPos.IsCreated)
                startEndPos.Dispose();
            Debug.Log("Pathresult was null");
            return null;
        }
        path = pathfind.pathResult.ToArray();
        pathfind.pathResult.Dispose();
        startEndPos.Dispose();

        //the path that the main loop was giving back was in the reverse order
        Vector3[] reversedPath = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            reversedPath[i] = path[path.Length - (1 + i)];
        }
        
        return reversedPath;
    }
}


