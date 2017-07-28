using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchController : MonoBehaviour {

	private bool isCrouching = false;
	public bool IsCrouching {
		get { return isCrouching; }
	}

	private float crouchMovementSpeed;
	public float CrouchMovementSpeed {
		get { return crouchMovementSpeed; }
	}

	[HideInInspector] public bool cameraResetting = false;

	[SerializeField] private float crouchCamHeight;
	[SerializeField] private float crouchBodyScale;
	[SerializeField] private float crouchBodyPos;
	[SerializeField] private float crouchCapColHeight;
	[SerializeField] private float crouchCapColCenter;

	private float crouchDeltaHeight;
	private float crouchDeltaScale;
	private float crouchDeltaPos;
	private float crouchDeltaCapColHeight;
	private float crouchDeltaCapColCenter;
	private float standardCamHeight = 2.5f;
	private float standardBodyScale = 1.5f;
	private float standardBodyPos = 1.5f;
	private float standardCapColHeight = 2.0f;
	private float standardCapColCenter = 0.0f;

	public void CalculateCrouchVars(float movementSpeed)
	{
		crouchDeltaHeight = standardCamHeight - crouchCamHeight;
		crouchDeltaScale = standardBodyScale - crouchBodyScale;
		crouchDeltaPos = standardBodyPos - crouchBodyPos;
		crouchDeltaCapColHeight = standardCapColHeight - crouchCapColHeight;
		crouchDeltaCapColCenter = standardCapColCenter - crouchCapColCenter;
		//Calculate the movement speed while crouched
		crouchMovementSpeed = movementSpeed / 2;
	}

	public void HandleCrouching(CharacterController cc, Camera playerCamera, GameObject playerBody, bool crouchBtnDown)
	{
		//Crouching logic
		if(crouchBtnDown)
		{
			ToggleCrouch(playerBody);
		}
		//Store the local position for modification
		Vector3 camLocalPos = playerCamera.transform.localPosition;
		Vector3 bodyLocalPos = playerBody.transform.localPosition;
		Vector3 bodyLocalScale = playerBody.transform.localScale;
		CapsuleCollider capCol = playerBody.GetComponent<CapsuleCollider>();
		float capColHeight = capCol.height;
		Vector3 capColCenter = capCol.center;
		//Modify the local position based on if we are/aren't crouching
		if(isCrouching)
		{
			if(camLocalPos.y > crouchCamHeight)
			{
				camLocalPos.y = LowerHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, crouchCamHeight);
			}
			if(bodyLocalScale.y > crouchBodyScale)
			{
				bodyLocalScale.y = LowerHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, crouchBodyScale);
			}
			if(bodyLocalPos.y > crouchBodyPos)
			{
				bodyLocalPos.y = LowerHeight(bodyLocalPos.y, crouchDeltaPos, Time.deltaTime, crouchBodyPos);
			}
			if(capColHeight > crouchCapColHeight)
			{
				capColHeight = LowerHeight(capColHeight, crouchDeltaCapColHeight, Time.deltaTime, crouchCapColHeight);
			}
			if(capColCenter.y > crouchCapColCenter)
			{
				capColCenter.y -= (crouchDeltaHeight/2);
			}
		}
		else if(cc.isGrounded)
		{
			if(camLocalPos.y < standardCamHeight)
			{
				camLocalPos.y = RaiseHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, standardCamHeight);
			}
			else 
			{
				cameraResetting = false;
			}
			if(bodyLocalScale.y < standardBodyScale)
			{
				bodyLocalScale.y = RaiseHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, standardBodyScale);
			}
			if(bodyLocalPos.y < standardBodyPos)
			{
				bodyLocalPos.y = RaiseHeight(bodyLocalPos.y, crouchDeltaPos, Time.deltaTime, standardBodyPos);
			}
			if(capColHeight < standardCapColHeight)
			{
				capColHeight = RaiseHeight(capColHeight, crouchDeltaCapColHeight, Time.deltaTime, standardCapColHeight);
			}
			if(capColCenter.y < standardCapColCenter)
			{
				capColCenter.y += (crouchDeltaHeight/2);
			}
		}
		//Apply the local position updates
		playerCamera.transform.localPosition = camLocalPos;
		playerBody.transform.localPosition = bodyLocalPos;
		playerBody.transform.localScale = bodyLocalScale;
		capCol.height = capColHeight;
		capCol.center = capColCenter;
	}

	//Helper function to toggle crouching flags
	public void ToggleCrouch(GameObject playerBody)
	{
		if(isCrouching)
		{
			isCrouching = false;
			cameraResetting = true;
		}
		else 
		{
			isCrouching = true;
		}
	}

	//Code to actual handle crouching and standing
	private void Crouch(GameObject playerBody)
	{
		isCrouching = true;
	}
	public void StopCrouching(GameObject playerBody)
	{
		isCrouching = false;		
	}

	//Helper function to lower the height of the player due to crouching
	private float LowerHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos - (delta * deltaTime * 2) < height)
		{
			yPos = height;
		}
		else
		{
			yPos -= delta * Time.deltaTime * 2;
		}
		return yPos;
	}

	//Helper function to raise the height of the player due to standing
	private float RaiseHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos + (delta * deltaTime * 2) > height)
		{
			yPos = height;
		}
		else
		{
			yPos += delta * Time.deltaTime * 2;
		}
		return yPos;
	}
}
