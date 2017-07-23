using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunController : MonoBehaviour {

	[HideInInspector] public bool wallSticking = false;

	private bool wallRunningLeft = false;
	private bool wallRunningRight = false;
	private bool wallRunningBack = false;
	private bool initWallRun = false;
	private bool wallJumped = false;
	private Vector3 wallRunDirection;
	private Vector3 wallRunNormal;
	private bool wallRunningDisabled = false;
	private float wallRunningDisabledTimer = 0.0f;
	private float wallRunTimer = 0.0f;
	private float wallRunMax = 2.0f;
	private string lastWallName = "";

	[SerializeField] private float cameraTotalRotation = 0f;
	[SerializeField] private float cameraRotAmount = 15f;
	[SerializeField] private float cameraRotZ = 0;

	private FXManager fxManager;

	void Start() 
	{
		//Initialize a reference to the FX Manager
		fxManager = GameObject.FindObjectOfType<FXManager>();
	}

	/*
	 * Raycast outwards from the player (left, right, and back) to detect walls.  
	 * If either any are hit while the player is in the air, activate wall-running.
	 */
	public void HandleWallRunning(Vector3 velocity, GameObject playerBody, bool isGrounded, ref int jumps)
	{
		//Raycast in several directions to see if we are wall-running
		RaycastHit wallRunHit = DoWallRunCheck(velocity, playerBody, isGrounded);

		//Start wall-running if we hit something and we're not already wall-running
		if(wallRunHit.collider != null && initWallRun)
		{
			//Start the timer
			wallRunTimer = wallRunMax;
			//Play a networked landing sound
			fxManager.GetComponent<PhotonView>().RPC("LandingFX", PhotonTargets.All, this.transform.position);
			//Reset the jump counter
			jumps = 2;
			//Project our wall-run direction and store the hit point information
			wallRunDirection = Vector3.ProjectOnPlane(velocity, wallRunHit.normal);
			wallRunNormal = wallRunHit.normal;
			//Move the player closer to the wall to ensure rotation doesn't cause us to come off the wall
			this.transform.Translate((-wallRunHit.normal/2), Space.World);
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
			wallRunningLeft = false;
			wallRunningRight = false;
			wallRunningBack = false;
			wallRunTimer = 0.0f;
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
		if(Vector3.Angle(transform.forward, testV) > 90)
		{
			wallRunDirection = new Vector3(-wallRunDirection.x, wallRunDirection.y, -wallRunDirection.z);
		}
		//The velocity should be in the direction we're facing, parallel to the wall
		velocity = new Vector3(wallRunDirection.x, velocity.y, wallRunDirection.z);
		//If we just initialized wall-running
		if(initWallRun)
		{
			//Reset the y velocity if we're falling but we've activated wall-running
			if(velocity.y < 0)
			{
				velocity.y = 0;	
			}
			//Marked that we're done initializing the wall-run
			initWallRun = false;
		}
		//W will speed up movement slightly and S will slow down movement slightly
		float scaleVal = 0.0f;
		//Scale the Vector up to 14 units if we're holding forward
		if(isWPressed)
		{
			Vector3 temp = new Vector3(velocity.x, 0, velocity.z);
			Vector3 nVelocity = temp.normalized * 14;
			//Keep the y-velocity the same
			nVelocity.y = velocity.y;
			velocity = nVelocity;
		}
		//Slow us down 25% if we're holding backwards
		if(isSPressed)
		{
			scaleVal = 0.75f;
		}
		//Scale the X and Z values by the scale value and clamp
		if(scaleVal > 0)
		{
			velocity.x *= scaleVal;
			velocity.z *= scaleVal;
		}
		//Make sure we don't move too fast
		velocity.x = Mathf.Clamp(velocity.x, -14f, 14f);
		velocity.z = Mathf.Clamp(velocity.z, -14f, 14f);
		return velocity;
	}

	/**
	 * This function was created to handle a special case.
	 * When we are wall-running, we don't want to be able to into the wall.
	 * This function will clamp our look rotation so that we cannot look too far towards the wall.
	 */
	public bool SetWallRunLookRotation()
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
			if(wallRunningRight)
			{ 
				otherCross = Vector3.Cross(Vector3.up, wallRunNormal); 
			}
			if(wallRunningLeft)
			{ 
				otherCross = Vector3.Cross(Vector3.up, -wallRunNormal); 
			}
			float degrees = Vector3.Angle(testForward, otherCross);
			float rads = degrees * Mathf.Deg2Rad;
			testForward = Vector3.RotateTowards(testForward, otherCross, 10, 0.0f);
			transform.rotation = Quaternion.LookRotation(new Vector3(testForward.x, 0, testForward.z));
			//Flag that we handled the look rotation
			return true;
		}
		else 
		{
			//Flag that we didn't handle the look rotation
			return false;
		}
	}

	/**
	 * Tilt the camera left or right depending on which side we are wall-running.
	 * No tilt while facing directly away from the wall.
	 */
	public float CalculateCameraTilt(Camera playerCamera)
	{
		//Wall-running left or right, tilt in the opposite direction
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
		//Facing directly away from the wall, no rotation
		else 
		{
			cameraTotalRotation = 0f;
			cameraRotZ = 0;
		}
		return cameraRotZ;
	}
		
	/**
	 * Helper function to return if we are wall-running in any direction
	 */
	public bool isWallRunning()
	{
		if(wallRunningBack || wallRunningLeft || wallRunningRight)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/**
	 * Perform raycast to check wall-running rules; return the hit location
	 */
	public RaycastHit DoWallRunCheck(Vector3 velocity, GameObject playerBody, bool isGrounded)
	{
		//If we're not grounded and we haven't disabled wall-running
		if(!isGrounded && ((isWallRunning() && (wallRunTimer > 0 || wallSticking)) || !isWallRunning()))
		{
			//Raycast in several directions to see if we are wall-running
			RaycastHit vHit;
			RaycastHit rHit;
			RaycastHit lHit;
			RaycastHit bHit;
			Vector3 pushDir = new Vector3(velocity.x, 0, velocity.z);
			Physics.Raycast(playerBody.transform.position, pushDir, out vHit, 0.825f);
			Physics.Raycast(playerBody.transform.position, playerBody.transform.right, out rHit, 0.825f);
			Physics.Raycast(playerBody.transform.position, -playerBody.transform.right, out lHit, 0.825f);
			Physics.Raycast(playerBody.transform.position, -playerBody.transform.forward, out bHit, 0.825f);

			//Check the angle between our velocity any wall we may have impacted
			bool rightGood = false;
			bool leftGood = false;
			Vector3 testV = new Vector3(velocity.x, 0, velocity.z);
			if(Vector3.Angle(testV, rHit.normal) > 90 && Vector3.Magnitude(testV) > 1)
			{
				rightGood = true;	
			}
			if(Vector3.Angle(testV, lHit.normal) > 90 && Vector3.Magnitude(testV) > 1)
			{
				leftGood = true;
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
				if (!isWallRunning()) {
					initWallRun = true;
				}
				//Flag the side we are wall-running
				wallRunningLeft = false;
				wallRunningRight = true;
				wallRunningBack = false;
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
				if (!isWallRunning()) {
					initWallRun = true;
				}
				//Flag the side we are wall-running
				wallRunningLeft = true;
				wallRunningRight = false;
				wallRunningBack = false;
				//Store the name of the wall we ran on
				lastWallName = lHit.collider.name;
				//Reset the wall jumped flag
				wallJumped = false;
				return lHit;
			} 
			//Backwards raycast
			else if (bHitValid && bDoubleJumpCheck) 
			{
				//Flag if we are initializing the wall-run
				if (!isWallRunning()) {
					initWallRun = true;
				}
				//Flag the side we are wall-running
				wallRunningLeft = false;
				wallRunningRight = false;
				wallRunningBack = true;
				//Store the name of the wall we ran on
				lastWallName = bHit.collider.name;
				//Reset the wall jumped flag
				wallJumped = false;
				return bHit;
			}
		}
		else if(wallRunTimer <= 0 && !wallSticking)
		{
			wallRunningDisabled = true;
		}

		return new RaycastHit();
	}

	/**
	 * Checks to make sure we didn't jump off and back onto the same wall
	 */
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
					//Allow wall running
					return true;
				}
			}
		}
		//Did not pass rule checks
		return false;
	}
	//rHit.collider != null && ((wallJumped && lastWallName != rHit.collider.name) || (!wallJumped && !wallRunningDisabled));

	/*
	 * This function handles jumping while wall-running.  
	 * Depending on any keys held down, the jump will be angled away from the wall.
	 */
	public Vector3 WallJump(Vector3 velocity, float jumpSpeed, bool isDPressed, bool isWPressed, bool isAPressed)
	{
		//The angle we want to jump relative to the wall
		float degrees = 45f;
		//Flag us as having wall jumped
		wallJumped = true;
		//Depending on the button held and our wall-running side, increase the angle
		if(wallRunningLeft)
		{
			if(isDPressed)
			{
				degrees = 45f;
			}
			else if(isWPressed)
			{
				degrees = 70f;
			}
		}
		else if(wallRunningRight)
		{
			if(isAPressed)
			{
				degrees = 45f;
			}
			else if(isWPressed)
			{
				degrees = 70f;
			}
		}
		else 
		{
			degrees = 0f;
		}
		//Disable wall-running
		wallRunningLeft = false;
		wallRunningRight = false;
		wallRunningBack = false;
		//Reset the y-velocity for rotation calculations
		velocity.y = 0;
		//Convert degress to radians
		float radians = degrees * Mathf.Deg2Rad;
		//Scale the normal of the wall we're running on
		Vector3 dir = wallRunNormal * 14f;
		//Rotate the current velocity towards the target direction the amount of radians determined above
		velocity = Vector3.RotateTowards(dir, velocity, radians, 0.0f);
		velocity *= 1.2f;
		//Jump upwards
		velocity.y = jumpSpeed;
		return velocity;
	}

	/**
	 * Stick to the wall if we aim while wall-running 
	 */
	public void HandleWallSticking(bool isAiming)
	{
		if(isAiming && isWallRunning())
		{
			wallSticking = true;
			wallRunTimer = 0f;
		}
		else
		{
			wallSticking = false;
		}
	}
}
