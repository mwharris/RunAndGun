using System;
using UnityEngine;

[Serializable]
public class PlayerLook {

	private float minVerticalRotation = -90f;
	private float maxVerticalRotation = 90f;

	private Quaternion playerLocalRot;
	private Quaternion camLocalRot;

	public float mouseSensitivity = 5.0f;

	public void Init(Transform player, Transform camera)
	{
		playerLocalRot = player.localRotation;
		camLocalRot = camera.localRotation;
	}

	public void LookRotation(Transform player, Transform camera, Vector2 lookInput)
	{
		//Add mouse sensitivity to the controller input
		Vector2 inputs = lookInput * mouseSensitivity;

		//Apply the rotation to both the player and the camera
		//Debug.Log(inputs);
		playerLocalRot *= Quaternion.Euler(0f, inputs.y, 0f);
		camLocalRot *= Quaternion.Euler(-inputs.x, 0f, 0f);

		//Clamp the rotation in the x axis 
		camLocalRot = ClampRotationAroundXAxis(camLocalRot);

		//Update the rotation of our camera
		player.localRotation = playerLocalRot;
		camera.localRotation = camLocalRot;
	}

	//Helper method from MouseLook.cs
	private Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

		angleX = Mathf.Clamp (angleX, minVerticalRotation, maxVerticalRotation);

		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}
}
