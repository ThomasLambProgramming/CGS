using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed;
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-transform.right * (moveSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(transform.right * (moveSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(transform.forward * (moveSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(-transform.forward * (moveSpeed * Time.deltaTime));
        }

        float mouseAmountX = Input.GetAxis("Mouse X");
        float mouseAmountY = Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(-mouseAmountY, mouseAmountX, 0));
    }
}
