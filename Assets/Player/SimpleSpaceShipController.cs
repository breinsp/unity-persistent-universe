using Assets.StarSystem.Generation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpaceShipController : MonoBehaviour
{
    public SystemController controller;

    public float velocity;
    public float moveSpeed = 2;
    public float rollSpeed = 3;
    public float sensitivity = 1f;

    [Range(-1, 1)]
    public float pitch;
    [Range(-1, 1)]
    public float yaw;
    [Range(-1, 1)]
    public float roll;
    [Range(-1, 1)]
    public float throttle;

    public float deadzone = 0.05f;

    private void FixedUpdate()
    {
        var rigidbody = GetComponent<Rigidbody>();
        Vector3 mousePos = Input.mousePosition;

        pitch = (mousePos.y - (Screen.height * 0.5f)) / (Screen.height * 0.5f);
        yaw = (mousePos.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f);
        roll = -Input.GetAxis("Horizontal");
        throttle = Input.GetAxis("Vertical");

        pitch = -Mathf.Clamp(pitch, -1.0f, 1.0f);
        yaw = Mathf.Clamp(yaw, -1.0f, 1.0f);
        roll = Mathf.Clamp(roll, -1.0f, 1.0f);
        throttle = Mathf.Clamp(throttle, -1.0f, 1.0f);

        pitch = FixInput(pitch);
        yaw = FixInput(yaw);
        roll = FixInput(roll);
        throttle = FixInput(throttle);

        float speed = moveSpeed;
        float boost = Input.GetKey(KeyCode.LeftShift) && throttle > 0 ? (controller.zoomed != null ? 5 : 3000) : 1;
        if (throttle < 0)
        {
            speed *= 0.2f;
        }
        speed *= controller.zoomed != null ? controller.planetScaleMultiplier : 1;

        Vector3 move = new Vector3(0, 0, throttle * speed * boost);
        Vector3 rot = new Vector3(pitch, yaw, roll * rollSpeed);

        rigidbody.AddRelativeForce(move, ForceMode.Force);
        rigidbody.AddRelativeTorque(rot * 100 * sensitivity / (boost == 1000 ? 5 : 1), ForceMode.Force);
        velocity = rigidbody.velocity.magnitude;

        float speedFactor = Mathf.Clamp(velocity / 200f, 0, 1);
    }

    private float FixInput(float value)
    {
        if (Mathf.Abs(value) < deadzone)
        {
            if (value > 0)
            {
                return Mathf.Clamp(value - deadzone, 0, 1);
            }
            else
            {
                return Mathf.Clamp(value + deadzone, -1, 0);
            }
        }
        return value;
    }
}