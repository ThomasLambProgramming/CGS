using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Navmeshtest : MonoBehaviour
{
    NavMeshAgent agent;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        if (agent.hasPath == false)
        {
            agent.SetDestination(new Vector3(Random.Range(-45.0f, 45.0f), 0, Random.Range(-45.0f, 45.0f)));
        }
        if (agent.remainingDistance < 3f)
        {
            agent.SetDestination(new Vector3(Random.Range(-45.0f, 45.0f), 0, Random.Range(-45.0f, 45.0f)));
        }
    }
}
