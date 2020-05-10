using UnityEngine;

public class WallRunHelper
{
    public bool DoWallRunCheck(IStateParams stateParams, Transform player, Vector3 velocity, bool isWallRunning, bool isGrounded)
    {
        float rayDistance = 1f;
        
        if (!isGrounded)
        {
            var lastHitInfo = stateParams.WallRunHitInfo;
            // Check initialization of wall-running rules
            if (!isWallRunning)
            {
                RaycastHit velocityHitInfo;
                RaycastHit inputHitInfo;
                // Create vectors for our raycasts
                Vector3 rayPos = new Vector3(player.position.x, player.position.y + 1, player.position.z);
                Vector3 vDir = new Vector3(velocity.x, 0, velocity.z);
                Vector3 iDir = CreateInputVector(player);
                // Perform the raycasts
                Physics.Raycast(rayPos, vDir, out velocityHitInfo, rayDistance);
                Physics.Raycast(rayPos, iDir, out inputHitInfo, rayDistance);
                // Make sure we're not looking too far into the wall
                var lookAngleGood = LookAngleGood(player, velocityHitInfo, inputHitInfo);
                // Bring it all together and decide if we should wall-run
                if (lookAngleGood && (velocityHitInfo.collider != null || inputHitInfo.collider != null))
                {
                    stateParams.WallRunHitInfo = velocityHitInfo.collider != null ? velocityHitInfo : inputHitInfo;
                    return true;
                }
            }
            // Check continuous wall-running rules.
            else if (lastHitInfo.collider != null)
            {
                // Raycast along the last hit info's normal, in the reverse direction, to see if we're still on a wall.
                RaycastHit wallNormalHitInfo;
                Vector3 rayDir = new Vector3(-lastHitInfo.normal.x, 0, -lastHitInfo.normal.z);
                Physics.Raycast(player.position, rayDir, out wallNormalHitInfo, rayDistance);
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
        // If we reached this point we shouldn't be wall-running
        return false;
    }

    private bool LookAngleGood(Transform player, RaycastHit velocityHitInfo, RaycastHit inputHitInfo)
    {
        // Don't wall run if we're looking directly into the wall
        var lookAngleGood = false;
        var normal = GetWallRunHitNormal(velocityHitInfo, inputHitInfo);
        if (normal != Vector3.zero)
        {
            lookAngleGood = Vector3.Angle(player.forward, normal) < 135f;
        }
        return lookAngleGood;
    }
    
    private Vector3 GetWallRunHitNormal(RaycastHit velocityHitInfo, RaycastHit inputHitInfo)
    {
        Vector3 hitNormal = Vector3.zero;
        if (velocityHitInfo.collider != null)
        {
            hitNormal = velocityHitInfo.normal;
        }
        else if (inputHitInfo.collider != null)
        {
            hitNormal = inputHitInfo.normal;
        }
        return hitNormal;
    }
    
    private Vector3 CreateInputVector(Transform player)
    {
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;
        return ((player.forward * forwardSpeed) + (player.right * sideSpeed));
    }
}
