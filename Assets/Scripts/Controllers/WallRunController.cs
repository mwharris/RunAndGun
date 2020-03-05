using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunController : AbstractBehavior {

	[HideInInspector] public bool wallSticking = false;
    [HideInInspector] public bool wallStickVelocitySet = false;
    [HideInInspector] public bool wallJumped = false;
    
	private bool initWallRun = false;

	private Vector3 wallRunDirection;
	private Vector3 wallRunNormal;
    private string wallRunTag;

    private bool wallRunningDisabled = false;
	private float wallRunningDisabledTimer = 0.0f;
	private float wallRunTimer = 0.0f;
	private float wallRunMax = 2.0f;
	private string lastWallName = "";

	private float cameraRotAmount = 0.1f;

	private FXManager fxManager;
	private PlayerJump jumpController;
    
    private bool wrapAroundRotationCircle = false;

    private float _wallRunSpeed = 16f;
    private float _wallJumpSpeed = 17.6f;

    void Start() 
	{
		//Initialize references to various scripts we need
		fxManager = GameObject.FindObjectOfType<FXManager>();
		jumpController = GetComponent<PlayerJump>();
    }

	/*
	 * Raycast outwards from the player (left, right, and back) to detect walls.  
	 * If any are hit while the player is in the air, activate wall-running.
	 */
	public void HandleWallRunning(Vector3 velocity, bool isGrounded, PlayerBodyData playerBodyData)
	{
		//Raycast in several directions to see if we are wall-running
		RaycastHit wallRunHit = DoWallRunCheck(velocity, isGrounded);

        //Start wall-running if we hit something and we're not already wall-running
        if (wallRunHit.collider != null && initWallRun)
		{
			//Start the timer
			wallRunTimer = wallRunMax;
            //Play a networked landing sound
			fxManager.GetComponent<PhotonView>().RPC("LandingFX", PhotonTargets.All, this.transform.position);
			//Reset the jump counter
			jumpController.ResetJumps();
			//Project our wall-run direction and store the hit point information
			wallRunDirection = Vector3.ProjectOnPlane(velocity, wallRunHit.normal);
			wallRunNormal = wallRunHit.normal;
            wallRunTag = wallRunHit.collider.gameObject.tag;
            //Move the player closer to the wall to ensure rotation doesn't cause us to come off the wall
            Vector3 trans = -wallRunHit.normal * 0.29f;
			transform.Translate(trans, Space.World);
            //Make sure we are rotated parallel to the wall
            SetWallRunLookRotation(true);
        }
		//Continue wall-running if we are already and the rules passed
		else if(wallRunHit.collider != null && isWallRunning())
		{
			//Decrement the running timer
			wallRunTimer -= Time.deltaTime;
			//Project our wall-run direction and store the hit point information
			wallRunDirection = Vector3.ProjectOnPlane(velocity, wallRunHit.normal);
            wallRunNormal = wallRunHit.normal;
            wallRunTag = wallRunHit.collider.gameObject.tag;
            //Make sure we are rotated parallel to the wall
            SetWallRunLookRotation(false);
        }
		//The rules failed but we're already wall-running
		else if(wallRunHit.collider == null && isWallRunning())
        {
            DeactivateWallRunning(playerBodyData.playerCamera);
        }

		//Enable wall-running if we touch the ground or the timer runs out
		if(isGrounded) 
		{
			lastWallName = "";
			wallJumped = false;
		}
		if(isGrounded)
		{
			wallRunningDisabled = false;
			wallRunningDisabledTimer = 0;
		}
        else 
        {
        	wallRunningDisabledTimer -= Time.deltaTime;
        }
    }

    /*
	 * Handle velocity calculations while the player is wall-running
	 */
    public Vector3 SetWallRunVelocity(Vector3 velocity, bool isWPressed, bool isSPressed)
    {
        //Check the angle between where we are looking and the wall's normal
        Vector3 testV = new Vector3(velocity.x, 0, velocity.z);
        //Flip the velocity if we're facing the opposite direction of our velocity
        if (Vector3.Angle(transform.forward, testV) > 110)
        {
            wallRunDirection = new Vector3(-wallRunDirection.x, wallRunDirection.y, -wallRunDirection.z);
        }
        //The velocity should be in the direction we're facing, parallel to the wall
        velocity = new Vector3(wallRunDirection.x, velocity.y, wallRunDirection.z);
        //Reset the y velocity if we're falling but we've activated wall-running
        if (initWallRun)
        {
            if (velocity.y < 0)
            {
                velocity.y = 0;
            }
            initWallRun = false;
        }
        //Scale the Vector up to 21 units if we're holding forward
        if (isWPressed)
        {
            //Special case: our wallRunDirection / velocity in X and Z is 0
            if (velocity.x == 0f && velocity.z == 0f)
            {
                //Determine which direction we are looking relative to the wallRunNormal
                float angle = Vector3.Angle(transform.forward, wallRunNormal);
                float dir = AngleDir(wallRunNormal, transform.forward, transform.up);
                //Depending on which way we are looking, start moving in that direction
                if (angle > 20)
                {
                    Vector3 temp = Quaternion.AngleAxis(dir > 0 ? 90 : -90, Vector3.up) * wallRunNormal * _wallRunSpeed;
                    velocity = new Vector3(temp.x, velocity.y, temp.z);
                }
            }
            else
            {
                Vector3 temp = new Vector3(velocity.x, 0, velocity.z);
                Vector3 nVelocity = temp.normalized * _wallRunSpeed;
                nVelocity.y = velocity.y;
                velocity = nVelocity;
            }
        }
        //Slow us down 25% if we're holding backwards
        if (isSPressed)
        {
            velocity.x *= 0.75f;
            velocity.z *= 0.75f;
        }
        //Make sure we don't move too fast
        velocity.x = Mathf.Clamp(velocity.x, -_wallRunSpeed, _wallRunSpeed);
        velocity.z = Mathf.Clamp(velocity.z, -_wallRunSpeed, _wallRunSpeed);
        return velocity;
    }

    public void SetWallRunLookRotationInputs(LookRotationInput lri, Camera playerCamera, Vector3 velocity)
    {
        //Calculate the correct camera tilt based on wall-run side
        lri.WallRunZRotation = CalculateCameraTilt(playerCamera);
        //Also pass whether our angle wrap around the +180/-180 threshold
        lri.WrapAround = wrapAroundRotationCircle;
    }

    /**
	 * Handle lerping our look rotation away from the wall when we initiate wall-running.
	 * Also handles lerping our look rotation when wall-running on Curved Walls.
	 */
    public void SetWallRunLookRotation(bool initial)
    {
        //Calculate cross product between up and our wallrun wall's normal.
        //This will yield a vector parallel to the wall we are currently running on.
        Vector3 cross = new Vector3();
		if(inputState.playerIsWallRunningRight)
		{
            cross = Vector3.Cross(Vector3.up, wallRunNormal); 
		}
		if(inputState.playerIsWallRunningLeft)
		{
            cross = Vector3.Cross(Vector3.up, -wallRunNormal); 
		}
        Vector3 crossNoUp = new Vector3(cross.x, 0, cross.z);
        
        //Determine if we are looking into the wall when we initiate wall-running
        Vector3 perp = Vector3.Cross(cross, transform.forward);
        float dir = Vector3.Dot(perp, Vector3.up);
        
        //Make sure we're not look rotating to a zero vector
        if (crossNoUp != Vector3.zero && crossNoUp.magnitude > Single.Epsilon)        
        {
	        Quaternion newQuat = Quaternion.LookRotation(crossNoUp);
	        //Lerp our body rotation parallel to the wall when running along a curved surface.
	        //This handles when we reach a new surface normal on the curved wall.
	        if (initial && wallRunTag == "CurvedWall")
	        {
		        transform.rotation = Quaternion.Lerp(transform.rotation, newQuat, Time.deltaTime * 10f);
	        }
	        //Lerp our body rotation parallel to flat walls only when we're looking into the wall.
	        //Always perform this lerp on curved walls.
	        else if (wallRunTag == "CurvedWall"
	                 || dir > 0.0 && inputState.playerIsWallRunningRight
	                 || dir < 0.0 && inputState.playerIsWallRunningLeft)
	        {
		        transform.rotation = Quaternion.Lerp(transform.rotation, newQuat, Time.deltaTime * 8f);
	        }
        }
        else
        {
	        Debug.LogWarning("WallRunController.SetWallRunLookRotation() : Attempted to LookRotation a zero vector!");
        }
	}

	//Tilt the camera left or right depending on which side we are wall-running.
	//No tilt while facing directly away from the wall.
	public float CalculateCameraTilt(Camera playerCamera)
    {
        float lerpedRot = 0f;
        float rotSpeed = 4 * Time.deltaTime;
        if (isWallRunning())
        {
            //Wall-running left, tilt right
            if (inputState.playerIsWallRunningLeft)
            {
                lerpedRot = Mathf.Lerp(playerCamera.transform.localRotation.z, -cameraRotAmount, rotSpeed);
            }
            //Wall-running right, tilt left
            else if (inputState.playerIsWallRunningRight)
            {
                lerpedRot = Mathf.Lerp(playerCamera.transform.localRotation.z, cameraRotAmount, rotSpeed);
            }
            //Facing directly away from the wall, no rotation
            else
            {
                lerpedRot = Mathf.Lerp(playerCamera.transform.localRotation.z, 0f, rotSpeed);
            }
        }
        else if (!inputState.playerIsGrounded)
        {
            lerpedRot = Mathf.Lerp(playerCamera.transform.localRotation.z, 0f, rotSpeed);
        }
        else 
        {
            lerpedRot = 0;
        }
        return lerpedRot;
	}
		
	//Helper function to return if we are wall-running in any direction
	public bool isWallRunning()
	{
		if(inputState.playerIsWallRunningBack || inputState.playerIsWallRunningLeft || inputState.playerIsWallRunningRight)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	//Perform raycast to check wall-running rules; return the hit location
	public RaycastHit DoWallRunCheck(Vector3 velocity, bool isGrounded)
    {
        //bool currentlyWallRunning = isWallRunning() && (wallRunTimer > 0 || wallSticking);
        bool currentlyWallRunning = isWallRunning() || wallSticking;
        //If we're not grounded and we haven't disabled wall-running
        if (!isGrounded && (currentlyWallRunning || !isWallRunning()))
		{
			//Raycast in several directions to see if we are wall-running
			RaycastHit vHit;
			RaycastHit rHit;
			RaycastHit lHit;
			RaycastHit bHit;
			
			Vector3 pushDir = new Vector3(velocity.x, 0, velocity.z);
            float rayDistance = isWallRunning() ? 1.5f : 0.825f;
            Vector3 rayPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            
            // Layer mask so we don't wall-run on non-wall-runnable surfaces
            int layerMask = 1 << 10;
            layerMask = ~layerMask;
            
            Physics.Raycast(rayPos, pushDir, out vHit, 0.825f);
            Physics.Raycast(rayPos, transform.right, out rHit, rayDistance, layerMask);
            Physics.Raycast(rayPos, -transform.right, out lHit, rayDistance, layerMask);
            Physics.Raycast(rayPos, -transform.forward, out bHit, rayDistance, layerMask);

            //Check the angle between our velocity and any wall we may have impacted
            bool rightGood = false;
			bool leftGood = false;
            bool backGood = false;
            Vector3 testV = new Vector3(velocity.x, 0, velocity.z);
			if(Vector3.Angle(testV, rHit.normal) > 90 && Vector3.Magnitude(testV) > 1)
			{
				rightGood = true;	
			}
			if(Vector3.Angle(testV, lHit.normal) > 90 && Vector3.Magnitude(testV) > 1)
			{
				leftGood = true;
            }
            if (Vector3.Angle(testV, bHit.normal) > 90 && Vector3.Magnitude(testV) > 1)
            {
                backGood = true;
            }
            //Also check the angle between our look direction and our velocity
            bool lookAngleGood = false;
			float lookVeloAngle = Vector3.Angle(transform.forward, testV);
			if(lookVeloAngle <= 90) 
			{
				lookAngleGood = true;
			}
			//Determine if either raycast hits are valid
			bool rHitValid = rHit.collider != null && rHit.collider.tag != "Player" && rHit.collider.tag != "Invisible";
			bool lHitValid = lHit.collider != null && lHit.collider.tag != "Player" && lHit.collider.tag != "Invisible";
			bool bHitValid = bHit.collider != null && bHit.collider.tag != "Player" && bHit.collider.tag != "Invisible";
			//Rule checks to prevent jumping onto the same wall
			bool rDoubleJumpCheck = DoubleWallRunCheck(rHit);
			bool lDoubleJumpCheck = DoubleWallRunCheck(lHit);
            bool bDoubleJumpCheck = DoubleWallRunCheck(bHit);

			//Check if we should activate wall-running - right raycast
			if (lookAngleGood && rHitValid && rDoubleJumpCheck && (rightGood || isWallRunning())) 
			{
				//Flag if we are initializing the wall-run
				if (!isWallRunning())
                {
					initWallRun = true;
				}
                //Flag the side we are wall-running
                inputState.playerIsWallRunningLeft = false;
                inputState.playerIsWallRunningRight = true;
                inputState.playerIsWallRunningBack = false;
				//Store the name of the wall we ran on
				lastWallName = rHit.collider.name;
				//Reset the wall jumped flag
				wallJumped = false;
                return rHit;
			} 
			//Left raycast
			else if (lookAngleGood && lHitValid && lDoubleJumpCheck && (leftGood || isWallRunning())) 
			{
				//Flag if we are initializing the wall-run
				if (!isWallRunning())
                {
					initWallRun = true;
                }
                //Flag the side we are wall-running
                inputState.playerIsWallRunningLeft = true;
                inputState.playerIsWallRunningRight = false;
                inputState.playerIsWallRunningBack = false;
				//Store the name of the wall we ran on
				lastWallName = lHit.collider.name;
				//Reset the wall jumped flag
				wallJumped = false;
                return lHit;
			} 
			//Backwards raycast
			else if (bHitValid && bHitValid && bDoubleJumpCheck && (backGood || isWallRunning())) 
			{
				//Flag if we are initializing the wall-run
				if (!isWallRunning())
                {
					initWallRun = true;
                }
                //Flag the side we are wall-running
                inputState.playerIsWallRunningLeft = false;
                inputState.playerIsWallRunningRight = false;
                inputState.playerIsWallRunningBack = true;
				//Store the name of the wall we ran on
				lastWallName = bHit.collider.name;
				//Reset the wall jumped flag
				wallJumped = false;
                return bHit;
			}
        }
		//else if(wallRunTimer <= 0 && !wallSticking)
        else if (!wallSticking)
        {
			wallRunningDisabled = true;
		}

		return new RaycastHit();
	}

	//Checks to make sure we didn't jump off and back onto the same wall
	public bool DoubleWallRunCheck(RaycastHit hit){
		//If the raycast hit a wall...
		if(hit.collider != null && hit.collider.tag != "Player"){
			//...and we wall jumped onto a new wall
			if(wallJumped && lastWallName != hit.collider.name){
				//Allow wall running
				return true;
			}
			//...and we did not wall jump
			else if(!wallJumped){
				//...and we fell off a wall onto a new wall
				if(wallRunningDisabled && lastWallName != hit.collider.name){
					//Allow wall running
					wallRunningDisabled = false;
					return true;
				}
				//..and wall running is not disabled
				else if(!wallRunningDisabled){
                    //Make sure we are not getting caught on another face of the same object
                    Vector3 testOldNormal = new Vector3(wallRunNormal.x, 0f, wallRunNormal.z);
                    Vector3 testNewNormal = new Vector3(hit.normal.x, 0f, hit.normal.z);
                    if (Vector3.Angle(testOldNormal, testNewNormal) >= 90 && lastWallName == hit.transform.name)
                    {
                        return false;
                    }
                    //Otherwise we're good to go
                    return true;
				}
			}
		}
		//Did not pass rule checks
		return false;
	}

	//This function handles jumping while wall-running.  
	//Depending on any keys held down, the jump will be angled away from the wall.
    public Vector3 WallJump(Vector3 velocity, float jumpSpeed, Transform playerCamera)
    {
        Quaternion cameraRot = playerCamera.rotation;
        //Stop wall-running
        DeactivateWallRunning(playerCamera);
        //Flag us as having wall jumped
        wallJumped = true;
        //Next rotate our velocity in a direction dependent on controller input
        velocity = RotatePlayerVelocity(velocity, cameraRot);
        //Jump upwards
        velocity.y = jumpSpeed;
        return velocity;
    }

    private void DeactivateWallRunning(Transform playerCamera)
    {
        //Disable wall-running
        ResetActiveVars();
        //First rotate the player object towards camera forward
        RotatePlayerBody(playerCamera);
    }

    //Rotate our player body to match the camera forward
    private void RotatePlayerBody(Transform playerCamera)
    {
        Quaternion origCamRot = playerCamera.rotation;
        //We want to match the rotation of the player camera
        Vector3 targetDir = new Vector3(playerCamera.forward.x, 0f, playerCamera.forward.z);
        //Calculate radians between current and target rotation
        float degrees = Vector3.Angle(transform.forward, targetDir);
        float radians = degrees * Mathf.Deg2Rad;
        //Perform the actual rotation
        Vector3 newForward = Vector3.RotateTowards(transform.forward, targetDir, radians, 0.0f);
        transform.rotation = Quaternion.LookRotation(newForward);
        //The above rotates our camera as well, so rotate it back
        playerCamera.rotation = origCamRot;
    }

    //Rotate our velocity to match direction of new forward taking into account player input
    private Vector3 RotatePlayerVelocity(Vector3 velocity, Quaternion cameraRot)
    {
        //Get keyboard or controller input using raw axis values
        float forwardAxis = inputState.GetButtonValue(inputs[0]);
        float backwardAxis = inputState.GetButtonValue(inputs[1]);
        float leftAxis = inputState.GetButtonValue(inputs[2]);
        float rightAxis = inputState.GetButtonValue(inputs[3]);
        //Determine our target jump direction relative to player based on input
        Vector3 targetDir = new Vector3(leftAxis + rightAxis, 0, forwardAxis + backwardAxis) * _wallJumpSpeed;
        targetDir = cameraRot * targetDir;
        //Disable wall-running
        ResetActiveVars();
        //Find the angle, in radians, between our target direction and current direction
        Vector3 dir = wallRunNormal * _wallJumpSpeed;
        float degrees = Vector3.Angle(dir, targetDir);
        float radians = degrees * Mathf.Deg2Rad;
        //Rotate the current direction the amount of radians determined above
        velocity = Vector3.RotateTowards(dir, targetDir, radians, 0.0f);
        velocity *= 1.2f;
        return velocity;
    }

	//Stick to the wall if we aim while wall-running 
    public void HandleWallSticking()
	{
		if(inputState.playerIsAiming && isWallRunning())
		{
			wallSticking = true;
			wallRunTimer = 0f;
            initWallRun = false;
		}
		else
		{
			wallSticking = false;
		}
	}

    //Helper function to reset variables activate while wall-running
    private void ResetActiveVars()
    {
        inputState.playerIsWallRunningLeft = false;
        inputState.playerIsWallRunningRight = false;
        inputState.playerIsWallRunningBack = false;
        wrapAroundRotationCircle = false;
        wallRunTimer = 0.0f;
    }

    //Returns -1 when to the left, 1 to the right, and 0 for forward/backward
    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }
}