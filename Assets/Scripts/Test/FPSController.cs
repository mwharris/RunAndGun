using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    public float speed = 1f;
    public float sprintSpeed = 1f;
    private CharacterController cc;
    private Vector3 velocity = new Vector3(0,0,0);
    private float forwardSpeed;
    private float sideSpeed;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        //Get movement speed in particular direction
        forwardSpeed = Input.GetAxis("Vertical");
        sideSpeed = Input.GetAxis("Horizontal");
        //Increase our speed if we are sprinting
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            forwardSpeed *= speed * 1.2f;
            sideSpeed *= speed * 1.2f;
        }
        else
        {
            forwardSpeed *= speed;
            sideSpeed *= speed;
        }
        //Update velocity with our horizontal and vertical movement
        velocity += forwardSpeed * transform.forward;
        velocity += sideSpeed * transform.right;
        //Stop if no movement buttons being held
        if (forwardSpeed == 0 && sideSpeed == 0)
        {
            velocity = new Vector3(0, 0, 0);
        }
        //Add linear drag on the x and z axis
        if (cc.isGrounded)
        {
            velocity.x *= 0.9f;
            velocity.z *= 0.9f;
        }
        //Call the Character Controller to move us
        Debug.Log(velocity);
        //cc.Move(velocity * Time.deltaTime);
    }
}
