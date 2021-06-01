using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Vector3 velocity;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb = null;
    public Vector3 targetPosition;
    public float maxMoveSpeed = 10f;
    public float moveSpeed = 10f;
    public float turnSpeed = 10f;
    private bool avoiding = false;
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var transform1 = transform;
            transform1.position = startPosition;
            transform1.rotation = startRotation;
            rb.velocity = Vector3.zero;
        }
        
        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();
        direction *= moveSpeed;
        
        




    }
}
