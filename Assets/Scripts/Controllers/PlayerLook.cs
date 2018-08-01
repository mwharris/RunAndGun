using System;
using UnityEngine;

[Serializable]
public class PlayerLook
{
    public Transform neck;
    public Transform firstSpine;
    public Transform lastSpine;

    private float minVerticalRotation = -50f;
	private float maxVerticalRotation = 53f;

    private Quaternion playerLocalRot;
	private Quaternion camLocalRot;
    private Quaternion neckLocalRot;

    public void Init(Transform player, Transform camera)
	{
		playerLocalRot = player.localRotation;
		camLocalRot = camera.localRotation;
        neckLocalRot = neck.localRotation;
    }

    //Called from FirstPersonController to handle look rotations
    public float LookRotation(LookRotationInput lri)
    {
        //Update inputs according to Options menu
        Vector2 inputs = ApplyOptionsToInput(lri);
        //Update player, camera, and head rotations based on inputs
        return ApplyLookRotations(inputs, lri);
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

    //Handle all updating of camera rotations and horizontal player body rotations
    private float ApplyLookRotations(Vector2 inputs, LookRotationInput lri)
    {
        camLocalRot = lri.camera.localRotation;
        playerLocalRot = lri.player.localRotation;
        Quaternion nanTest = lri.player.localRotation;
        //Apply the rotation to camera (vertical look rotation)
        camLocalRot *= Quaternion.Euler(-inputs.x, 0f, 0f);
        playerLocalRot *= Quaternion.Euler(0f, inputs.y, 0f);
        neckLocalRot *= Quaternion.Euler(0f, -inputs.x, 0f);
        //Clamp the player rotation in the y axis if we are wall-running
        if (lri.wallRunAngle1 != 0 || lri.wallRunAngle2 != 0)
        {
            playerLocalRot = ClampWallRunRotation(playerLocalRot, lri.wallRunAngle1, lri.wallRunAngle2, lri.wrapAround);
        }
        //Clamp rotation camera and head rotations to not look too far up/down
        camLocalRot = ClampRotationAroundAxis(camLocalRot, "x");
        neckLocalRot = ClampRotationAroundAxis(neckLocalRot, "y");
        //If we are wall-running then add a rotation in the z-axis
        camLocalRot.z = lri.wallRunZRotation;
        if (lri.wallRunZRotation == 0)
        {
            camLocalRot.y = 0;
        }
        //Special Case to catch error with player local rotations
        if (float.IsNaN(playerLocalRot.x) || float.IsNaN(playerLocalRot.y) || float.IsNaN(playerLocalRot.z))
        {
            if (float.IsNaN(playerLocalRot.x)) { playerLocalRot.x = 0f; }
            if (float.IsNaN(playerLocalRot.y)) { playerLocalRot.y = 0f; }
            if (float.IsNaN(playerLocalRot.z)) { playerLocalRot.z = 0f; }
        }
        //Update the rotation of our player and camera
        lri.player.localRotation = playerLocalRot;
        lri.camera.localRotation = camLocalRot;
        //Return the angle of our head for the animator
        return (2.0f * Mathf.Rad2Deg * Mathf.Atan(neckLocalRot.y / neckLocalRot.w));
    }

    private Quaternion ClampRotationAroundAxis(Quaternion q, string axis)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
    
        float angle = 0;
        if (axis == "x")
        {
            angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        }
        else if (axis == "y")
        {
            angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
        }

        angle = Mathf.Clamp(angle, minVerticalRotation, maxVerticalRotation);

        float newVal = Mathf.Tan(0.5f * Mathf.Deg2Rad * angle);
        if (axis == "x")
        {
            q.x = newVal;
        }
        else if (axis == "y")
        {
            q.y = newVal;
        }

        return q;
    }

    private Quaternion ClampWallRunRotation(Quaternion q, float angle1, float angle2, bool wrapAround)
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

    //Custom clamp method to handle fixing our circular rotation issue in ClampRotationAroundYAxis
    private float CustomClamp(float value, float lowerBound, float upperBound, bool wrapAround)
    {
        //Only take action if the clamp value is outside the bounded area.
        //Special case: bounded area wraps around the 360 degree circle leading to 2 bounded areas.
        if ((!wrapAround && (value < lowerBound || value > upperBound))
            || (wrapAround && (value > lowerBound && value < upperBound)))
        {
            //Determine which bound the value is "closer" to
            float lowerBoundDelta = 0;
            float upperBoundDelta = 0;
            //The lower bound SHOULD always be negative
            if (lowerBound <= 0)
            {
                //If the value is negative as well
                if (value < 0)
                {
                    //The delta is the distance between the two negative values
                    lowerBoundDelta = Mathf.Abs(lowerBound - value);
                }
                //If the value is positive
                else
                {
                    //The delta is the minimum of the path lowerBound -> 0 -> value OR lowerBound -> -180/180 -> value
                    float a = value + Mathf.Abs(lowerBound);
                    float b = Mathf.Abs(-180 - lowerBound) + (180 - value);
                    lowerBoundDelta = Mathf.Min(a, b);
                }
            }
            else
            {
                Debug.LogError("LOWER BOUND IS POSITIVE!!!");
            }
            //The lower bound SHOULD always be positive
            if (upperBound >= 0)
            {
                //If the value is negative
                if (value < 0)
                {
                    //The delta is the minimum of the path upperBound -> 0 -> value OR upperBound -> -180/180 -> value
                    float a = upperBound + Mathf.Abs(value);
                    float b = (180 - upperBound) + Mathf.Abs(-180 - value);
                    upperBoundDelta = Mathf.Min(a, b);
                }
                //If the value is positive as well
                else
                {
                    //The delta is simply the distance between the two
                    upperBoundDelta = Mathf.Abs(upperBound - value);
                }
            }
            else
            {
                Debug.LogError("UPPER BOUND IS NEGATIVE!!!");
            }
            //Determine which bound is closer to the value and clamp to that
            if (lowerBoundDelta < upperBoundDelta)
            {
                value = lowerBound;
            }
            else if (upperBoundDelta < lowerBoundDelta)
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
