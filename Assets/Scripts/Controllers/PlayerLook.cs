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

    //Called from FirstPersonController to handle look rotations
	public void LookRotation(LookRotationInput lri)
	{
        //Update inputs according to Options menu
        Vector2 inputs = ApplyOptionsToInput(lri);
        //Update player and camera rotations based on inputs
        ApplyRotations(inputs, lri);
    }

    //Update inputs with settings from Options menu
    private Vector2 ApplyOptionsToInput(LookRotationInput lri)
    {
        //Add mouse sensitivity to the controller input
        Vector2 inputs = lri.lookInput * lri.mouseSensitivity;
        //Invert the Y input if Options dictates it
        if (lri.invertY)
        {
            inputs = new Vector2(-inputs.x, inputs.y);
        }
        return inputs;
    }

    //Handle all updating of player rotations, vertical and horizontal
    private void ApplyRotations(Vector2 inputs, LookRotationInput lri)
    {
        camLocalRot = lri.camera.localRotation;
        playerLocalRot = lri.player.localRotation;
        Quaternion nanTest = lri.player.localRotation;
        //Apply the rotation to camera (vertical look rotation)
        camLocalRot *= Quaternion.Euler(-inputs.x, 0f, 0f);
        playerLocalRot *= Quaternion.Euler(0f, inputs.y, 0f);
        //Clamp the rotations in the y axis if we are wall-running
        if (lri.wallRunAngle1 != 0 || lri.wallRunAngle2 != 0)
        {
            playerLocalRot = ClampRotationAroundYAxis(playerLocalRot, lri.wallRunAngle1, lri.wallRunAngle2, lri.wrapAround);
        }
        //Clamp the x rotation as well
        camLocalRot = ClampRotationAroundXAxis(camLocalRot);
        //If we are wall-running then add a rotation in the z-axis
        camLocalRot.z = lri.wallRunZRotation;
        //Update the rotation of our camera
        if (float.IsNaN(playerLocalRot.x) || float.IsNaN(playerLocalRot.y) || float.IsNaN(playerLocalRot.z))
        {
            Debug.LogError("Rotation is NaN!");
            Debug.LogError("Old rotation is: " + nanTest);
            Debug.LogError("Current player rotation is: " + lri.player.localRotation);
            Debug.LogError("NaN is: " + playerLocalRot);
            if (float.IsNaN(playerLocalRot.x)) { playerLocalRot.x = 0f; }
            if (float.IsNaN(playerLocalRot.y)) { playerLocalRot.y = 0f; }
            if (float.IsNaN(playerLocalRot.z)) { playerLocalRot.z = 0f; }
        }
        lri.player.localRotation = playerLocalRot;
        lri.camera.localRotation = camLocalRot;
    }

    private Quaternion ClampRotationAroundYAxis(Quaternion q, float angle1, float angle2, bool wrapAround)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);

        if (angle1 > angle2)
        {
            angleY = CustomClamp(angleY, angle2, angle1, wrapAround);
        }
        else
        {
            angleY = CustomClamp(angleY, angle1, angle2, wrapAround);
        }

        q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

        return q;
    }

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

    //Custom clamp method to handle fixing our circular rotation issue in ClampRotationAroundYAxis
    private float CustomClamp(float value, float lowerBound, float upperBound, bool wrapAround)
    {
        //Only take action if the clamp value is outside the bounded area.
        //Special case: bounded area wraps around the 360 degree circle leading to 2 bounded areas.
        if ((!wrapAround && (value < lowerBound || value > upperBound))
            || (wrapAround && (value > lowerBound && value < upperBound)))
        {
            //Determine which bound the value is "closer" to
            float lowerBoundAbsDelta = 0;
            float upperBoundAbsDelta = 0;
            //Use both the actual value and the inverse of the value.
            //The reason for this is the way ClampRotationAroundYAxis handles calculating our angleY.
            //Once it passes the -180 mark, it flips the value.
            lowerBoundAbsDelta = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs(lowerBound));
            upperBoundAbsDelta = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs(upperBound));
            //Determine which value is closer
            if (lowerBoundAbsDelta < upperBoundAbsDelta)
            {
                value = lowerBound;
            }
            else if (upperBoundAbsDelta < lowerBoundAbsDelta)
            {
                value = upperBound;
            }
            //If we are not wrapping around -180/+180 area, use a simple clamp
            else if (!wrapAround)
            {
                value = Mathf.Clamp(value, lowerBound, upperBound);
            }
            else
            {
                //Clamp to whichever value is closer without the Abs()
                if (Mathf.Abs(value - lowerBound) < Mathf.Abs(value - upperBound))
                {
                    value = Mathf.Clamp(value, -180, lowerBound);
                }
                else
                {
                    value = Mathf.Clamp(value, upperBound, 180);
                }
            }
        }
        return value;
    }
}
