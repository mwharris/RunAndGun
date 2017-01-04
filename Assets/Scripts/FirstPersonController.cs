﻿using UnityEngine;
using System.Collections;

public class FirstPersonController : MonoBehaviour {
	public Camera playerCamera;   
	public GameObject playerBody;
	private CharacterController cc;
	private GameObject gunObj;
	public AudioClip jumpSound;  
	private AudioSource aSource;    

	/// MOUSE CONTROL VARIABLES ////////////////
	public float mouseSensitivity = 5.0f;
	public float horizontalRotation = 0f;
	public float verticalRotation = 0f;
	public float verticalVelocity = 0f;
	public float upDownRange = 60.0f;
	public bool invertY = false;
	////////////////////////////////////////////

	/// TIMERS /////////////////////////////////
	public float walkTimer = 0f;
	public float runTimer = 0f;
	private float origWalkTimer;
	private float origRunTimer;
	////////////////////////////////////////////

	/// VELOCITY VARIABLES /////////////////////
	private Vector3 velocity;
	//private float movementSpeed = 60.0f;
	private float movementSpeed = 0.96f;
	//private float movementSpeed = 1.98f;
	private float jumpSpeed = 8f;
	private float forwardSpeed;
	private float sideSpeed;
	private int jumps;
	private bool isSprinting = false;
	private bool startedSprinting = false;
	private bool wasAirborne;
	private bool allowAirMovement = false;
	////////////////////////////////////////////

	/// MOVEMENT FLAGS /////////////////////
	private bool isWDown = false;
	private bool isWPressed = false;
	private bool isADown = false;
	private bool isAPressed = false;
	private bool isSDown = false;
	private bool isSPressed = false;
	private bool isDDown = false;
	private bool isDPressed = false;
	private bool isShiftDown = false;
	private bool isShiftPressed = false;
	private bool isCTRLDown = false;
	private bool isCTRLPressed = false;
	private bool isSpaceDown = false;
	private bool isSpacePressed = false;
	////////////////////////////////////////

	/// IMPORTED SCRIPTS //////////////////
	private FXManager fxManager;
	private MyHeadBob headBobScript;
	private GameManager gm;
	private MenuController mc;
	private ShootController sc;
	////////////////////////////////////////////

	/// WALL-RUNNING VARIABLES //////////////////
	private float cameraTotalRotation = 0f;
	private float cameraRotAmount = 15f;
	private float cameraRotZ = 0;
	private bool wallRunningLeft = false;
	private bool wallRunningRight = false;
	private bool wallRunningBack = false;
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

	// CROUCHING VARIABLES 	////////////////////
	public bool isCrouching = false;
	public float crouchCamHeight;
	public float crouchBodyScale;
	public float crouchBodyPos;
	private float crouchMovementSpeed;
	private float crouchDeltaHeight;
	private float crouchDeltaScale;
	private float crouchDeltaPos;
	private float standardCamHeight;
	private float standardBodyScale;
	private float standardBodyPos;
	////////////////////////////////////////////

	void Start () {
		//Initialize a reference to the character controller component
		cc = this.GetComponent<CharacterController>();
		//Lock the mouse cursor
		Cursor.lockState = CursorLockMode.Locked;
		//We will only ever have 2 jumps
		jumps = 2;
		//Get a reference to the audio source
		aSource = this.transform.GetComponent<AudioSource>();
		//Keep track of how long our walk audio delay should be
		origWalkTimer = walkTimer;
		origRunTimer = runTimer;
		//Initialize a reference to various scripts we need
		fxManager = GameObject.FindObjectOfType<FXManager>();
		headBobScript = playerCamera.GetComponent<MyHeadBob>();
		gm = GameObject.FindObjectOfType<GameManager>();
		mc = GameObject.FindObjectOfType<MenuController>();
		sc = GameObject.FindObjectOfType<ShootController>();
		//Set camera height variables
		standardCamHeight = 2.5f;
		standardBodyScale = 1.5f;
		standardBodyPos = 1.5f;
		crouchDeltaHeight = standardCamHeight - crouchCamHeight;
		crouchDeltaScale = standardBodyScale - crouchBodyScale;
		crouchDeltaPos = standardBodyPos - crouchBodyPos;
		//Calculate the movement speed while crouched
		crouchMovementSpeed = movementSpeed/2;
	}

