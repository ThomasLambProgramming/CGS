using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Agent : MonoBehaviour
{
    public GameObject pathpos = null;
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
        path = AgentUtility.GetRandomPath(transform.position);
        currentIndex = 0;
        transform.LookAt(path[currentIndex]);
    }
}
