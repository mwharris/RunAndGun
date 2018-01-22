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
	[SerializeField] private float crouchBodyScale;
	[SerializeField] private float crouchBodyPos;
	[SerializeField] private float crouchCCHeight;
	[SerializeField] private float crouchCCCenter;

	private float crouchDeltaHeight;
	private float crouchDeltaScale;
	private float crouchDeltaPos;
	private float crouchDeltaCCHeight;
	private float crouchDeltaCCCenter;

	private float standardCamHeight;
	private float standardBodyScale;
	private float standardBodyPos;
	private float standardCCHeight;
	private float standardCCCenter;

	/**
	 * Calculate some variables needed for crouching logic
	 */
	//TODO: Make all "standard" values passed-in not hard-coded
	public void CalculateCrouchVars(GameObject player, GameObject playerCamera, float movementSpeed)
	{
		//Store the standard character controller height
		standardCamHeight = playerCamera.transform.localPosition.y;
		Transform body = player.transform.GetChild(0);
		standardBodyScale = body.localScale.y;
		standardBodyPos = body.localPosition.y;
		standardCCHeight = player.GetComponent<CharacterController>().height;
		standardCCCenter = player.GetComponent<CharacterController>().center.y;
		//Calculate the change in positions based on desired crouch variables
		crouchDeltaHeight = standardCamHeight - crouchCamHeight;
		crouchDeltaScale = standardBodyScale - crouchBodyScale;
		crouchDeltaPos = standardBodyPos + crouchBodyPos;
		crouchDeltaCCHeight = standardCCHeight - crouchCCHeight;
		crouchDeltaCCCenter = standardCCCenter - crouchCCCenter;
		//Calculate the movement speed while crouched
		crouchMovementSpeed = movementSpeed / 2;
	}

	/**
	 * Handle shrinking / expanding the player and attached components when crouching or standing 
	 */
	public void HandleCrouching(CharacterController cc, Camera playerCamera, GameObject playerBody, GameManager.GameState gs)
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
			Vector3 bodyLocalPos = playerBody.transform.localPosition;
			Vector3 bodyLocalScale = playerBody.transform.localScale;
			float ccHeight = cc.height;
			Vector3 ccCenter = cc.center;
			//Modify the local position over time based on if we are/aren't crouching
			if(inputState.playerIsCrouching)
			{
				if(bodyLocalScale.y > crouchBodyScale)
				{
					bodyLocalScale.y = LowerHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, crouchBodyScale);
				}
				if(bodyLocalPos.y > crouchBodyPos)
				{
					bodyLocalPos.y = LowerHeight(bodyLocalPos.y, crouchDeltaPos, Time.deltaTime, crouchBodyPos);
				}
				if(ccHeight > crouchCCHeight)
				{
					ccHeight = LowerHeight(ccHeight, crouchDeltaCCHeight, Time.deltaTime, crouchCCHeight);
				}
				if(ccCenter.y > crouchCCCenter)
				{
					ccCenter.y = LowerHeight(ccCenter.y, crouchDeltaCCCenter, Time.deltaTime, crouchCCCenter);
				}
				if(camLocalPos.y > crouchCamHeight)
				{
					camLocalPos.y = LowerHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, crouchCamHeight);
				}
			}
			else if(cc.isGrounded)
			{
				if(bodyLocalScale.y < standardBodyScale)
				{
					bodyLocalScale.y = RaiseHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, standardBodyScale);
				}
				if(bodyLocalPos.y < standardBodyPos)
				{
					bodyLocalPos.y = RaiseHeight(bodyLocalPos.y, crouchDeltaPos, Time.deltaTime, standardBodyPos);
				}
				if(ccHeight < crouchCCHeight)
				{
					ccHeight = RaiseHeight(ccHeight, crouchDeltaCCHeight, Time.deltaTime, standardCCHeight);
				}
				if(ccCenter.y < crouchCCCenter)
				{
					ccCenter.y = RaiseHeight(ccCenter.y, crouchDeltaCCCenter, Time.deltaTime, standardCCCenter);
				}
				if(camLocalPos.y < standardCamHeight)
				{
					camLocalPos.y = RaiseHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, standardCamHeight);
				}
				//Special case: when we are standing, we need to mark the camera as being moved since other scripts try to adjust the camera's position while standing
				else 
				{
					cameraResetting = false;
				}
			}
			//Apply the local position updates
			playerCamera.transform.localPosition = camLocalPos;
			playerBody.transform.localPosition = bodyLocalPos;
			playerBody.transform.localScale = bodyLocalScale;
			cc.height = ccHeight;
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
			//Flag the camera as being moved
			cameraResetting = true;
		}
		else 
		{
			Crouch();
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
	}

	/**
	 * Helper function to lower the height of the player due to crouching
	 */
	private float LowerHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos - (delta * deltaTime * crouchSpeed) < height)
		{
			yPos = height;
		}
		else
		{
			yPos -= delta * Time.deltaTime * crouchSpeed;
		}
		return yPos;
	}

	/**
	 * Helper function to raise the height of the player due to standing
	 */
	private float RaiseHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos + (delta * deltaTime * crouchSpeed) > height)
		{
			yPos = height;
		}
		else
		{
			yPos += delta * Time.deltaTime * crouchSpeed;
		}
		return yPos;
	}
}