	void Update () {
		//Test stuff
		Debug.DrawRay(transform.position, transform.right * 1f);
		Debug.DrawRay(transform.position, -transform.right * 1f);
		Debug.DrawRay(transform.position, transform.forward * 1f);
		Debug.DrawRay(transform.position, -transform.forward * 1f);
		Vector3 testV = new Vector3(velocity.x, 0, velocity.z);
		Debug.DrawRay(transform.position, testV);

		//Wall run test checks
		//DoWallRunCheckFAKE(cc.isGrounded);

		//Update the mou
		gatherOptions();
		//Gather all mouse and keyboard inputs if we aren't paused
		gatherInputs();
		//Handle any mouse input that occurred
		handleMouseInput();
		//Handle crouching
		handleCrouching();
		//Handle the movement of the player
		handleMovement();
		//Apply gravity
		handleGravity();
		//Handle jumping of the player
		handleJumping();
		//Handle the player wall-running
		handleWallRunning();
		//Set a flag if we're airborne this frame
		if(!cc.isGrounded)
		{
			wasAirborne = true;
		}
		else 
		{
			wasAirborne = false;
		}
		//Linear drag along the X and Z while grounded
		if(cc.isGrounded)
		{
			velocity.x *= 0.9f;
			velocity.z *= 0.9f;
		}
		//Move the char controller
		cc.Move(velocity * Time.deltaTime);
	}

	void gatherOptions()
	{
		//Set some variables based on Optons menu
		if(mc.mouseSensitivity != null && mc.mouseSensitivity > 0)
		{
			mouseSensitivity = mc.mouseSensitivity;
		}
		if(mc.invertY != null)
		{
			invertY = mc.invertY;
		}
	}

