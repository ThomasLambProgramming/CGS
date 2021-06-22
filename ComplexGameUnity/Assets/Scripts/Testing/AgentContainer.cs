using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentContainer : MonoBehaviour
{
    public int amountToSpawn = 100;
    public GameObject Agent;
    public int amountSpawned = 0;

    public float minX = 0;
    public float maxX = 100;
    public float minY = 0;
    public float maxY = 100;
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            for (int i = 0; i < amountToSpawn; i++)
                Instantiate(Agent, new Vector3(Random.Range(minX,maxX), 
                    1.2f, 
                    Random.Range(minY,maxY)), Quaternion.identity, transform);

            amountSpawned += amountToSpawn;
        }
    }
}
