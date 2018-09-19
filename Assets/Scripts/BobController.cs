using UnityEngine;
using System.Collections;

public class BobController : MonoBehaviour
{
    private Vector3 origRestPosition;
    private Quaternion origRestRotation;

    public Vector3 restPosition;
    private Quaternion restRotation;

    public Vector3 aimPosition;
    public Vector3 aimRotation;

    public Vector3 crouchPosition;

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

    void Start()
    {
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;
        origRestPosition = restPosition;
        origRestRotation = restRotation;
    }

    void Update()
	{        
        //Special case for Bob Target.
        //When aiming we need to update our current Rest pos/rot to the Aim pos/rot.
        if (!isCamera && inputState.playerIsAiming)
        {
            restPosition = aimPosition;
            restRotation = Quaternion.Euler(aimRotation);
        }
        else if (!isCamera && !inputState.playerIsAiming) 
        {
            restPosition = origRestPosition;
            restRotation = origRestRotation;
        }
        
        //While we are moving apply a head/body bob
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
		{
            //Don't bob while we're aiming
            if (!inputState.playerIsAiming)
            {
                float bobSpeed = inputState.playerIsSprinting ? sprintBobSpeed : walkBobSpeed;
                float bobAmount = inputState.playerIsSprinting ? sprintBobAmount : walkBobAmount;

                //Increase / decrease bobSpeed based on our controller input
                float inputSpeed = Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical"));
                float inputSpeedClamped = Mathf.Clamp(inputSpeed, -1f, 1f);
                timer += bobSpeed * Time.deltaTime * inputSpeedClamped;

                //Bounce the position left/right in a cycle according to the timer
                transform.localPosition = new Vector3(restPosition.x + Mathf.Cos(timer) * bobAmount, restPosition.y + Mathf.Abs((Mathf.Sin(timer) * bobAmount)), restPosition.z);
            }
            //If we're not controlling a camera, also tilt slightly in the direction of the movement
            if (!isCamera)
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
        }
		else
		{
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

            if (!isCamera && restRotation != transform.localRotation)
            {
                float rotLerpSpeed = 4f * Time.deltaTime;
                if (inputState.playerIsAiming)
                {
                    rotLerpSpeed = 20f * Time.deltaTime;
                }
                transform.localRotation = Quaternion.Lerp(transform.localRotation, restRotation, rotLerpSpeed);
            }
		}

        //Completed a full cycle. Reset to 0 to avoid bloated values.
        if (timer > Mathf.PI * 2)
        {
            timer = 0;
        }
	}
}
