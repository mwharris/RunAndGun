using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchController : AbstractBehavior {

	private float crouchMovementSpeed;
	public float CrouchMovementSpeed {
		get { return crouchMovementSpeed; }
	}

	//Special flag to mark the player camera as changing positions, used in FirstPersonController
	[HideInInspector] public bool cameraResetting = false;

	[SerializeField] private float crouchSpeed = 8.0f;
	[SerializeField] private float crouchCamHeight;
	[SerializeField] private float crouchCCHeight;
    [SerializeField] private float crouchCCRadius;
    [SerializeField] private Vector3 crouchCCCenter;

    private float crouchDeltaHeight;
    private float crouchDeltaDepth;
	private float crouchDeltaCCHeight;
    private float crouchDeltaCCRadius;
    private Vector3 crouchDeltaCCCenter;

	private float standardCamHeight;
    private float standardCamDepth;
	private float standardCCHeight;
    private float standardCCRadius;
    private Vector3 standardCCCenter;

	/**
	 * Calculate some variables needed for crouching logic
	 */
	public void CalculateCrouchVars(GameObject player, GameObject playerCamera, float movementSpeed)
	{
        //Store the standard camera heights and depths
        standardCamHeight = playerCamera.transform.localPosition.y;
        standardCamDepth = playerCamera.transform.localPosition.z;
        //Store the standard body/character controller heights and depths
		standardCCHeight = player.GetComponent<CharacterController>().height;
		standardCCCenter = player.GetComponent<CharacterController>().center;
        standardCCRadius = player.GetComponent<CharacterController>().radius;
        //Calculate the change in positions based on desired crouch variables
        crouchDeltaHeight = standardCamHeight - crouchCamHeight;
		crouchDeltaCCHeight = standardCCHeight - crouchCCHeight;
        crouchDeltaCCRadius = crouchCCRadius - standardCCRadius;
        //Calculate the movement speed while crouched
        crouchMovementSpeed = movementSpeed / 2;
	}

	/**
	 * Handle shrinking / expanding the player and attached components when crouching or standing 
	 */
	public void HandleCrouching(CharacterController cc, Camera playerCamera, GameManager.GameState gs)
	{
		if (gs == GameManager.GameState.playing) 
		{
			bool isCrouchDown = inputState.GetButtonPressed(inputs[0]) && inputState.GetButtonHoldTime(inputs[0]) == 0;
			//Crouching logic
			if(isCrouchDown)
			{
				ToggleCrouch();
			}
			//Store the local position for modification
			Vector3 camLocalPos = playerCamera.transform.localPosition;
			float ccHeight = cc.height;
            float ccRadius = cc.radius;
			Vector3 ccCenter = cc.center;
            //Modify the local position over time based on if we are/aren't crouching
            if (inputState.playerIsCrouching && inputState.playerIsGrounded)
			{
				if(ccHeight > crouchCCHeight)
				{
                    ccHeight = Mathf.Lerp(ccHeight, crouchCCHeight, Time.deltaTime * 4f);
                }
                if (ccRadius < crouchCCRadius)
                {
                    ccRadius = Mathf.Lerp(ccRadius, crouchCCRadius, Time.deltaTime * 4f);
                }
                if(ccCenter != crouchCCCenter)
				{
                    ccCenter = new Vector3(0, (ccHeight / 2) + 0.3f, 0);
                }
                if (camLocalPos.y > crouchCamHeight)
                {
                    camLocalPos.y = Mathf.Lerp(camLocalPos.y, crouchCamHeight, Time.deltaTime * 4f);
				}
            }
			else if(cameraResetting && inputState.playerIsGrounded)
			{
				if(ccHeight < standardCCHeight)
				{
                    ccHeight = Mathf.Lerp(ccHeight, standardCCHeight, Time.deltaTime * 8f);
                }
                if (ccRadius > standardCCRadius)
                {
                    ccRadius = Mathf.Lerp(ccRadius, standardCCRadius, Time.deltaTime * 8f);
                }
                if(ccCenter != standardCCCenter)
				{
                    ccCenter = new Vector3(0, (ccHeight / 2) + 0.3f, 0);
                }
                if (camLocalPos.y < standardCamHeight)
                {
                    camLocalPos.y = Mathf.Lerp(camLocalPos.y, standardCamHeight, Time.deltaTime * 8f);
                }
				//Special case: when we are standing, we need to mark the camera as being moved since other scripts try to adjust the camera's position while standing
				else 
				{
					cameraResetting = false;
                }
            }
			//Apply the local position updates
			playerCamera.transform.localPosition = camLocalPos;
			cc.height = ccHeight;
            cc.radius = ccRadius;
			cc.center = ccCenter;
        }    
    }

    /**
        * Helper function to toggle crouching flags
        */
    public void ToggleCrouch()
	{
		if(inputState.playerIsCrouching)
		{
			StopCrouching();
		}
		else 
		{
			Crouch();
            //Make sure we disable sprinting
            inputState.playerIsSprinting = false;
        }
	}

	/**
	 * Public methods to modify isCrouching variable 
	 */
	public void Crouch()
	{
		inputState.playerIsCrouching = true;
	}
	public void StopCrouching()
	{
		inputState.playerIsCrouching = false;
        //Flag the camera as being moved
        cameraResetting = true;
    }

    /**
	 * Helper function to lower the height of the player due to crouching
	 */
    private float LowerHeight(float posValue, float delta, float deltaTime, float newPosValue)
    {
        return LowerHeight(posValue, delta, deltaTime, newPosValue, crouchSpeed);
    }
    private float LowerHeight(float posValue, float delta, float deltaTime, float newPosValue, float speed)
	{
		if(posValue - (delta * deltaTime * speed) < newPosValue)
		{
            posValue = newPosValue;
		}
		else
		{
            posValue -= delta * Time.deltaTime * speed;
		}
		return posValue;
	}

    /**
	 * Helper function to raise the height of the player due to standing
	 */
    private float RaiseHeight(float posValue, float delta, float deltaTime, float newPosValue)
    {
        return RaiseHeight(posValue, delta, deltaTime, newPosValue, crouchSpeed);
    }
    private float RaiseHeight(float posValue, float delta, float deltaTime, float newPosValue, float speed)
	{
		if(posValue + (delta * deltaTime * speed) > newPosValue)
		{
            posValue = newPosValue;
		}
		else
		{
            posValue += delta * Time.deltaTime * speed;
		}
		return posValue;
	}

    /**
	 * Helper functions to raise the center of the character controller due to standing/crouching
	 */
    private Vector3 LowerCCCenter(Vector3 ccCenter, Vector3 delta, float deltaTime, Vector3 newCCCenter)
    {
        ccCenter.x = LowerHeight(ccCenter.x, Mathf.Abs(ccCenter.x - newCCCenter.x), deltaTime, newCCCenter.x);
        ccCenter.y = LowerHeight(ccCenter.y, Mathf.Abs(ccCenter.y - newCCCenter.y), deltaTime, newCCCenter.y);
        ccCenter.z = LowerHeight(ccCenter.z, Mathf.Abs(ccCenter.z - newCCCenter.z), deltaTime, newCCCenter.z);
        return ccCenter;
    }
    private Vector3 RaiseCCCenter(Vector3 ccCenter, Vector3 delta, float deltaTime, Vector3 newCCCenter)
    {
        ccCenter.x = RaiseHeight(ccCenter.x, Mathf.Abs(newCCCenter.x - ccCenter.x), deltaTime, newCCCenter.x);
        ccCenter.y = RaiseHeight(ccCenter.y, Mathf.Abs(newCCCenter.y - ccCenter.y), deltaTime, newCCCenter.y);
        ccCenter.z = RaiseHeight(ccCenter.z, Mathf.Abs(newCCCenter.z - ccCenter.z), deltaTime, newCCCenter.z);
        return ccCenter;
    }
}
