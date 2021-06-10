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
    public float seeAheadDistance = 4;
    public float maxMoveSpeed = 10f;
    public float moveSpeed = 10f;
    public float turnSpeed = 10f;
    public float maxAvoidForce = 10f;
    public bool hasBeenAdjusted = false;
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
       
        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();
        direction *= (moveSpeed * Time.deltaTime);

        rb.velocity += direction;
        Debug.DrawLine(transform.position, rb.velocity, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rb.velocity.normalized, out hit, seeAheadDistance))
        {
            if (hit.transform.CompareTag("Agent"))
            {
                
                Vector3 avoidForce = new Vector3(rb.velocity.z, 0, rb.velocity.x);
                avoidForce = Vector3.Normalize(avoidForce) * maxAvoidForce;

                rb.velocity += avoidForce;
            }
        }
        else
            hasBeenAdjusted = false;

        float velMag = rb.velocity.magnitude;
        if (velMag > maxMoveSpeed)
        {
            float overAmount = velMag - maxMoveSpeed;
            rb.velocity += -rb.velocity.normalized * overAmount;
        }



    }
}
