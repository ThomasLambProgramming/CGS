﻿using System.Collections;
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
    public bool debugMode = false;
    public bool avoidance = false;
    public NodeContainer pathData = null;

    public int walkableLayer = 1 << 23;
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
            if (debugMode)
            {
                Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
                for (int i = 0; i < path.Length; i++)
                {
                    if (i == 0)
                    {
                        Debug.DrawLine(transform.position, path[currentIndex], Color.blue);
                    }
                    else if (i > currentIndex)
                    {
                        Debug.DrawLine(path[i - 1], path[i], Color.blue);
                    }
                }
            }

            Vector3 direction = path[currentIndex] - transform.position;
            direction.y = 0;
            direction.Normalize();
            Vector3 yCheckDirection = new Vector3(direction.x, -2, direction.z);
            direction *= moveSpeed;

            rb.velocity += direction;
            
            RaycastHit yCheck;
            if (Physics.Raycast(transform.position, yCheckDirection, out yCheck, 5))
            {
                if (yCheck.transform.CompareTag("Ground"))
                {
                    transform.position = new Vector3(transform.position.x, yCheck.point.y + 1.2f, transform.position.z);
                }
            }
            if (avoidance)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, rb.velocity.normalized, out hit, seeAheadDistance))
                {
                    if (hit.transform.CompareTag("Agent"))
                    {
                        Rigidbody otherAgent = hit.rigidbody;
                        //if the dot of their velocities are almost opposite then we want to avoid
                        if (Vector3.Dot(otherAgent.velocity.normalized, rb.velocity.normalized) < -0.7f)
                        {
                            Vector3 avoidForce = new Vector3(-rb.velocity.z, 0, rb.velocity.x);
                            avoidForce = Vector3.Normalize(avoidForce) * (maxAvoidForce);

                            rb.velocity += avoidForce;
                        }
                    }
                }
            }

            float velMag = rb.velocity.magnitude;
            if (velMag > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }

            if (Vector3.SqrMagnitude(path[currentIndex] - transform.position) < goNextDist)
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
        if (path == null && pathData.NodeGraph != null)
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
        if (pathData.NodeGraph == null)
        {
            Debug.LogWarning("There is no node graph! Please create one from the node window" +
                             " Window/NodeGraph.");
            return null;
        }

        Vector3[] path;
        PathFindJob pathfind = new PathFindJob();
        pathfind.NodeData = pathData.NodeGraph;

        int[] StartEndIndex = {0, 0};

        StartEndIndex[0] = FindClosestNode(transform.position);
        StartEndIndex[1] = Random.Range(0, pathData.NodeGraph.Length - 1);
        while (StartEndIndex[1] == StartEndIndex[0])
        {
            StartEndIndex[1] = Random.Range(0, pathData.NodeGraph.Length - 1);
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

    public int FindClosestNode(Vector3 a_position)
    {
        if (pathData.NodeGraph == null)
            return -1;

        int closestNode = -1;
        float distance = 1000000;

        for (int i = 0; i < pathData.NodeGraph.Length; i++)
        {
            float nodeDist = Vector3.Magnitude(pathData.NodeGraph[i].m_position - a_position);
            if (nodeDist < distance)
            {
                distance = nodeDist;
                closestNode = i;
            }
        }

        return closestNode;
    }
}