using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour 
{
	[SerializeField] private float amount;
	[SerializeField] private float smoothAmount;

	private Vector3 initialPosition;

	void Start()
	{
		//Store the initial local position of the weapon
		initialPosition = transform.localPosition;
	}

	void Update()
	{
		//Get any mouse movement in the x and y axis and negate in order to move the weapon the opposite way
		float movementX = -Input.GetAxis("Mouse X") * amount;
		float movementY = -Input.GetAxis("Mouse Y") * amount;
		//Create a target vector based on the negative mouse movement
		Vector3 finalPosition = new Vector3(movementX, movementY, 0);
		//Add the target amounts to the initial position to make it relative
		finalPosition = finalPosition + initialPosition;
		//Lerp our current position to the determined target position
		transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, smoothAmount * Time.deltaTime);
	}
}
