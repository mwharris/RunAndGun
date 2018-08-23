using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (CharacterController))]
public class PlayerController : AbstractBehavior {
	
	private CharacterController cc;
	private Vector3 velocity;
	private Vector2 moveInput;
	private Vector2 lookInput;

	[SerializeField] private PlayerLook playerLook;
	[SerializeField] private Camera playerCam;
	[SerializeField] private float speed = 5f;

	void Start()
	{
		cc = GetComponent<CharacterController>();
		//playerLook.Init(transform, playerCam.transform);
	}

	private void Update()
	{
		GetInput();
		RotateView();
	}

	private void FixedUpdate()
	{
		//Determine desired move direction using directional input vector
		Vector3 moveDirection = transform.forward*moveInput.x + transform.right*moveInput.y;

		//Update our velocity accordingly, adding movement speed
		velocity.x = moveDirection.x * speed;
		velocity.z = moveDirection.z * speed;

		//Call a method to actually perform the movement
		cc.Move(velocity * Time.fixedDeltaTime);
	}
		
	private void GetInput()
	{
		GetMoveInput();
		GetLookInput();
	}

	private void GetMoveInput()
	{
		//Gather input axis and set movement accordingly
		float forwardDir = 0f;
		float sideDir = 0f;
		if (inputState.GetButtonPressed(inputs[0])) 
		{
			forwardDir += inputState.GetButtonValue(inputs[0]);
		}
		if (inputState.GetButtonPressed(inputs[1])) 
		{
			forwardDir += inputState.GetButtonValue(inputs[1]);
		}
		if (inputState.GetButtonPressed(inputs[2])) 
		{
			sideDir += inputState.GetButtonValue(inputs[2]);
		}
		if (inputState.GetButtonPressed(inputs[3])) 
		{
			sideDir += inputState.GetButtonValue(inputs[3]);
		}

		//Store values in an input vector
		moveInput = new Vector2(forwardDir, sideDir);

		//Normalize if values exceed 1, this prevents faster diagonal movement
		if (moveInput.sqrMagnitude > 1) 
		{
			moveInput.Normalize();
		}
	}

	private void GetLookInput()
	{
		float horizontalRot = 0f;
		float verticalRot = 0f;

		verticalRot += inputState.GetButtonValue(inputs[4]);
		verticalRot += inputState.GetButtonValue(inputs[5]);
		horizontalRot += inputState.GetButtonValue(inputs[6]);
		horizontalRot += inputState.GetButtonValue(inputs[7]);

		lookInput = new Vector2(verticalRot, horizontalRot);
	}

	private void RotateView()
	{
//		playerLook.LookRotation(transform, playerCam.transform, lookInput, 5.0f, true);
	}
}
