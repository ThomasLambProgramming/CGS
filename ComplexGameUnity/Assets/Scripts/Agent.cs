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

                    path = AgentUtility.FindPath(start, end);


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
}
//make an abstract class for agents to be able to use
//public class PathFind : MonoBehaviour
//{
//    void 
//}
