using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Agent : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float stoppingSpeed = 5f;
    public float goNextDist = 2f;
    Vector3[] path = null;
    public float seeAheadDistance = 4;
    public float maxAvoidForce = 2f;

    //since its an array we need the index
    int currentIndex = 0;
    public bool hasBeenAdjusted = false;
    public Rigidbody rb = null;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void FixedUpdate()
    {
        if (path != null)
        {

            Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
            for (int i = 0; i < path.Length; i++)
            {
                if (i == 0)
                {
                    Debug.DrawLine(transform.position, path[currentIndex], Color.blue);
                }
                else
                {
                    Debug.DrawLine(path[i - 1], path[i], Color.blue);
                }
            }
            Vector3 direction = path[currentIndex] - transform.position;
            direction.y = transform.position.y;
            direction.Normalize();
            direction *= moveSpeed;

            rb.velocity += direction;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, rb.velocity.normalized, out hit, seeAheadDistance))
            {
                if (hit.transform.CompareTag("Agent"))
                {
                    Agent hitAgent = hit.transform.GetComponent<Agent>();
                    //if the two agents velocities are too different then apply the change
                    if (Vector3.Dot(hitAgent.rb.velocity, rb.velocity) < 0.8f)
                    {
                        Vector3 ahead = transform.position + rb.velocity.normalized * seeAheadDistance;
                        Vector3 avoidForce = ahead - hit.transform.position;
                        if (!hasBeenAdjusted)
                        {
                            avoidForce.x += 2.0f;
                            hitAgent.hasBeenAdjusted = true;
                        }
                        else
                            avoidForce.x -= 2.0f;
                        avoidForce = Vector3.Normalize(avoidForce) * maxAvoidForce;

                        rb.velocity += avoidForce;
                    }
                }
            }
            float velMag = rb.velocity.magnitude;
            if (velMag > moveSpeed)
            {
                float overAmount = velMag - moveSpeed;
                rb.velocity += -rb.velocity.normalized * overAmount;
            }
            
            if (Vector3.Distance(path[currentIndex], transform.position) < goNextDist)
            {
                if (currentIndex < path.Length - 1)
                    currentIndex++;
                else
                {
                    path = null;
                    currentIndex = 0;
                }
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
        int[] StartEndIndex = { 0, 0 };

        StartEndIndex[0] = NodeUtility.FindClosestNode(transform.position);
        StartEndIndex[1] = Random.Range(0, NodeManager.m_nodeGraph.Length - 1);
        while (StartEndIndex[1] == StartEndIndex[0])
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


