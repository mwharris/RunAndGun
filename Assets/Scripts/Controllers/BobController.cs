using UnityEngine;
using System.Collections;

public class BobController : AbstractBehavior
{
    public bool isCamera = false;

    private AnimationPosInfo origRestPosInfo;
    [SerializeField] private AnimationPosInfo restPosInfo;
    [SerializeField] private AnimationPosInfo sprintPosInfo;
    [SerializeField] private AnimationPosInfo shAimPosInfo;
    [SerializeField] private AnimationPosInfo dhAimPosInfo;
    [SerializeField] private AnimationPosInfo crouchPosInfo;

    [HideInInspector] public float transitionSpeed = 20f;
    [HideInInspector] public float walkBobSpeed = 7.8f;
    [HideInInspector] public float walkBobAmount = 0.01f;
    [HideInInspector] public float sprintBobSpeed = 12f;
    [HideInInspector] public float sprintBobAmount = 0.03f;

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
        restPosInfo.localPos = transform.localPosition;
        restPosInfo.rotation = transform.localRotation;
        origRestPosInfo = new AnimationPosInfo();
        origRestPosInfo.localPos = restPosInfo.localPos;
        origRestPosInfo.rotation = restPosInfo.rotation;
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

    //Determine if we need to change the resting position of our cycle
    private void SetRestPositionAndRotation()
    {
        //Only change positions if we're controlling the body
        if (!isCamera)
        {
            //When aiming the resting position centers on the screen
            if (inputState.playerIsAiming)
            {
                restPosInfo.localPos = shAimPosInfo.localPos;
                restPosInfo.rotation = Quaternion.Euler(shAimPosInfo.localRot);
                if (!aiming)
                {
                    aiming = true;
                    sprinting = false;
                    reset = true;
                    lerpToPos = shAimPosInfo.localPos;
                    lerpToRot = Quaternion.Euler(shAimPosInfo.localRot);
                }
            }
            else if (inputState.playerIsSprinting && inputState.playerIsGrounded)
            {
                restPosInfo.localPos = sprintPosInfo.localPos;
                restPosInfo.rotation = origRestPosInfo.rotation;
                if (!sprinting)
                {
                    sprinting = true;
                    aiming = false;
                    reset = true;
                    lerpToPos = sprintPosInfo.localPos;
                    lerpToRot = origRestPosInfo.rotation;
                }
            }
            //Otherwise keep us in the hip-aimed location
            else
            {
                restPosInfo.localPos = origRestPosInfo.localPos;
                restPosInfo.rotation = origRestPosInfo.rotation;
                if (aiming || sprinting)
                {
                    aiming = false;
                    sprinting = false;
                    reset = true;
                    lerpToPos = origRestPosInfo.localPos;
                    lerpToRot = origRestPosInfo.rotation;
                }
            }
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
        if (!inputState.playerIsAiming && inputState.playerIsGrounded 
            && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
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
            transform.localPosition = new Vector3(restPosInfo.localPos.x + Mathf.Cos(timer) * bobAmount, restPosInfo.localPos.y + Mathf.Abs((Mathf.Sin(timer) * bobAmount)), restPosInfo.localPos.z);
        }
        else
        {
            //Reset the bob cycle
            timer = Mathf.PI / 2;
            if (restPosInfo.localPos != transform.localPosition && crouchPosInfo.localPos != transform.localPosition)
            {
                Vector3 newPosition = new Vector3(
                    Mathf.Lerp(transform.localPosition.x, restPosInfo.localPos.x, transitionSpeed * Time.deltaTime),
                    Mathf.Lerp(transform.localPosition.y, restPosInfo.localPos.y, transitionSpeed * Time.deltaTime),
                    Mathf.Lerp(transform.localPosition.z, restPosInfo.localPos.z, transitionSpeed * Time.deltaTime)
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
            else if (restPosInfo.rotation != transform.localRotation)
            {
                float rotLerpSpeed = 4f * Time.deltaTime;
                if (inputState.playerIsAiming)
                {
                    rotLerpSpeed = 20f * Time.deltaTime;
                }
                transform.localRotation = Quaternion.Lerp(transform.localRotation, restPosInfo.rotation, rotLerpSpeed);
            }
        }
    }

    private bool LerpComplete(Vector3 startPos, Vector3 endPos)
    {
        if (Mathf.Abs(endPos.x - startPos.x) <= 0.001f
            && Mathf.Abs(endPos.y - startPos.y) <= 0.001f
            && Mathf.Abs(endPos.z - startPos.z) <= 0.001f)
        {
            return true;
        }
        return false;
    }

}