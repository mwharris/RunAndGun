using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunController : AbstractBehavior {

	[HideInInspector] public bool wallSticking = false;
    [HideInInspector] public bool wallStickVelocitySet = false;
    
	private bool initWallRun = false;
    private bool wallJumped = false;

	private Vector3 wallRunDirection;
	private Vector3 wallRunNormal;

	private bool wallRunningDisabled = false;
	private float wallRunningDisabledTimer = 0.0f;
	private float wallRunTimer = 0.0f;
	private float wallRunMax = 2.0f;
	private string lastWallName = "";

	private float cameraTotalRotation = 0f;
	private float cameraRotAmount = 0.1f;
	private float cameraRotZ = 0;

	private FXManager fxManager;
	private PlayerJump jumpController;

    private float wallRunAngle1 = 0f;
    private float wallRunAngle2 = 0f;
    private bool wrapAroundRotationCircle = false;
    private bool reclampRotation = false;
    private bool curvedWall = false;

    private FirstPersonController fpsC;

    void Start() 
	{
		//Initialize references to various scripts we need
		fxManager = GameObject.FindObjectOfType<FXManager>();
		jumpController = GetComponent<PlayerJump>();
        fpsC = GetComponent<FirstPersonController>();

    }

	/*
	 * Raycast outwards from the player (left, right, and back) to detect walls.  
	 * If either any are hit while the player is in the air, activate wall-running.
	 */
	public void HandleWallRunning(Vector3 velocity, bool isGrounded) //, ref int jumps)
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
            //Move the player closer to the wall to ensure rotation doesn't cause us to come off the wall
            Vector3 trans = -wallRunHit.normal * 0.29f;
			transform.Translate(trans, Space.World);
            //Set a rotation to make sure we aren't looking too far into the wall
            SetWallRunLookRotation();
        }
		//Continue wall-running if we are already and the rules passed
		else if(wallRunHit.collider != null && isWallRunning())
		{
			//Decrement the running timer
			wallRunTimer -= Time.deltaTime;
			//Project our wall-run direction and store the hit point information
			wallRunDirection = Vector3.ProjectOnPlane(velocity, wallRunHit.normal);
            wallRunNormal = wallRunHit.normal;
        }
		//The rules failed but we're already wall-running
		else if(wallRunHit.collider == null && isWallRunning())
        {
            //Deactivate wall-running
            ResetActiveVars();
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
			//wallRunningDisabledTimer = 0;
		}
        //else 
        //{
        //	wallRunningDisabledTimer -= Time.deltaTime;
        //}
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
        //Scale the Vector up to 14 units if we're holding forward
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
                    Vector3 temp = Quaternion.AngleAxis(dir > 0 ? 90 : -90, Vector3.up) * wallRunNormal * 14;
                    velocity = new Vector3(temp.x, velocity.y, temp.z);
                }
            }
            else
            {
                Vector3 temp = new Vector3(velocity.x, 0, velocity.z);
                Vector3 nVelocity = temp.normalized * 14;
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
        velocity.x = Mathf.Clamp(velocity.x, -14f, 14f);
        velocity.z = Mathf.Clamp(velocity.z, -14f, 14f);
        //Set look rotation clamp angles with a proper velocity
        FindWallRunClampAngles(velocity);
        return velocity;
    }

    //Calculate the angles to clamp the player's look rotation while wall-running
    public void FindWallRunClampAngles(Vector3 v)
    {
        //If we're wall-running backwards we don't want to reclamp
        if (inputState.playerIsWallRunningLeft || inputState.playerIsWallRunningRight || (wallSticking && initWallRun))
        {
            //Get the angles between velocity and North and velocity and South
            Vector3 testV = new Vector3(v.x, 0f, v.z);
            float angle1 = Vector3.Angle(testV, Vector3.forward);
            float angle2 = Vector3.Angle(testV, Vector3.back);
            if (inputState.playerIsWallRunningLeft)
            {
                angle1 = 0;
                angle2 = 180;
            }
            else if (inputState.playerIsWallRunningRight)
            {
                angle1 = 0;
                angle2 = -180;
            }
            //Wall-running on an angled surface (not perfectly N/S or E/W) 
            else if (angle1 > 0 && angle2 > 0)
            {
                //Determine the polarity of these angles using cross product
                Vector3 cross = Vector3.Cross(testV, Vector3.forward);
                if (cross.y > 0)
                {
                    angle1 = -angle1;
                }
                cross = Vector3.Cross(testV, Vector3.back);
                if (cross.y > 0)
                {
                    angle2 = -angle2;
                }
                //Determine whether the -180/+180 point is contained in bounded area
                float normalForwardAngle = Vector3.Angle(wallRunNormal, Vector3.forward);
                float normalBackAngle = Vector3.Angle(wallRunNormal, Vector3.back);
                //If the normal is closer to Back vector then it is contained
                if (normalBackAngle < normalForwardAngle)
                {
                    wrapAroundRotationCircle = true;
                }
            }
            else
            {
                wrapAroundRotationCircle = false;
            }
            wallRunAngle1 = angle1;
            wallRunAngle2 = angle2;
        }
    }

    public void SetWallRunLookRotationInputs(LookRotationInput lri, Camera playerCamera, Vector3 velocity)
    {
        //Calculate the correct camera tilt based on wall-run side
        lri.wallRunZRotation = CalculateCameraTilt(playerCamera);
        //Calculate the clamp angles for look rotation
        lri.wallRunAngle1 = wallRunAngle1;
        lri.wallRunAngle2 = wallRunAngle2;
        //Also pass whether our angle wrap around the +180/-180 threshold
        lri.wrapAround = wrapAroundRotationCircle;
    }

    /**
	 * This function was created to handle a special case.
	 * When we are wall-running, we don't want to be able to look into the wall.
	 * This function will clamp our look rotation so that we cannot look too far towards the wall.
	 */
    public void SetWallRunLookRotation()
	{
		Vector3 testNormal = wallRunNormal;
		Vector3 testForward = transform.forward;
		//Get a vector 90 degrees to the left and right of the normal
		testNormal = Quaternion.Euler(0,90,0) * testNormal;
		//Calculate cross products between where we're looking and these two vectors
		Vector3 cross = Vector3.Cross(testNormal, testForward);
		//Mess with the rotation based on our forward vector relative to the boundaries
		if(cross.y > 0.05)
		{
			//We looked too far left/right, rotate towards the cross product
			Vector3 otherCross = new Vector3();
			if(inputState.playerIsWallRunningRight)
			{ 
				otherCross = Vector3.Cross(Vector3.up, wallRunNormal); 
			}
			if(inputState.playerIsWallRunningLeft)
			{ 
				otherCross = Vector3.Cross(Vector3.up, -wallRunNormal); 
			}
			testForward = Vector3.RotateTowards(testForward, otherCross, 10, 0.0f);
			transform.rotation = Quaternion.LookRotation(new Vector3(testForward.x, 0, testForward.z));
		}
	}

	//Tilt the camera left or right depending on which side we are wall-running.
	//No tilt while facing directly away from the wall.
	public float CalculateCameraTilt(Camera playerCamera)
    {
        float lerpedRot = 0f;
        if (!inputState.playerIsGrounded)
        {
            //Wall-running left, tilt right
            if (inputState.playerIsWallRunningLeft)
            {
                lerpedRot = Mathf.Lerp(playerCamera.transform.localRotation.z, -cameraRotAmount, 8 * Time.deltaTime);
            }
            //Wall-running right, tilt left
            else if (inputState.playerIsWallRunningRight)
            {
                lerpedRot = Mathf.Lerp(playerCamera.transform.localRotation.z, cameraRotAmount, 8 * Time.deltaTime);
            }
            //Facing directly away from the wall, no rotation
            else
            {
                lerpedRot = Mathf.Lerp(cameraRotZ, 0f, 8 * Time.deltaTime);
            }
        }
        else 
        {
            cameraRotZ = 0;
        }
        cameraRotZ = lerpedRot;
        return cameraRotZ;
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
            Physics.Raycast(transform.position, pushDir, out vHit, 0.825f);
            Physics.Raycast(transform.position, transform.right, out rHit, rayDistance);
            Physics.Raycast(transform.position, -transform.right, out lHit, rayDistance);
            Physics.Raycast(transform.position, -transform.forward, out bHit, rayDistance);

            //Check the angle between our velocity any wall we may have impacted
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
			bool rDoubleJumpCheck = DoubleWallRunCheck(rHit); //rHit.collider != null && ((wallJumped && lastWallName != rHit.collider.name) || (!wallJumped && !wallRunningDisabled));//			
			bool lDoubleJumpCheck = DoubleWallRunCheck(lHit); //lHit.collider != null && ((wallJumped && lastWallName != lHit.collider.name) || (!wallJumped && !wallRunningDisabled));//
			bool bDoubleJumpCheck = DoubleWallRunCheck(bHit); //bHit.collider != null && ((wallJumped && lastWallName != bHit.collider.name) || (!wallJumped && !wallRunningDisabled));//DoubleWallRunCheck(bHit);

			//Check if we should activate wall-running - right raycast
			if (lookAngleGood && rHitValid && rDoubleJumpCheck && (rightGood || isWallRunning())) 
			{
				//Flag if we are initializing the wall-run
				if (!isWallRunning())
                {
					initWallRun = true;
				}
                //Re-clamp rotation angles if we transition from another wall-run
                else if (inputState.playerIsWallRunningLeft || inputState.playerIsWallRunningBack) 
                {
                    reclampRotation = true;
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
                //Re-clamp rotation angles if we transition from another wall-run
                else if (inputState.playerIsWallRunningRight || inputState.playerIsWallRunningBack)
                {
                    reclampRotation = true;
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
                        Debug.Log("Caught!");
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
        //Disable wall-running
        ResetActiveVars();
        //Flag us as having wall jumped
        wallJumped = true;
        //First rotate the player object towards camera forward
        RotatePlayerBody(playerCamera);
        //Next rotate our velocity in a direction dependent on controller input
        velocity = RotatePlayerVelocity(velocity, cameraRot);
        fpsC.drawMe = velocity;
        //Jump upwards
        velocity.y = jumpSpeed;
        return velocity;
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
        Vector3 targetDir = new Vector3(leftAxis + rightAxis, 0, forwardAxis + backwardAxis) * 14f;
        targetDir = cameraRot * targetDir;
        //Disable wall-running
        ResetActiveVars();
        //Find the angle, in radians, between our target direction and current direction
        Vector3 dir = wallRunNormal * 14f;
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
        reclampRotation = false;
        wrapAroundRotationCircle = false;
        wallRunAngle1 = 0f;
        wallRunAngle2 = 0f;
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