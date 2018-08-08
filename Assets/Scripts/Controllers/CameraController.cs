using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform weaponHolder;
    public InputState inputState;

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

    void Start ()
    {
        originalLocalPosition = transform.localPosition;
        originalCrouchLocalPosition = new Vector3(transform.localPosition.x, crouchCamHeight, crouchCamDepth);
    }
	
	void Update ()
    {
        HandleAirborne();
        HandleAiming();
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

    private void HandleAiming()
    {
        if (inputState.playerIsAiming)
        {
            transform.position = weaponHolder.position;
            aimed = true;
        }
        else if (!inputState.playerIsAiming && aimed)
        {
            Vector3 target = inputState.playerIsCrouching ? originalCrouchLocalPosition : originalLocalPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * 8f);
            if (transform.localPosition == originalLocalPosition)
            {
                aimed = false;
            }
        }
    }
}
