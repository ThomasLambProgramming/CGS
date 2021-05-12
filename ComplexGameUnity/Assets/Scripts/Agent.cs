using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Agent : MonoBehaviour
{
    Vector3[] path = null;
    public static Vector3 start;
    public static Vector3 end;

    public GameObject startObj = null;
    public GameObject endObj = null;

    LineRenderer line = null;
    public Camera mainCam = null;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if(Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, 9000.0f, ~0))
            {
                start = hit.point;
                Node closestNode1 = NodeManager.m_nodeGraph[NodeUtility.FindClosestNode(hit.point)];
                startObj.transform.position = closestNode1.m_position;
            }  
        }
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, 9000.0f, ~0))
            {
                end = hit.point;

                PathFindJob pathfind = new PathFindJob();
                Vector3[] tempVectors = { start, end };
                NativeArray<Vector3> startEndPos = new NativeArray<Vector3>(tempVectors, Allocator.TempJob);
                pathfind.startEndPos = startEndPos;

                pathfind.Execute();

                path = new Vector3[pathfind.pathResult.Length];
                pathfind.pathResult.CopyTo(path);

                pathfind.pathResult.Dispose();
                startEndPos.Dispose();
                
                
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
