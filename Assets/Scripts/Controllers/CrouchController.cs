using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchController : MonoBehaviour {

	[HideInInspector] public bool isCrouching = false;
	[HideInInspector] public float crouchMovementSpeed;

	public float crouchCamHeight;
	public float crouchBodyScale;
	public float crouchBodyPos;

	private float crouchDeltaHeight;
	private float crouchDeltaScale;
	private float crouchDeltaPos;
	private float standardCamHeight = 2.5f;
	private float standardBodyScale = 1.5f;
	private float standardBodyPos = 1.5f;

	public void CalculateCrouchVars(float movementSpeed)
	{
		crouchDeltaHeight = standardCamHeight - crouchCamHeight;
		crouchDeltaScale = standardBodyScale - crouchBodyScale;
		crouchDeltaPos = standardBodyPos - crouchBodyPos;
		//Calculate the movement speed while crouched
		crouchMovementSpeed = movementSpeed / 2;
	}

	public void HandleCrouching(CharacterController cc, Camera playerCamera, GameObject playerBody, bool crouchBtnDown)
	{
		//Crouching logic
		if(crouchBtnDown)
		{
			ToggleCrouch(playerBody);
			if(isCrouching)
			{
				cc.height = 1.9f;
			}
			else
			{
				cc.height = 2.9f;
			}
		}
		//Store the local position for modification
		Vector3 camLocalPos = playerCamera.transform.localPosition;
		Vector3 bodyLocalPos = playerBody.transform.localPosition;
		Vector3 bodyLocalScale = playerBody.transform.localScale;
		//Modify the local position based on if we are/aren't crouching
		if(isCrouching)
		{
			if(camLocalPos.y > crouchCamHeight)
			{
				//Move the camera down
				camLocalPos.y = LowerHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, crouchCamHeight);
			}
			if(bodyLocalScale.y > crouchBodyScale)
			{
				//Scale the body down
				bodyLocalScale.y = LowerHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, crouchBodyScale);
			}
		}
		else
		{
			if(camLocalPos.y < standardCamHeight)
			{
				//Move the camera up
				camLocalPos.y = RaiseHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, standardCamHeight);
			}
			if(bodyLocalScale.y < standardBodyScale)
			{
				//Scale the body up
				bodyLocalScale.y = RaiseHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, standardBodyScale);
			}
		}
		//Apply the local position updates
		playerCamera.transform.localPosition = camLocalPos;
		playerBody.transform.localPosition = bodyLocalPos;
		playerBody.transform.localScale = bodyLocalScale;
	}

	//Helper function to toggle crouching flags
	public void ToggleCrouch(GameObject playerBody)
	{
		if(isCrouching)
		{
			StopCrouching(playerBody);
		}
		else 
		{
			Crouch(playerBody);
		}
	}

	//Code to actual handle crouching and standing
	private void Crouch(GameObject playerBody)
	{
		Vector3 test = new Vector3(0f, crouchDeltaHeight/2, 0f);
		playerBody.GetComponent<CapsuleCollider>().height -= crouchDeltaHeight;
		playerBody.GetComponent<CapsuleCollider>().center = playerBody.GetComponent<CapsuleCollider>().center - test;
		isCrouching = true;
	}
	public void StopCrouching(GameObject playerBody)
	{
		Vector3 test = new Vector3(0f, crouchDeltaHeight/2, 0f);
		playerBody.GetComponent<CapsuleCollider>().height += crouchDeltaHeight;
		playerBody.GetComponent<CapsuleCollider>().center = playerBody.GetComponent<CapsuleCollider>().center + test;
		isCrouching = false;		
	}

	//Helper function to lower the height of the player due to crouching
	private float LowerHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos - (delta * deltaTime * 8) < height)
		{
			yPos = height;
		}
		else
		{
			yPos -= delta * Time.deltaTime * 8;
		}
		return yPos;
	}

	//Helper function to raise the height of the player due to standing
	private float RaiseHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos + (delta * deltaTime * 8) > height)
		{
			yPos = height;
		}
		else
		{
			yPos += delta * Time.deltaTime * 8;
		}
		return yPos;
	}
}
