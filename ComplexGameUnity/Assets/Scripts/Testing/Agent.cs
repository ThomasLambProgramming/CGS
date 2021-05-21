using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Agent : MonoBehaviour
{
    public float maxSpeed = 100f;
    public float viewDistance = 4f;
    public float moveSpeed = 10f;
    public float turnSpeed = 2f;
    public float goNextDist = 2f;
    private float actualGotoNext;
    Vector3[] path = null;
    //since its an array we need the index
    int currentIndex = 0;
    private Rigidbody rigid = null;

    public void Start()
    {
        //this is so we dont do distance sqrt but the a^2 + b^2
        actualGotoNext = goNextDist * goNextDist;
        rigid = GetComponent<Rigidbody>();
        if (rigid == null)
        {
            Debug.LogWarning("Agent does not have a rigidbody component attached");
        }
    }
    public void FixedUpdate()
    {
        if (path != null)
        {
            Vector3 targetDirection = path[currentIndex] - transform.position;
            targetDirection.y = 0;
            targetDirection = Vector3.Normalize(targetDirection);

            Quaternion desiredRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * turnSpeed);
            rigid.velocity += targetDirection * moveSpeed * Time.deltaTime;
            if (Vector3.Magnitude(rigid.velocity) > maxSpeed)
            {
                rigid.velocity = Vector3.Normalize(rigid.velocity) * maxSpeed;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, viewDistance))
            {
                if (hit.transform.CompareTag("Agent"))
                {
                    Vector3 forceToAvoid = (rigid.velocity + transform.position) - hit.transform.position;
                    forceToAvoid = Vector3.Normalize(forceToAvoid) * turnSpeed;
                    Vector3.RotateTowards(rigid.velocity, forceToAvoid, turnSpeed, 0.0f);
                }
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


