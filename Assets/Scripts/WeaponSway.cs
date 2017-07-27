using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour 
{
	[SerializeField] private float amount;
	[SerializeField] private float maxAmount;
	[SerializeField] private float smoothAmount;
	[SerializeField] private float jumpDivisor;

	private Vector3 initialPosition;
	private FirstPersonController fpc;

	void Start()
	{
		//Store the initial local position of the weapon
		initialPosition = transform.localPosition;
		//Get a reference to the first person controller
		fpc = GetComponentInParent<Transform>().GetComponentInParent<Transform>().GetComponentInParent<FirstPersonController>();
	}

	void Update()
	{
		//Get any mouse movement in the x and y axis and negate in order to move the weapon the opposite way
		float movementX = -Input.GetAxis("Mouse X") * amount;
		float movementY = -Input.GetAxis("Mouse Y") * amount;
		//Account for y-velocity while jumping and falling
		if(fpc != null && !fpc.isGrounded)
		{
			movementY += (-fpc.velocity.y / jumpDivisor) * amount;
		}
		//Clamp the value at the max amount
		movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
		movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);
		//Create a target vector based on the negative mouse movement
		Vector3 finalPosition = new Vector3(movementX, movementY, 0);
		//Add the target amounts to the initial position to make it relative
		finalPosition = finalPosition + initialPosition;
		//Lerp our current position to the determined target position
		transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, smoothAmount * Time.deltaTime);
	}
}
