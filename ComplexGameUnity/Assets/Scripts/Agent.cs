using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    List<Vector3> path = new List<Vector3>();
    private Vector3 start;
    private Vector3 end;

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
                Node closestNode1 = AStar.FindClosestNode(hit.point);
                startObj.transform.position = closestNode1.m_position;
            }  
        }
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, 9000.0f, ~0))
            {
                end = hit.point;
                Node closestNode1 = AStar.FindClosestNode(hit.point);
                endObj.transform.position = closestNode1.m_position;
                path = AStar.Pathfind(start, end);
                line.positionCount = path.Count;
                for (int i = 0; i < path.Count; i++)
                {
                    line.SetPosition(i, path[i]);
                }
            }
        }
        
    }
}
