using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
   
    

    Vector3 PreviousPosition = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        if (Input.GetKey(KeyCode.Space))
        {
            
            PreviousPosition = transform.position;
            gameObject.transform.position = NodeManager.m_nodeGraph[Random.Range(0, NodeManager.m_nodeGraph.Length)].m_position;
            AStar.Pathfind(PreviousPosition, transform.position);
        }
    }
}
