using System;
using UnityEngine;

[Serializable]
public class PlayerLook {

	private float minVerticalRotation = -90f;
	private float maxVerticalRotation = 90f;

	private Quaternion playerLocalRot;
	private Quaternion camLocalRot;
	private float camOrigZ;

	public void Init(Transform player, Transform camera)
	{
		playerLocalRot = player.localRotation;
		camLocalRot = camera.localRotation;
		camOrigZ = camera.localRotation.eulerAngles.z;
	}

	public void LookRotation(Transform player, Transform camera, Vector2 lookInput, float deltaTime, float mouseSensitivity, bool invertY, float wallRunZRotation)
	{
		//Add mouse sensitivity to the controller input
		Vector2 inputs = lookInput * mouseSensitivity;
		//Invert the Y input if Options dictates it
		if (invertY) {
			inputs = new Vector2(-inputs.x, inputs.y);
		}
		//Apply the rotation to both the player and the camera
		camLocalRot = camera.localRotation;
		playerLocalRot *= Quaternion.Euler(0f, inputs.y, 0f);
		camLocalRot *= Quaternion.Euler(-inputs.x, 0f, 0f);
		//Clamp the up/down rotation in the x axis 
		camLocalRot = ClampRotationAroundXAxis(camLocalRot);
		//If we are wall-running then add a rotation in the z-axis
		if (wallRunZRotation != 0) 
		{
			float blah = Mathf.Lerp(camLocalRot.eulerAngles.z, wallRunZRotation, 30*deltaTime);
			camLocalRot.eulerAngles = new Vector3(camLocalRot.eulerAngles.x, camLocalRot.eulerAngles.y, blah);
		}
		//Reset this if we're done wall-running
		else if (wallRunZRotation == 0 && camLocalRot.z != 0) 
		{
			//This just seems to work better than reversing the above...
			camLocalRot.z = Mathf.Lerp(camLocalRot.z, 0f, 30*deltaTime);
		}
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
