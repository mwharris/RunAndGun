using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunController : MonoBehaviour {

	/// WALL RUNNING / STICKING VARIABLES //////////////////
	private float cameraTotalRotation = 0f;
	private float cameraRotAmount = 15f;
	private float cameraRotZ = 0;
	private bool wallRunningLeft = false;
	private bool wallRunningRight = false;
	private bool wallRunningBack = false;
	private bool wallSticking = false;
	private bool initWallRun = false;
	private Vector3 wallRunDirection;
	private Vector3 wallRunNormal;
	private bool wallRunningDisabled = false;
	private float wallRunningDisabledTimer = 0.0f;
	private float wallRunTimer = 0.0f;
	private float wallRunMax = 2.0f;
	private string lastWallName = "";
	private bool wallJumped = false;
	////////////////////////////////////////////

	//Tilt the camera left or right depending on which side we are wall-running
	public void CalculateCameraTilt(Camera playerCamera)
	{
		if(wallRunningLeft || wallRunningRight)
		{
			if(cameraTotalRotation < cameraRotAmount)
			{
				float currentAngle = playerCamera.transform.localRotation.eulerAngles.z;
				if(wallRunningLeft)
				{
					cameraRotZ = currentAngle - (Time.deltaTime * 100);
				}
				else
				{
					cameraRotZ = currentAngle + (Time.deltaTime * 100);
				}
				cameraTotalRotation += Time.deltaTime * 100;
			}
			else if(cameraTotalRotation > cameraRotAmount)
			{
				cameraTotalRotation = cameraRotAmount;
			}
		}
		else 
		{
			cameraTotalRotation = 0f;
			cameraRotZ = 0;
		}
	}

}
