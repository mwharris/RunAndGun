using System;
using UnityEngine;

[Serializable]
public class PlayerLook
{
    private Transform neck;

    private float minVerticalRotation = -90f;
    private float maxVerticalRotation = 90f;

    private Quaternion playerLocalRot;
    private Quaternion camLocalRot;
    private Quaternion neckLocalRot;

    private Ray sphereRay;
    public float sphereSize = 1f;
    private float hitDistance;

    public void Init(Transform player, Transform camera, PlayerBodyData bodyData)
    {
        neck = bodyData.neck;
        playerLocalRot = player.localRotation;
        camLocalRot = camera.localRotation;
        neckLocalRot = neck.localRotation;
    }

    public void UpdateBodyData(PlayerBodyData bodyData)
    {
        neck = bodyData.neck;
    }

    //Called from FirstPersonController to handle look rotations
    public void LookRotation(LookRotationInput lri)
    {
        //Update inputs according to Options menu
        Vector2 inputs = ApplyOptionsToInput(lri);
        //Update player, camera, and head rotations based on inputs
        ApplyLookRotations(inputs, lri);
        //Perform aim assist functions
        DoAimAssist(lri);
    }

    //Update inputs with settings from Options menu
    private Vector2 ApplyOptionsToInput(LookRotationInput lri)
    {
        //Calculate sensitivity based on settings and if aiming
        float totalSensitivity = lri.mouseSensitivity;
        if (lri.isAiming || lri.lockedOnPlayer)
        {
            totalSensitivity *= 0.4f;
        }
        Vector2 inputs = lri.lookInput * totalSensitivity;
        //Invert the Y input if Options dictates it
        if (lri.invertY)
        {
            inputs = new Vector2(-inputs.x, inputs.y);
        }
        return inputs;
    }

    //Handle all updating of camera rotations and horizontal player body rotations
    private void ApplyLookRotations(Vector2 inputs, LookRotationInput lri)
    {
        if (lri.isWallRunning)
        {
            HandleWallRunningRotations(inputs, lri);
        }
        else
        {
            HandleNormalRotations(inputs, lri);
        }
    }

    private void HandleNormalRotations(Vector2 inputs, LookRotationInput lri)
    {
        camLocalRot = lri.camera.localRotation;
        playerLocalRot = lri.player.localRotation;
        //Apply the rotation to camera (vertical look rotation)
        camLocalRot *= Quaternion.Euler(-inputs.x, 0f, 0f);
        playerLocalRot *= Quaternion.Euler(0f, inputs.y, 0f);
        neckLocalRot *= Quaternion.Euler(0f, -inputs.x, 0f);
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
        lri.headAngle = (2.0f * Mathf.Rad2Deg * Mathf.Atan(neckLocalRot.y / neckLocalRot.w));
    }

    private void HandleWallRunningRotations(Vector2 inputs, LookRotationInput lri)
    {
        camLocalRot = lri.camera.localRotation;
        //Apply the rotation to camera (vertical look rotation)
        camLocalRot *= Quaternion.Euler(-inputs.x, inputs.y, 0f);
        neckLocalRot *= Quaternion.Euler(0f, -inputs.x, 0f);
        //Clamp rotation camera and head rotations to not look too far up/down
        camLocalRot = ClampRotationAroundAxis(camLocalRot, "x");
        neckLocalRot = ClampRotationAroundAxis(neckLocalRot, "y");
        //If we are wall-running then add a rotation in the z-axis
        camLocalRot.z = lri.wallRunZRotation;
        if (lri.wallRunZRotation == 0)
        {
            camLocalRot.y = 0;
        }
        //Update the rotation of our player and camera
        lri.camera.localRotation = camLocalRot;
        //Return the angle of our head for the animator
        lri.headAngle = (2.0f * Mathf.Rad2Deg * Mathf.Atan(neckLocalRot.y / neckLocalRot.w));
    }

    private void DoAimAssist(LookRotationInput lri)
    {
        if (lri.aimAssistEnabled)
        {
            //Shoot a thick raycast out from camera forward
            Vector3 v = lri.camera.transform.forward;
            sphereRay = new Ray(lri.camera.transform.position, v);
            RaycastHit[] hits = Physics.SphereCastAll(sphereRay, sphereSize);
            //Check if we hit any players
            Transform hitTransform = FindClosestHitTransform(hits, lri.player);
            if (hitTransform != null && hitTransform.gameObject.tag == "Player")
            {
                lri.lockedOnPlayer = true;
            }
            else
            {
                lri.lockedOnPlayer = false;
            }
        }
    }

    //Find the closest object we hit that is not ourself
    private Transform FindClosestHitTransform(RaycastHit[] hits, Transform player)
    {
        Transform hitTransform = null;
        float distance = 0f;
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform != player
                && !hit.transform.IsChildOf(player)
                && (hitTransform == null || hit.distance < distance))
            {
                hitTransform = hit.transform;
                distance = hit.distance;
            }
        }
        hitDistance = distance;
        return hitTransform;
    }

    public void DrawDebugGizmos(Transform camera)
    {
        Gizmos.color = Color.red;
        float distance = hitDistance == 0 ? 50f : hitDistance;
        Debug.DrawLine(camera.position, camera.position + camera.forward * distance);
        Gizmos.DrawWireSphere(camera.position + camera.forward * distance, sphereSize);
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
}
