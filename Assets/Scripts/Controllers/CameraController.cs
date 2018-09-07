using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float crouchCamHeight;
    public float crouchCamDepth;

    public Vector3 cameraWRLeft;
    public Vector3 cameraWRRight;

    public float jumpOffset;

    private Vector3 originalLocalPosition;
    private Vector3 originalCrouchLocalPosition;

    private bool crouched = false;
    private bool aimed = false;
    private bool wallRan = false;
    private bool jumped = false;

    private PlayerBodyData playerBodyData;
    private InputState inputState;

    void Start ()
    {
        originalLocalPosition = transform.localPosition;
        originalCrouchLocalPosition = new Vector3(transform.localPosition.x, crouchCamHeight, crouchCamDepth);
        //Retrieve the player body data from our parent's parent (which is our player gameobject)
        GameObject player = transform.parent.parent.parent.gameObject;
        playerBodyData = player.GetComponent<BodyController>().PlayerBodyData;
        inputState = player.GetComponent<InputState>();
    }
	
	void Update ()
    {
        HandleAirborne();
        HandleWallRunning();
	}

    private void HandleAirborne()
    {
        if (!inputState.playerIsGrounded && !inputState.playerIsWallRunningBack 
            && !inputState.playerIsWallRunningLeft && !inputState.playerIsWallRunningRight)
        {
            transform.localPosition = new Vector3(originalLocalPosition.x, originalLocalPosition.y + jumpOffset, originalLocalPosition.z);
            jumped = true;
        }
        else if (inputState.playerIsGrounded && jumped)
        {
            transform.localPosition = originalLocalPosition;
            jumped = false;
        }
    }

    private void HandleWallRunning()
    {
        if (inputState.playerIsWallRunningLeft && !wallRan)
        {
            transform.localPosition = cameraWRLeft;
            wallRan = true;
        }
        else if (inputState.playerIsWallRunningRight && !wallRan)
        {
            transform.localPosition = cameraWRRight;
            wallRan = true;
        }
        else if (!inputState.playerIsWallRunningLeft && !inputState.playerIsWallRunningRight && wallRan)
        {
            transform.localPosition = originalLocalPosition;
            wallRan = false;
        }
    }
}