	void gatherInputs()
	{
		//Only gather user input if we're not paused
		if(gm.GetGameState() == GameManager.GameState.playing)
		{
			//Mouse look inputs.  Include mouse inversion and sensitivity
			horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
			verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1 : 1);
			//Keyboard movement inputs
			forwardSpeed = Input.GetAxis("Vertical");
			sideSpeed = Input.GetAxis("Horizontal");
			isWDown = Input.GetKeyDown(KeyCode.W);
			isWPressed = Input.GetKey(KeyCode.W);
			isADown = Input.GetKeyDown(KeyCode.A);
			isAPressed = Input.GetKey(KeyCode.A);
			isSDown = Input.GetKeyDown(KeyCode.S);
			isSPressed = Input.GetKey(KeyCode.S);
			isDDown = Input.GetKeyDown(KeyCode.D);
			isDPressed = Input.GetKey(KeyCode.D);
			//Crouch and Sprint
			isCTRLDown = Input.GetKeyDown(KeyCode.LeftControl);
			isCTRLPressed = Input.GetKey(KeyCode.LeftControl);
			isShiftDown = Input.GetKeyDown(KeyCode.LeftShift);
			isShiftPressed = Input.GetKey(KeyCode.LeftShift);
			//Jump
			isSpaceDown = Input.GetKeyDown(KeyCode.Space);
			isSpacePressed = Input.GetKey(KeyCode.Space);
		}
		//Reset all flags if we're paused
		else
		{
			forwardSpeed = 0;
			sideSpeed = 0;
			isWDown = false;
			isWPressed = false;
			isADown = false;
			isAPressed = false;
			isSDown = false;
			isSPressed = false;
			isDDown = false;
			isDPressed = false;
			isCTRLDown = false;
			isCTRLPressed = false;
			isShiftDown = false;
			isShiftPressed = false;
			isSpaceDown = false;
			isSpacePressed = false;
		}
	}

	//Tilt the camera left or right depending on which side we are wall-running
	void CalculateCameraTilt()
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

	//Calculate and apply player rotation based on mouse input
	void ApplyHorizontalRotation()
	{
		if(wallRunningLeft || wallRunningRight || wallRunningBack)
		{
			Vector3 testNormal = wallRunNormal;
			Vector3 testForward = transform.forward;
			//Get the vector 90 degrees to the left and right of the normal
			testNormal = Quaternion.Euler(0,90,0) * testNormal;
			//Calculate cross products between where we're looking and these two vectors
			Vector3 cross = Vector3.Cross(testNormal, testForward);
			//Mess with the rotation based on our forward vector relative to the boundaries
			if(cross.y > 0.05)
			{
				//We looked too far left/right, rotate towards the cross product
				Vector3 otherCross = new Vector3();
				if(wallRunningRight){ otherCross = Vector3.Cross(Vector3.up, wallRunNormal); }
				if(wallRunningLeft){ otherCross = Vector3.Cross(Vector3.up, -wallRunNormal); }
				float degrees = Vector3.Angle(testForward, otherCross);
				float rads = degrees * Mathf.Deg2Rad;
				testForward = Vector3.RotateTowards(testForward, otherCross, 10, 0.0f);
				transform.rotation = Quaternion.LookRotation(new Vector3(testForward.x, 0, testForward.z));
			}
			//Rotate normally if we don't break the boundaries
			else
			{
				transform.Rotate(0, horizontalRotation, 0);
			}
		}
		//Rotate normally if we are not wall-running
		else 
		{
			transform.Rotate(0, horizontalRotation, 0);
		}
	}

	void handleMouseInput()
	{
		//Apply any horizontal look rotation
		ApplyHorizontalRotation();
		//Clamp the up and down mouse range so we don't look too far in one direction
		verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
		//Calculate the angle we should tilt the camera depending on wall-run side
		CalculateCameraTilt();
		//Rotate the camera Up/Down and Tilt using Euler angles
		playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, cameraRotZ);
	}

	//Handle any WASD or arrow key movement
	void handleMovement()
	{
		//Decrement the walk/run audio timers
		walkTimer -= Time.deltaTime;
		runTimer -= Time.deltaTime;

		//Grounded movement
		if(cc.isGrounded)
		{
			//Activate sprinting if shift was pressed
			if(isShiftDown)
			{
				//Flag us as sprinting
				ToggleSprint();
				//Come out of crouch if we're crouching
				if(isSprinting && isCrouching)
				{
					StopCrouching();
				}
			}
			//If we are not holding forward and we already started sprinting
			else if(!isWPressed && startedSprinting && isSprinting)
			{
				//Deactive sprinting
				isSprinting = false;
				startedSprinting = false;
			}

			//Apply movement speed based on crouching, sprinting, or standing
			if(isCrouching)
			{
				forwardSpeed *= crouchMovementSpeed;
				sideSpeed *= crouchMovementSpeed;
				headBobScript.enabled = false;
			}
			else if(isSprinting)
			{
				forwardSpeed *= movementSpeed * 1.5f;
				sideSpeed *= movementSpeed * 1.5f;
				headBobScript.enabled = true;
				//Mark ourselves as started sprinting if we're moving while sprinting
				if(forwardSpeed != 0 || sideSpeed != 0)
				{
					startedSprinting = true;
				}
			}
			else
			{
				forwardSpeed *= movementSpeed;
				sideSpeed *= movementSpeed;
				headBobScript.enabled = false;
			}

			//Play sprinting FX while sprinting
			if(forwardSpeed != 0 || sideSpeed != 0)
			{
				//Increase footstep rate
				PlayFootStepAudio(isSprinting, false);
			}

			//Add the x / z movement
			if(forwardSpeed == 0 && sideSpeed == 0)
			{
				velocity.x = 0;
				velocity.z = 0;
			} 
			else 
			{
				velocity += forwardSpeed * transform.forward;
				velocity += sideSpeed * transform.right;
				//velocity += forwardSpeed * transform.forward * Time.deltaTime;
				//velocity += sideSpeed * transform.right * Time.deltaTime;
			}
		}
		//Air / Wall-running movement
		else
		{
			//Disable head bob
			headBobScript.enabled = false;

			//If we're wall-running
			if(wallRunningLeft || wallRunningRight || wallRunningBack)
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
				if(isWPressed)
				{
					//Scale the Vector up to 14 units if we're holding forward
					Vector3 blah = new Vector3(velocity.x, 0, velocity.z);
					Vector3 nVelocity = blah.normalized;
					nVelocity = nVelocity * 14;
					nVelocity.y = velocity.y;
					velocity = nVelocity;
				}
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
				//Play footstep FX while wall-running
				PlayFootStepAudio(false, true);
			}
			else
			{
				//Get the movement input
				forwardSpeed = forwardSpeed * (movementSpeed/10);
				sideSpeed = sideSpeed * (movementSpeed/10);
				//Add the x / z movement
				if(forwardSpeed != 0 && allowAirMovement)
				{
					velocity += forwardSpeed * transform.forward;
					//velocity += forwardSpeed * transform.forward * Time.deltaTime;
				} 
				if(sideSpeed != 0 && allowAirMovement) 
				{
					velocity += sideSpeed * transform.right;
					//velocity += sideSpeed * transform.right * Time.deltaTime;
				}
			}
		}
	}

	//Pull the player down continuously unless we are grounded
	void handleGravity()
	{
		//Add normal gravity if we aren't wall-running
		if(!wallRunningLeft && !wallRunningRight && !wallRunningBack)
		{
			if(!cc.isGrounded)
			{
				//Add gravity only when we aren't on the ground
				//velocity += Physics.gravity;
				velocity += Physics.gravity * Time.deltaTime;
			} 
			else 
			{
				//No y-velocity while grounded
				//velocity += -Physics.gravity * Time.deltaTime;
				//Double jump enabled while grounded
				jumps = 2;
			}
		}
		//If we're wall-running, lower the gravity to simulate it
		else if(!cc.isGrounded)
		{
			//Slow our descent
			if(velocity.y <= 0)
			{
				//velocity += (Physics.gravity/4);
				velocity += (Physics.gravity/4) * Time.deltaTime;
			}
			//Otherwise use normal gravity
			else 
			{
				velocity += (Physics.gravity/1.5f) * Time.deltaTime;
			}
		}
	}

	//rHit.collider != null && ((wallJumped && lastWallName != rHit.collider.name) || (!wallJumped && !wallRunningDisabled));
	bool DoubleWallRunCheck(RaycastHit hit){
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

	//Perform raycast to check wall-running rules; return the hit location
	RaycastHit DoWallRunCheck(bool isGrounded)
	{
		//Boolean to determine if we are wall-running in any direction
		bool wallRunning = wallRunningLeft || wallRunningRight || wallRunningBack;

		//If we're not grounded and we haven't disabled wall-running
		if(!isGrounded && ((wallRunning && wallRunTimer > 0) || !wallRunning))
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
			if (lookAngleGood && rHitValid && rDoubleJumpCheck && (rightGood || wallRunning)) 
			{
				if(wallRunningDisabled){
					print("FOUND IT!!!");
				}
				//Flag if we are initializing the wall-run
				if (!wallRunning) {
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
			else if (lookAngleGood && lHitValid && lDoubleJumpCheck && (leftGood || wallRunning)) 
			{
				if(wallRunningDisabled){
					print("FOUND IT!!!");
				}
				//Flag if we are initializing the wall-run
				if (!wallRunning) {
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
				if(wallRunningDisabled){
					print("FOUND IT!!!");
				}
				//Flag if we are initializing the wall-run
				if (!wallRunning) {
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
			else if(wallRunning)
			{
				print("WUUUUUUUUUUUUUUUUUUUUUUUT???");
			}
		}
		else if(wallRunTimer <= 0)
		{
			wallRunningDisabled = true;
		}

		return new RaycastHit();
	}

	//Raycast outwards from the player to detect walls.  If either are hit while the player is in the air,
	//active the wall-running state.  
	void handleWallRunning()
	{
		//Raycast in several directions to see if we are wall-running
		RaycastHit wallRunHit = DoWallRunCheck(cc.isGrounded);

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
		else if(wallRunHit.collider != null && (wallRunningLeft || wallRunningRight || wallRunningBack))
		{
			//Decrement the running timer
			wallRunTimer -= Time.deltaTime;
			//Project our wall-run direction and store the hit point information
			wallRunDirection = Vector3.ProjectOnPlane(velocity, wallRunHit.normal);
			wallRunNormal = wallRunHit.normal;
		}
		//The rules failed but we're already wall-running
		else if(wallRunHit.collider == null && (wallRunningLeft || wallRunningRight || wallRunningBack))
		{
			//Deactivate wall-running
			wallRunningLeft = false;
			wallRunningRight = false;
			wallRunningBack = false;
			wallRunTimer = 0.0f;
		}

		//Enable wall-running if we touch the ground or the timer runs out
		if(cc.isGrounded) 
		{
			lastWallName = "";
			wallJumped = false;
		}
		if(cc.isGrounded)
		{
			wallRunningDisabled = false;
			//wallRunningDisabledTimer = 0;
		} 
		//else 
		//{
		//	wallRunningDisabledTimer -= Time.deltaTime;
		//}
	}

	//Push the player upwards if we jumped
	void handleJumping()
	{
		//Play a sound this frame if we hit the ground
		if(wasAirborne && cc.isGrounded)
		{
			//Play a networked landing sound
			fxManager.GetComponent<PhotonView>().RPC("LandingFX", PhotonTargets.All, this.transform.position);
			//Reset our air movement flag
			allowAirMovement = false;
		}

		if(isSpaceDown && jumps > 0)
		{
			//Decrement our jumps so we can only jump twice
			jumps--;
			//Play a sound of use jumping
			PlayJumpSound(!cc.isGrounded);
			//Stop aiming if we are aiming
			sc.StopAiming();
			//Add an immediate velocity upwards to jump
			velocity.y = jumpSpeed;
			//If we're wall-running, angle our jump outwards
			if(wallRunningLeft || wallRunningRight || wallRunningBack)
			{
				//Handle double jumping
				WallJump();
				//Disable wall-running
				wallRunningLeft = false;
				wallRunningRight = false;
				wallRunningBack = false;
				//wallRunningDisabled = true;
				//wallRunningDisabledTimer = 0.5f;
			}
			else
			{
				//Determine if we jumped straight upwards
				if(velocity.x == 0 && velocity.z == 0){
					allowAirMovement = true;
				} else {
					allowAirMovement = false;
				}
				//If we're crouched, uncrouch for our jump
				if(isCrouching)
				{
					ToggleCrouch();
				}
				//Add a little horizontal movement if we double jumped while holding a key
				if(!cc.isGrounded)
				{
					//Handle double jumping
					RotateDoubleJump();
				}
			}	
		}
	}

	/*
	 * This function handles double jumping by applying a force in the direction
	 * the player is holding relatice to their current look position.  This will
	 * also give them a slight boost in that direction.
	*/
	private void ForceDoubleJump()
	{
		//If the player is holding left or right at the time of the jump,
		//apply a force in the direction they are pressing.
		if(isSPressed)
		{
			velocity = velocity + (-transform.forward * 7);
		}
		if(isAPressed)
		{
			velocity = velocity + (-transform.right * 7);
		}
		if(isDPressed)
		{
			velocity = velocity + (transform.right * 7);
		}
		//Apply upward force
		velocity.y = jumpSpeed;
	}

	/*
	 * This function handles jumping while wall-running.  Depending on any keys held down,
	 * the jump will be angled away from the wall.
	 */
	private void WallJump()
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
	}

	/*
	 * This function handles double jumping by rotating the current velocity
	 * toward the direction the player is holding relative to their current
	 * look position.  This applies no boost unlike ForceDoubleJump().
	*/
	private void RotateDoubleJump()
	{
		//Determine our target jump direction based on player input
		bool buttonPushed = false;
		Vector3 targetDir = velocity;
		if(isSPressed)
		{
			targetDir = -transform.forward;
			buttonPushed = true;
		}
		if(isAPressed)
		{
			if(buttonPushed)
			{
				targetDir += -transform.right;
			}
			else
			{
				targetDir = -transform.right;
			}
			buttonPushed = true;
		}
		if(isDPressed)
		{
			if(buttonPushed)
			{
				targetDir += transform.right;
			}
			else
			{
				targetDir = transform.right;
			}
			buttonPushed = true;
		}
		if(isWPressed)
		{
			if(buttonPushed)
			{
				targetDir += transform.forward;
			}
			else
			{
				targetDir = transform.forward;
			}
		}
		//Reset the y-velocity for rotation calculations
		velocity.y = 0;
		//Find the angle, in radians, between our target direction and current direction
		float degrees = Vector3.Angle(velocity, targetDir);
		float radians = degrees * Mathf.Deg2Rad;
		//Rotate the current direction the amount of radians determined above
		velocity = Vector3.RotateTowards(velocity, targetDir, radians, 0.0f);
		//Jump upwards
		velocity.y = jumpSpeed;
	}

	private void PlayFootStepAudio(bool isSprinting, bool isWallRunning)
	{
		if (!cc.isGrounded && !isWallRunning && !aSource.isPlaying)
		{
			return;
		}

		if((!isSprinting && walkTimer <= 0) || ((isSprinting || isWallRunning) && runTimer <= 0) )
		{
			//Reset the audio timer
			walkTimer = origWalkTimer;
			runTimer = origRunTimer;
			//Play a networked walking sound
			fxManager.GetComponent<PhotonView>().RPC("FootstepFX", PhotonTargets.All, this.transform.position);
		}
	}

	private void PlayJumpSound(bool isDoubleJump)
	{
		if(isDoubleJump)
		{
			//Play a networked double jump sound
			fxManager.GetComponent<PhotonView>().RPC("DoubleJumpFX", PhotonTargets.All, this.transform.position);
		}
		else
		{
			aSource.clip = jumpSound;
			aSource.Play();
		}
	}

	private void ToggleSprint()
	{
		isSprinting = !isSprinting;
		if(isSprinting)
		{
			//Stop aiming if we are aiming
			sc.StopAiming();
		}
		if(isSprinting == false)
		{
			//Start our sprint process
			startedSprinting = false;
		}
	}

	private void handleCrouching()
	{
		//Crouching logic
		if(isCTRLDown)
		{
			ToggleCrouch();
			if(isCrouching)
			{
				cc.height = 1.9f;
			}
			else
			{
				cc.height = 2.9f;
			}
		}
		//Store the local position for modification
		Vector3 camLocalPos = playerCamera.transform.localPosition;
		Vector3 bodyLocalPos = playerBody.transform.localPosition;
		Vector3 bodyLocalScale = playerBody.transform.localScale;
		//Modify the local position based on if we are/aren't crouching
		if(isCrouching)
		{
			if(camLocalPos.y > crouchCamHeight)
			{
				//Move the camera down
				camLocalPos.y = lowerHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, crouchCamHeight);
			}
			if(bodyLocalScale.y > crouchBodyScale)
			{
				//Scale the body down
				bodyLocalScale.y = lowerHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, crouchBodyScale);
			}
		}
		else
		{
			if(camLocalPos.y < standardCamHeight)
			{
				//Move the camera up
				camLocalPos.y = RaiseHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, standardCamHeight);
			}
			if(bodyLocalScale.y < standardBodyScale)
			{
				//Scale the body up
				bodyLocalScale.y = RaiseHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, standardBodyScale);
			}
		}
		//Apply the local position updates
		playerCamera.transform.localPosition = camLocalPos;
		playerBody.transform.localPosition = bodyLocalPos;
		playerBody.transform.localScale = bodyLocalScale;
	}

	//Helper function to toggle crouching flags
	private void ToggleCrouch()
	{
		if(isCrouching)
		{
			StopCrouching();
		}
		else 
		{
			Crouch();
		}
	}

	//Code to actual handle crouching and standing
	private void Crouch()
	{
		Vector3 test = new Vector3(0f, crouchDeltaHeight/2, 0f);
		playerBody.GetComponent<CapsuleCollider>().height -= crouchDeltaHeight;
		playerBody.GetComponent<CapsuleCollider>().center = playerBody.GetComponent<CapsuleCollider>().center - test;
		isCrouching = true;
	}
	private void StopCrouching()
	{
		Vector3 test = new Vector3(0f, crouchDeltaHeight/2, 0f);
		playerBody.GetComponent<CapsuleCollider>().height += crouchDeltaHeight;
		playerBody.GetComponent<CapsuleCollider>().center = playerBody.GetComponent<CapsuleCollider>().center + test;
		isCrouching = false;		
	}

	//Helper function to lower the height of the player due to crouching
	private float lowerHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos - (delta * deltaTime * 8) < height)
		{
			yPos = height;
		}
		else
		{
			yPos -= delta * Time.deltaTime * 8;
		}
		return yPos;
	}

	//Helper function to raise the height of the player due to standing
	private float RaiseHeight(float yPos, float delta, float deltaTime, float height)
	{
		if(yPos + (delta * deltaTime * 8) > height)
		{
			yPos = height;
		}
		else
		{
			yPos += delta * Time.deltaTime * 8;
		}
		return yPos;
	}
}
