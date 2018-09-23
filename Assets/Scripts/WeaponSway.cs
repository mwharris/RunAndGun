using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : AbstractBehavior 
{
    [SerializeField] private InputState inputState;
    [SerializeField] private float amount;
	[SerializeField] private float maxAmount;
	[SerializeField] private float smoothAmount;
	[SerializeField] private float jumpDivisor;
	[SerializeField] private Transform weapon;

	private Vector3 initialPosition;
	private GameManager gm;

	void Start()
	{
		//Store the initial local position of the weapon
		initialPosition = weapon.localPosition;
		//Get a reference to the global GameManager
		gm = GameObject.FindObjectOfType<GameManager>();
	}

	void Update()
	{
		if(gm.GetGameState() != GameManager.GameState.paused)
		{
			//Get any movement in X and Y and negate it in order to the move the weapon in the opposite direction
			Vector2 lookInput = GetInputs();
			float movementX = -lookInput.y * amount;
			float movementY = -lookInput.x * amount;
			//Account for y-velocity while jumping and falling
			if(!inputState.playerIsGrounded)
			{
				movementY += (-inputState.playerVelocity.y / jumpDivisor) * amount;
			}
			//Clamp the value at the max amount
			movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
			movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);
			//Create a target vector based on the negative mouse movement
			Vector3 finalPosition = new Vector3(movementX, movementY, 0);
			//Add the target amounts to the initial position to make it relative
			finalPosition = finalPosition + initialPosition;
			//Lerp our current position to the determined target position
			weapon.localPosition = Vector3.Lerp(weapon.localPosition, finalPosition, smoothAmount * Time.deltaTime);
		}
	}

	private Vector2 GetInputs()
	{
		Vector2 lookInput = Vector2.zero;
		if (inputs != null)
		{
			float horizontalRot = 0f;
			float verticalRot = 0f;

			verticalRot += inputState.GetButtonValue(inputs[0]);
			verticalRot += inputState.GetButtonValue(inputs[1]);
			horizontalRot += inputState.GetButtonValue(inputs[2]);
			horizontalRot += inputState.GetButtonValue(inputs[3]);

			lookInput = new Vector2(verticalRot, horizontalRot);
		}
		return lookInput;
	}
}
