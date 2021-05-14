using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
public class tempAgent : MonoBehaviour
{

    //this is a backup of the line rendering for the path. just wanted it in case of whatever reason
    Vector3[] path = null;
    public static Vector3 start;
    public static Vector3 end;

    public GameObject startObj = null;
    public GameObject endObj = null;

    LineRenderer line = null;
    private Camera mainCam = null;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        mainCam = Camera.main;
        if (NodeManager.nodeScriptableObject != null && NodeManager.m_nodeGraph == null)
        {
            NodeManager.m_nodeGraph = NodeManager.nodeScriptableObject.NodeGraph;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, 9000.0f, ~0))
            {
                start = hit.point;
                Node closestNode1 = NodeManager.m_nodeGraph[NodeUtility.FindClosestNode(hit.point)];
                startObj.transform.position = closestNode1.m_position;
            }
        }
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, 9000.0f, ~0))
            {
                end = hit.point;

                if (start != null && end != null)
                {

                    path = FindPath(start, end);
                    

                    //path = NodeUtility.Pathfind(start, end);
                    line.positionCount = path.Length;
                    for (int i = 0; i < path.Length; i++)
                    {
                        line.SetPosition(i, path[i]);
                    }
                    endObj.transform.position = path[0];

                }
            }
        }

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
