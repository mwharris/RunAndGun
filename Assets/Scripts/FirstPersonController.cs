using UnityEngine;
using System.Collections;

public class FirstPersonController : MonoBehaviour {
	//private float movementSpeed = 60.0f;
	private float movementSpeed = 0.96f;
	//private float movementSpeed = 1.98f;
	public float mouseSensitivity = 5.0f;
	public float verticalRotation = 0f;
	public float verticalVelocity = 0f;
	public float upDownRange = 60.0f;
	private float jumpSpeed = 8f;
	public float walkTimer = 0f;
	public float runTimer = 0f;
	public Camera playerCamera;
	public AudioClip jumpSound;         
	public CharacterController charController;
	public GameObject playerBody;
	public bool isCrouching = false;

	/// WALL-RUNNING VARIABLES //////////////////
	private float cameraTotalRotation = 0f;
	private float cameraRotAmount = 15f;
	private float cameraRotZ = 0;
	private bool wallRunRight = false;
	private bool wallRunLeft = false;
	private Vector3 wallRunDirection;
	private Vector3 wallRunNormal;
	private bool wallJumpDirectionSet = false;
	private bool wallRunningDisabled = false;
	private float wallRunTimer = 0.0f;
	private float wallRunMax = 2.0f;
	private bool test = false;
	////////////////////////////////////////////

	private float crouchMovementSpeed;
	private bool isSprinting = false;
	private bool startedSprinting = false;
	private float origWalkTimer;
	private float origRunTimer;
	private bool wasAirborne;
	private bool allowAirMovement = false;
	private AudioSource aSource;
	private CharacterController cc;
	private GameObject gunObj;
	private Vector3 velocity;
	private float forwardSpeed;
	private float sideSpeed;
	private int jumps;
	private FXManager fxManager;
	private MyHeadBob headBobScript;

	//Variables for crouching
	public float crouchCamHeight;
	public float crouchBodyScale;
	public float crouchBodyPos;
	private float crouchDeltaHeight;
	private float crouchDeltaScale;
	private float crouchDeltaPos;
	private float standardCamHeight;
	private float standardBodyScale;
	private float standardBodyPos;

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
		//Initialize a reference to the FXManager
		fxManager = GameObject.FindObjectOfType<FXManager>();
		//Initialize a reference to HeadBob script
		headBobScript = playerCamera.GetComponent<MyHeadBob>();
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
		///////////// TODO: REMOVE THESE RAYCAST TEST LINES //////////////
		/*
		Vector3 r = transform.right*5;
		Vector3 l = -transform.right*5;
		Vector3 f = transform.forward*5;
		Vector3 fr = Vector3.RotateTowards(transform.forward, transform.right, 45 * Mathf.Deg2Rad, 10)*5;
		Vector3 fl = Vector3.RotateTowards(transform.forward, -transform.right, 45 * Mathf.Deg2Rad, 10)*5;
		Vector3 b = -transform.forward*5;
		Vector3 br = Vector3.RotateTowards(-transform.forward, transform.right, 45 * Mathf.Deg2Rad, 10)*5;
		Vector3 bl = Vector3.RotateTowards(-transform.forward, -transform.right, 45 * Mathf.Deg2Rad, 10)*5;
		Debug.DrawRay(transform.position, new Vector3(r.x, r.y + 5, r.z));
		Debug.DrawRay(transform.position, new Vector3(l.x, l.y + 5, l.z));
		Debug.DrawRay(transform.position, new Vector3(f.x, f.y + 5, f.z));
		Debug.DrawRay(transform.position, new Vector3(fr.x, fr.y + 5, fr.z));
		Debug.DrawRay(transform.position, new Vector3(fl.x, fl.y + 5, fl.z));
		Debug.DrawRay(transform.position, new Vector3(b.x, b.y + 5, b.z));
		Debug.DrawRay(transform.position, new Vector3(br.x, br.y + 5, br.z));
		Debug.DrawRay(transform.position, new Vector3(bl.x, bl.y + 5, bl.z));
		*/
		////////////////////////////////////////////////////////////////
		Debug.DrawRay(transform.position, transform.right);
		Debug.DrawRay(transform.position, -transform.right);
		Debug.DrawRay(transform.position, transform.forward);
		Debug.DrawRay(transform.position, -transform.forward);

		if (test) 
		{
			test = false;
		}

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
		if(cc.isGrounded){
			velocity.x *= 0.9f;
			velocity.z *= 0.9f;
		}

