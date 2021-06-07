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
                Vector3 ahead = transform.position + rb.velocity.normalized * seeAheadDistance;
                Vector3 avoidForce = ahead - hit.transform.position;
                if (!hasBeenAdjusted)
                {
                    avoidForce.x += 2.0f;
                    Test temp = hit.transform.GetComponent<Test>();
                    temp.hasBeenAdjusted = true;
                }
                else
                    avoidForce.x -= 2.0f;
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
