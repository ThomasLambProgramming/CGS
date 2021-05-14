using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Agent : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float goNextDist = 2f;
    private float actualGotoNext;
    Vector3[] path = null;
    //since its an array we need the index
    int currentIndex = 0;
    
    public void Start()
    {
        //this is so we dont do distance sqrt but the a^2 + b^2
        actualGotoNext = goNextDist * goNextDist;

        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
    }
    public void Update()
    {
        if (NodeManager.m_nodeGraph == null)
            return;

        if (path == null && NodeManager.m_nodeGraph != null)
        {
            GetNewPath();
        }
        else if (Vector3.Distance(path[currentIndex], transform.position) < goNextDist)
        {
            if (currentIndex < path.Length - 1)
            {
                currentIndex++;
                //for now snapping is ok
                transform.LookAt(path[currentIndex]);
            }
            else
                path = null;
        }
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

    }
    private void GetNewPath()
    {
        path = FindPath(transform.position, new Vector3(Random.Range(-50.0f, 50.0f), 0, Random.Range(-50.0f, 50.0f)));
        currentIndex = 0;
        transform.LookAt(path[currentIndex]);
    }
    private Vector3[] FindPath(Vector3 a_startPosition, Vector3 a_endPosition)
    {
        if (NodeManager.m_nodeGraph == null)
        {
            Debug.Log("There is no node graph! Please create one from the node window" +
                "Window/NodeGraph.");
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

        return path;
    }
}


