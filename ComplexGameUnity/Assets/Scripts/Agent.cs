using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Agent : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float viewDistance = 4f;
    public float turnSpeed = 2f;
    public float goNextDist = 2f;
    private Vector3 velocity = Vector3.zero;
    private float actualGotoNext;
   
    Vector3[] path = null;
    //since its an array we need the index
    int currentIndex = 0;
  

    public void Start()
    {
        //this is so we dont do distance sqrt but the a^2 + b^2
        actualGotoNext = goNextDist * goNextDist;
    }
    public void FixedUpdate()
    {
        if (path != null)
        {
            bool hasAvoided = false;
            RaycastHit rayHit;
            if (Physics.Raycast(transform.position, Vector3.Normalize(velocity), out rayHit, viewDistance))
            {
                if (rayHit.transform.CompareTag("Agent") || rayHit.transform.CompareTag("Obstacle"))
                {
                    hasAvoided = true;
                    
                }
            }
            else
            {
                
            }
            if (Vector3.Magnitude(path[currentIndex] - transform.position) < actualGotoNext)
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
        path = FindPath(transform.position, new Vector3(Random.Range(-50.0f, 50.0f), 0, Random.Range(-50.0f, 50.0f)));
        currentIndex = 0;
    }
    private Vector3[] FindPath(Vector3 a_startPosition, Vector3 a_endPosition)
    {
        if (NodeManager.m_nodeGraph == null)
        {
            Debug.LogWarning("There is no node graph! Please create one from the node window" +
                " Window/NodeGraph.");
            return null;
        }

        Vector3[] path;
        PathFindJob pathfind = new PathFindJob();
        Vector3[] tempVectors = { a_startPosition, a_endPosition };
        NativeArray<Vector3> startEndPos = new NativeArray<Vector3>(tempVectors, Allocator.Temp);
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