		//Move the char controller
		cc.Move(velocity * Time.deltaTime);
	}
		
	void handleMouseInput()
	{
		//Left-Right rotation based on the mouse
		float rotLeftRight = Input.GetAxis("Mouse X") * mouseSensitivity;
		transform.Rotate(0, rotLeftRight, 0);

		//Up-Down rotation based on the mouse
		verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
		//Clamp the range so we don't look too far up or down
		verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

		//Rotate our camera to the left or right if we are wall-running
		if(wallRunLeft || wallRunRight)
		{
			if(cameraTotalRotation < cameraRotAmount)
			{
				float currentAngle = playerCamera.transform.localRotation.eulerAngles.z;
				if(wallRunLeft)
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

		//Rotate the camera using Euler angles
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
			if(Input.GetKeyDown(KeyCode.LeftShift))
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
			else if(!Input.GetKey(KeyCode.W) && startedSprinting && isSprinting)
			{
				//Deactive sprinting
				isSprinting = false;
				startedSprinting = false;
			}

			//Get the movement input and apply movement speed based on crouching, sprinting, or standing
			forwardSpeed = Input.GetAxis("Vertical");
			sideSpeed = Input.GetAxis("Horizontal");
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
			if(wallRunLeft || wallRunRight)
			{
				//The velocity should be equal to our velocity when we started wall-running
				if(!wallJumpDirectionSet)
				{
					velocity = new Vector3(wallRunDirection.x, velocity.y, wallRunDirection.z);
				}
				//W will speed up movement slightly and S will slow down movement slightly
				float scaleVal = 0.0f;
				if(Input.GetKey(KeyCode.W))
				{
					//Scale the Vector up to 14 units if we're holding forward
					//Vector3 nVelocity = new Vector3(velocity.x, 0, velocity.z);
					//nVelocity = nVelocity * 14;
					//nVelocity.y = velocity.y;
					//velocity = nVelocity;
					//scaleVal = 1.5f;
					Vector3 blah = new Vector3(velocity.x, 0, velocity.z);
					Vector3 nVelocity = blah.normalized;
					nVelocity = nVelocity * 14;
					nVelocity.y = velocity.y;
					velocity = nVelocity;
				}
				if(Input.GetKey(KeyCode.S))
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
				forwardSpeed = Input.GetAxis("Vertical") * (movementSpeed/10);
				sideSpeed = Input.GetAxis("Horizontal") * (movementSpeed/10);
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
		if(!wallRunLeft && !wallRunRight)
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

	//Helper function to check the rules for activating wall-running
	private bool checkActivateWallRunRules(RaycastHit vHit, RaycastHit rHit, RaycastHit lHit, bool isGrounded)
	{
		if(!isGrounded && !wallRunningDisabled)
		{
			//We hit a wall we were heading towards
			if(vHit.collider != null)
			{
				return true;
			}
			//We aren't heading towards a wall, but we are close to one we can wall-run on,
			//AND we hit a button towards the wall.
			else if((lHit.collider != null && Input.GetKey(KeyCode.A)) || (rHit.collider != null && Input.GetKey(KeyCode.D)))
			{
				return true;
			}
		}
		return false;
	}
		
	//Helper function to check the rules for de-activating wall-running
	private bool checkDeactivateWallRunRules(RaycastHit vHit, RaycastHit rHit, RaycastHit lHit, RaycastHit bHit, bool isGrounded)
	{
		//If we're not holding forward, we touched the ground, or none of our raycasts hit a wall, break wall-running
		if(wallRunTimer <= 0 || cc.isGrounded || (rHit.collider == null && lHit.collider == null && bHit.collider == null))
		{
			return true;
		}
		return false;
	}

	//Raycast outwards from the player to detect walls.  If either are hit while the player is in the air,
	//active the wall-running state.  
	void handleWallRunning()
	{
		//Raycast in several directions to see if we are wall-running
		RaycastHit vHit;
		RaycastHit rHit;
		RaycastHit lHit;
		RaycastHit bHit;
		Vector3 pushDir = new Vector3(velocity.x, 0, velocity.z);
		Physics.Raycast(playerBody.transform.position, pushDir, out vHit, 2f);
		Physics.Raycast(playerBody.transform.position, playerBody.transform.right, out rHit, 2f);
		Physics.Raycast(playerBody.transform.position, -playerBody.transform.right, out lHit, 2f);
		Physics.Raycast(playerBody.transform.position, -playerBody.transform.forward, out bHit, 2f);

		if(!wallRunLeft && !wallRunRight)
		{
			//Check the rules for activating wall-running
			if(checkActivateWallRunRules(vHit, rHit, lHit, cc.isGrounded))
			{
				//Start the timer
				wallRunTimer = wallRunMax;

				//Right raycast
				if(rHit.collider != null || Input.GetKeyDown(KeyCode.D))
				{
					//Play a networked landing sound
					fxManager.GetComponent<PhotonView>().RPC("LandingFX", PhotonTargets.All, this.transform.position);
					//Flag wall-running state
					wallRunRight = true;
					//Store the hit point information
					wallRunDirection = Vector3.ProjectOnPlane(velocity, rHit.normal);
					wallJumpDirectionSet = false;
					wallRunNormal = rHit.normal;
					//Reset the jump counter
					jumps = 2;
				}

				//Left raycast
				if(lHit.collider != null || Input.GetKeyDown(KeyCode.A))
				{
					//Play a networked landing sound
					fxManager.GetComponent<PhotonView>().RPC("LandingFX", PhotonTargets.All, this.transform.position);
					//Flag wall-running state
					wallRunLeft = true;
					//Store the hit point
					wallRunDirection = Vector3.ProjectOnPlane(velocity, lHit.normal);
					wallJumpDirectionSet = false;
					wallRunNormal = lHit.normal;
					//Reset the jump counter
					jumps = 2;
				}
			}
			else
			{
				wallRunRight = false;
				wallRunLeft = false;
				wallRunTimer = 0.0f;
				//Disabled wall-running until we hit the ground
				if(cc.isGrounded)
				{
					wallRunningDisabled = false;
				}
			}
		}
		//Check if we should de-activate wall-running
		else if(checkDeactivateWallRunRules(vHit, rHit, lHit, bHit, cc.isGrounded))
		{
			wallRunRight = false;
			wallRunLeft = false;
			wallRunTimer = 0.0f;
			//If we're grounded then don't disabled wall-running
			if(!cc.isGrounded)
			{
				wallRunningDisabled = true;
			}
		}
		//Decrement the wallRunTimer
		else 
		{
			wallRunTimer -= Time.deltaTime;
		}
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
		//Read the jump input
		if(Input.GetButtonDown("Jump") && jumps > 0)
		{
			//Decrement our jumps so we can only jump twice
			jumps--;
			//Play a sound of use jumping
			PlayJumpSound(!cc.isGrounded);
			//Add an immediate velocity upwards to jump
			velocity.y = jumpSpeed;
			//If we're wall-running, angle our jump outwards
			if(wallRunLeft || wallRunRight)
			{
				//Handle double jumping
				WallJump();
				//Disable wall-running
				wallRunLeft = false;
				wallRunRight = false;
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
		if(Input.GetKey(KeyCode.S))
		{
			velocity = velocity + (-transform.forward * 7);
		}
		if(Input.GetKey(KeyCode.A))
		{
			velocity = velocity + (-transform.right * 7);
		}
		if(Input.GetKey(KeyCode.D))
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
		//Depending on the button held and our wall-running side, increase the angle
		if(wallRunLeft)
		{
			if(Input.GetKey(KeyCode.D))
			{
				degrees = 45f;
			}
			else if(Input.GetKey(KeyCode.W))
			{
				degrees = 70f;
			}
			if (Input.GetKey (KeyCode.A)) {
				test = true;
			}
		}
		else
		{
			if(Input.GetKey(KeyCode.A))
			{
				degrees = 45f;
			}
			else if(Input.GetKey(KeyCode.W))
			{
				degrees = 70f;
			}
			if (Input.GetKey (KeyCode.D)) {
				test = true;
			}
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
		if(Input.GetKey(KeyCode.S))
		{
			targetDir = -transform.forward;
			buttonPushed = true;
		}
		if(Input.GetKey(KeyCode.A))
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
		if(Input.GetKey(KeyCode.D))
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
		if(Input.GetKey(KeyCode.W))
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

	private void ToggleSprint()
	{
		isSprinting = !isSprinting;
		if(isSprinting == false)
		{
			startedSprinting = false;
		}
	}

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

	private void handleCrouching()
	{
		//Crouching
		if(Input.GetKeyDown(KeyCode.LeftControl))
		{
			ToggleCrouch();
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
			if(bodyLocalPos.y > crouchBodyPos)
			{
				//Move the body down
				bodyLocalPos.y = lowerHeight(bodyLocalPos.y, crouchDeltaPos, Time.deltaTime, crouchBodyPos);
			}
		}
		else
		{
			if(camLocalPos.y < standardCamHeight)
			{
				//Move the camera up
				camLocalPos.y = raiseHeight(camLocalPos.y, crouchDeltaHeight, Time.deltaTime, standardCamHeight);
			}
			if(bodyLocalScale.y < standardBodyScale)
			{
				//Scale the body up
				bodyLocalScale.y = raiseHeight(bodyLocalScale.y, crouchDeltaScale, Time.deltaTime, standardBodyScale);
			}
			if(bodyLocalPos.y < standardBodyPos)
			{
				//Move the body up
				bodyLocalPos.y = raiseHeight(bodyLocalPos.y, crouchDeltaPos, Time.deltaTime, standardBodyPos);
			}
		}

		//Apply the local position updates
		playerCamera.transform.localPosition = camLocalPos;
		playerBody.transform.localPosition = bodyLocalPos;
		playerBody.transform.localScale = bodyLocalScale;
	}

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

	private float raiseHeight(float yPos, float delta, float deltaTime, float height)
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
