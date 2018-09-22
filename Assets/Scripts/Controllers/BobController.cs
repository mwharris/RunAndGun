using UnityEngine;
using System.Collections;

public class BobController : MonoBehaviour
{
    private Vector3 origRestPosition;
    private Quaternion origRestRotation;

    public Vector3 restPosition;
    private Quaternion restRotation;

    public Vector3 sprintPosition;

    public Vector3 aimPosition;
    public Vector3 aimRotation;

    public Vector3 crouchPosition;
    public Vector3 crouchRotation;

    public float transitionSpeed = 20f;
    public float walkBobSpeed = 4.8f;
    public float walkBobAmount = 0.1f;
    public float sprintBobSpeed = 12f;
    public float sprintBobAmount = 0.2f;

    public bool isCamera = false;
    public InputState inputState;

    //Initialized as this value because this is where sin = 1. 
    //So, this will make the camera always start at the crest of the sin wave, simulating someone picking up their foot and starting to walk.
    //You experience a bob upwards when you start walking as your foot pushes off the ground, the left and right bobs come as you walk.
    private float timer = Mathf.PI / 2;

    private bool aiming = false;
    private bool sprinting = false;
    private Vector3 lerpToPos;
    private Quaternion lerpToRot; 

    void Start()
    {
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;
        origRestPosition = restPosition;
        origRestRotation = restRotation;
    }

    private bool reset = false;

    void Update()
	{
        //Determine if we need to reset our Rest Positions based on player state.
        SetRestPositionAndRotation();
        //Call functions appropriate to the above function call
        if (reset)
        {
            HandleResetting();
        }
        else
        {
            HandleBob();
            HandleTilt();
        }
        //Completed a full cycle. Reset to 0 to avoid bloated values.
        if (timer > Mathf.PI * 2)
        {
            timer = 0;
        }
	}

    //Lerp to a new resting position reset out cycle
    private void HandleResetting()
    {
        //Lerp to the new resting position
        transform.localPosition = Vector3.Lerp(transform.localPosition, lerpToPos, 20f * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, lerpToRot, 20f * Time.deltaTime);
        //Snap to the new value once we get within a certain range
        if (LerpComplete(transform.localPosition, lerpToPos))
        {
            reset = false;
            transform.localPosition = lerpToPos;
            transform.localRotation = lerpToRot;
            //Reset our bob cycle
            timer = Mathf.PI / 2;
        }
    }

    //Handle bobbing the camera or body left/right while moving
    private void HandleBob()
    {
        //While we are moving apply a head/body bob
        if (!inputState.playerIsAiming && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            //Determine bob speed + amount depending on player movement state
            float bobSpeed = walkBobSpeed;
            float bobAmount = walkBobAmount;
            if (inputState.playerIsSprinting)
            {
                bobSpeed = sprintBobSpeed;
                bobAmount = sprintBobAmount;
            }
            else if (inputState.playerIsCrouching || inputState.playerIsAiming)
            {
                bobSpeed = walkBobSpeed / 1.5f;
                bobAmount = walkBobAmount / 1.5f;
            }
            //Increase / decrease bobSpeed based on our controller input
            float inputSpeed = Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical"));
            float inputSpeedClamped = Mathf.Clamp(inputSpeed, -1f, 1f);
            timer += bobSpeed * Time.deltaTime * inputSpeedClamped;
            //Bounce the position left/right in a cycle according to the timer
            transform.localPosition = new Vector3(restPosition.x + Mathf.Cos(timer) * bobAmount, restPosition.y + Mathf.Abs((Mathf.Sin(timer) * bobAmount)), restPosition.z);
        }
        else
        {
            //Reset the bob cycle
            timer = Mathf.PI / 2;
            if (restPosition != transform.localPosition && crouchPosition != transform.localPosition)
            {
                Vector3 newPosition = new Vector3(
                    Mathf.Lerp(transform.localPosition.x, restPosition.x, transitionSpeed * Time.deltaTime),
                    Mathf.Lerp(transform.localPosition.y, restPosition.y, transitionSpeed * Time.deltaTime),
                    Mathf.Lerp(transform.localPosition.z, restPosition.z, transitionSpeed * Time.deltaTime)
                );
                transform.localPosition = newPosition;
            }
        }
    }

    //Handle tilting the body when moving horizontally
    private void HandleTilt()
    {
        if (!isCamera)
        {
            //If we're moving Horizontally at all, apply a tilt to the hands
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                Vector3 bueler = transform.localRotation.eulerAngles;
                float tiltAngle = Input.GetAxisRaw("Horizontal") * -5f;
                if (inputState.playerIsAiming)
                {
                    tiltAngle = Input.GetAxisRaw("Horizontal") * -6f;
                }
                Quaternion q = Quaternion.Euler(bueler.x, bueler.y, tiltAngle);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, q, 4f * Time.deltaTime);
            }
            //Otherwise keep us in the resting position
            else if (restRotation != transform.localRotation)
            {
                float rotLerpSpeed = 4f * Time.deltaTime;
                if (inputState.playerIsAiming)
                {
                    rotLerpSpeed = 20f * Time.deltaTime;
                }
                transform.localRotation = Quaternion.Lerp(transform.localRotation, restRotation, rotLerpSpeed);
            }
        }
    }

    //Determine if we need to change the resting position of our cycle
    private void SetRestPositionAndRotation()
    {
        //Only change positions if we're controlling the body
        if (!isCamera)
        {
            //When aiming the resting position centers on the screen
            if (inputState.playerIsAiming)
            {
                restPosition = aimPosition;
                restRotation = Quaternion.Euler(aimRotation);
                if (!aiming)
                {
                    aiming = true;
                    sprinting = false;
                    reset = true;
                    lerpToPos = aimPosition;
                    lerpToRot = Quaternion.Euler(aimRotation);
                }
            }
            else if (inputState.playerIsSprinting)
            {
                restPosition = sprintPosition;
                restRotation = origRestRotation;
                if (!sprinting)
                {
                    sprinting = true;
                    aiming = false;
                    reset = true;
                    lerpToPos = sprintPosition;
                    lerpToRot = origRestRotation;
                }
            }
            //Otherwise keep us in the hip-aimed location
            else
            {
                restPosition = origRestPosition;
                restRotation = origRestRotation;
                if (aiming || sprinting)
                {
                    aiming = false;
                    sprinting = false;
                    reset = true;
                    lerpToPos = origRestPosition;
                    lerpToRot = origRestRotation;
                }
            }
        }
    }

    private bool LerpComplete(Vector3 startPos, Vector3 endPos)
    {
        if (Mathf.Abs(endPos.x - startPos.x) <= 0.001f
            && Mathf.Abs(endPos.y - startPos.y) <= 0.001f
            && Mathf.Abs(endPos.z - startPos.z) <= 0.001f)
        {
            Debug.Log("SNAP!");
            return true;
        }
        return false;
    }

}