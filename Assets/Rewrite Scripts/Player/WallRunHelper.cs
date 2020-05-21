using System.Collections.Generic;
using UnityEngine;

public class WallRunHelper
{
    public bool DoWallRunCheck(IStateParams stateParams, Transform player, Vector3 velocity, bool isWallRunning, bool isGrounded)
    {
        float rayDistance = 1f;
        
        if (!isGrounded)
        {
            var lastHitInfo = stateParams.WallRunHitInfo;
            Vector3 rayPos = new Vector3(player.position.x, player.position.y + 1, player.position.z);
            // Check initialization of wall-running rules
            if (!isWallRunning)
            {
                RaycastHit velocityHitInfo;
                RaycastHit inputHitInfo;
                RaycastHit rightHitInfo;
                RaycastHit leftHitInfo;
                // Create vectors for our raycasts
                Vector3 vDir = new Vector3(velocity.x, 0, velocity.z);
                Vector3 iDir = CreateInputVector(player);
                // Perform the raycasts
                Physics.Raycast(rayPos, vDir, out velocityHitInfo, rayDistance);
                Physics.Raycast(rayPos, iDir, out inputHitInfo, rayDistance);
                Physics.Raycast(rayPos, player.transform.right, out rightHitInfo, rayDistance);
                Physics.Raycast(rayPos, -player.transform.right, out leftHitInfo, rayDistance);
                // Make sure we're not looking too far into the wall
                var lookAngleGood =
                    LookAngleGood(player, vDir, iDir, velocityHitInfo, inputHitInfo, rightHitInfo, leftHitInfo);
                // Bring it all together and decide if we should wall-run
                if (lookAngleGood)
                {
                    if (velocityHitInfo.collider != null || inputHitInfo.collider != null)
                    {
                        stateParams.WallRunHitInfo = velocityHitInfo.collider != null ? velocityHitInfo : inputHitInfo;
                    }
                    else if (rightHitInfo.collider != null || leftHitInfo.collider != null)
                    {
                        stateParams.WallRunHitInfo = rightHitInfo.collider != null ? rightHitInfo : leftHitInfo;
                    }
                    return true;
                }
            }
            // Check continuous wall-running rules.
            else if (lastHitInfo.collider != null)
            {
                // Raycast along the last hit info's normal, in the reverse direction, to see if we're still on a wall.
                RaycastHit wallNormalHitInfo;
                Vector3 rayDir = new Vector3(-lastHitInfo.normal.x, 0, -lastHitInfo.normal.z);
                Physics.Raycast(rayPos, rayDir, out wallNormalHitInfo, rayDistance);
                Debug.DrawRay(rayPos, rayDir, Color.green);
                if (wallNormalHitInfo.collider != null)
                {
                    stateParams.WallRunHitInfo = wallNormalHitInfo;
                    return true;
                }
            }
        }
        // This is here to make sure we're not creating new RaycastHits every frame
        if (stateParams.WallRunHitInfo.collider != null)
        {
            stateParams.WallRunHitInfo = new RaycastHit();
        }
        return false;
    }

    // Checks where we're looking vs which side we're wall-running on
    private bool LookAngleGood(Transform player, Vector3 vDir, Vector3 iDir, RaycastHit velocityHitInfo,
        RaycastHit inputHitInfo, RaycastHit rightHitInfo, RaycastHit leftHitInfo)
    {
        var lookAngleGood = false;
        var normal = Vector3.zero;

        // Caught the wall due to velocity
        if (velocityHitInfo.collider != null)
        {
            // Make sure we're not looking too far into the wall.
            // Make sure out velocity going into the wall is above a threshold.
            normal = velocityHitInfo.normal;
            lookAngleGood = Vector3.Angle(player.forward, normal) < 135f && Vector3.Magnitude(vDir) > 1;
        }
        // Caught the wall due to pushing movement buttons
        else if (inputHitInfo.collider != null)
        {
            // Make sure we're not looking too far into the wall.
            // TODO: check input direction magnitude?...
            normal = inputHitInfo.normal;
            lookAngleGood = Vector3.Angle(player.forward, normal) < 135f;
        }
        // Caught the wall due to left/right body raycasts
        else if (rightHitInfo.collider != null || leftHitInfo.collider != null)
        {
            // Make sure we are looking into the wall.
            // Make sure our velocity is going into the wall.
            // Make sure our velocity magnitude into the wall is above a threshold.
            normal = rightHitInfo.collider != null ? rightHitInfo.normal : leftHitInfo.normal;
            lookAngleGood = Vector3.Angle(player.forward, normal) >= 90f && Vector3.Angle(vDir, normal) >= 90f &&
                            Vector3.Magnitude(vDir) > 1;
        }

        return lookAngleGood;
    }
    
    private Vector3 GetWallRunHitNormal(RaycastHit velocityHitInfo, RaycastHit inputHitInfo, 
        RaycastHit rightHitInfo, RaycastHit leftHitInfo)
    {
        Vector3 hitNormal = Vector3.zero;
        if (velocityHitInfo.collider != null)
        {
            return velocityHitInfo.normal;
        }
        if (inputHitInfo.collider != null)
        {
            return inputHitInfo.normal;
        }
        if (rightHitInfo.collider != null)
        {
            return rightHitInfo.normal;
        }
        if (leftHitInfo.collider != null)
        {
            return leftHitInfo.normal;
        }
        return hitNormal;
    }
    
    public Vector3 CreateInputVector(Transform player)
    {
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;
        return ((player.forward * forwardSpeed) + (player.right * sideSpeed));
    }
}
