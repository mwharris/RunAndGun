using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WallRunHelper
{
    public bool DoWallRunCheck(IStateParams stateParams, Transform player, Vector3 velocity, bool isWallRunning, bool isGrounded)
    {
        float rayDistance = 1f;
        int layerMask = ~(1 << 10);
        
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
                Physics.Raycast(rayPos, vDir.normalized, out velocityHitInfo, rayDistance, layerMask);
                Physics.Raycast(rayPos, iDir.normalized, out inputHitInfo, rayDistance, layerMask);
                Physics.Raycast(rayPos, player.transform.right, out rightHitInfo, rayDistance, layerMask);
                Physics.Raycast(rayPos, -player.transform.right, out leftHitInfo, rayDistance, layerMask);
                // Make sure we're not looking too far into the wall
                var lookAngleGood =
                    LookAngleGood(player, vDir, iDir, velocityHitInfo, inputHitInfo, rightHitInfo, leftHitInfo);
                // Bring it all together and decide if we should wall-run
                if (lookAngleGood)
                {
                    if (velocityHitInfo.collider != null || inputHitInfo.collider != null)
                    {
                        stateParams.WallRunHitInfo = velocityHitInfo.collider != null ? velocityHitInfo : inputHitInfo;
                        if (rightHitInfo.collider == null && leftHitInfo.collider == null)
                        {
                            Vector3 dir = velocityHitInfo.collider != null ? vDir : iDir;
                            CalculateWallRunSide(stateParams, player.transform.forward);
                        }
                        else
                        {
                            stateParams.WallRunningRight = rightHitInfo.collider != null;
                            stateParams.WallRunningLeft = leftHitInfo.collider != null;
                        }
                    }
                    else if (rightHitInfo.collider != null || leftHitInfo.collider != null)
                    {
                        stateParams.WallRunHitInfo = rightHitInfo.collider != null ? rightHitInfo : leftHitInfo;
                        stateParams.WallRunningRight = rightHitInfo.collider != null;
                        stateParams.WallRunningLeft = leftHitInfo.collider != null;
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
                Physics.Raycast(rayPos, rayDir, out wallNormalHitInfo, rayDistance, layerMask);
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

    // Calculate the wall-run side using Cross and Dot product
    private void CalculateWallRunSide(IStateParams stateParams, Vector3 forward)
    {
        Vector3 normal = stateParams.WallRunHitInfo.normal;
        Vector3 normalNoY = new Vector3(normal.x, 0, normal.z);
        // Take the cross product of the wall's normal (A) and our forward direction (B).
        // Yields a vector pointing Up if B is to the right of A or Down if B is to the left of A.
        Vector3 cross = Vector3.Cross(normalNoY, forward);
        // Use Dot product to determine if the vector is pointing Up or Down.
        float dot = Vector3.Dot(cross, Vector3.up);
        // If the result is negative then it's pointing down
        if (dot < 0)
        {
            stateParams.WallRunningLeft = true;
            stateParams.WallRunningRight = false;
        }
        // If the result is positive then it's pointing up
        else if (dot > 0)
        {
            stateParams.WallRunningLeft = false;
            stateParams.WallRunningRight = true;
        }
    }

    public Vector3 CreateInputVector(Transform player)
    {
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;
        return ((player.forward * forwardSpeed) + (player.right * sideSpeed));
    }
}
