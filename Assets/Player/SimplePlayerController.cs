using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{

    public float moveSpeed = 15;
    public float rotationSpeed = 2;

    private Vector3 moveDir;
    private float mouseX;
    private float mouseY;

    private void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }

    private void FixedUpdate()
    {
        var rigidbody = GetComponent<Rigidbody>();
        transform.position = transform.position + transform.TransformDirection(moveDir) * moveSpeed * Time.fixedDeltaTime * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1) * (Input.GetKey(KeyCode.RightControl) ? 10 : 1);
        
        if (Input.GetKey(KeyCode.Mouse0))
        {
            rigidbody.transform.RotateAround(transform.position, transform.up, mouseX);
            Camera.main.transform.RotateAround(transform.position, transform.right, -rotationSpeed * mouseY);
        }
    }
}
