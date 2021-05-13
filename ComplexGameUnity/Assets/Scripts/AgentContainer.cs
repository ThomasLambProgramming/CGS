using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentContainer : MonoBehaviour
{
    public int amountToSpawn = 100;
    public GameObject Agent;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            for (int i = 0; i < amountToSpawn; i++)
                Instantiate(Agent, new Vector3(0, 0, 0), Quaternion.identity, transform);
        }
    }
}
