using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;

[RequireComponent(typeof (CrouchController))]
[RequireComponent(typeof (WallRunController))]
[RequireComponent(typeof (ShootController))]
[RequireComponent(typeof (JumpController))]
[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
[RequireComponent(typeof (LerpControlledBob))]
public class FirstPersonController : MonoBehaviour 
{
	public GameObject playerBody;
	public AudioClip jumpSound;  
	public bool isGrounded;

	[HideInInspector] public CharacterController cc;
	[HideInInspector] public bool isSprinting = false;

	[SerializeField] private Camera playerCamera;   

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
	[HideInInspector] public Vector3 velocity;
	//private float movementSpeed = 60.0f;
	private float movementSpeed = 0.96f;
	//private float movementSpeed = 1.98f;
	private float jumpSpeed = 8f;
	[HideInInspector] public float forwardSpeed;
	[HideInInspector] public float sideSpeed;
	private int jumps;
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
	private GameManager gm;
	private CrouchController crouchController;
	private WallRunController wallRunController;
	private MenuController menuController;
	private ShootController shootContoller;
	private FXManager fxManager;
	private MyHeadBob headBobScript;
	//private JumpBob jumpBob;
	[SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
	////////////////////////////////////////////

	Vector3 ogCamPos;
	void Start () {
		ogCamPos = playerCamera.transform.localPosition;
		//Initialize a reference to the character controller component
		cc = GetComponent<CharacterController>();
		//Lock the mouse cursor
		//Cursor.lockState = CursorLockMode.Locked;
		//We will only ever have 2 jumps
		jumps = 2;
		//Get a reference to the audio source
		aSource = GetComponent<AudioSource>();
		//Keep track of how long our walk audio delay should be
		origWalkTimer = walkTimer;
		origRunTimer = runTimer;
		//Initialize a reference to various scripts we need
		fxManager = GameObject.FindObjectOfType<FXManager>();
		headBobScript = playerCamera.GetComponent<MyHeadBob>();
		gm = GameObject.FindObjectOfType<GameManager>();
		menuController = GameObject.FindObjectOfType<MenuController>();
		//jumpBob = GetComponent<LerpControlledBob>();
		//Set up the various controllers
		crouchController = GetComponent<CrouchController>();
		wallRunController = GetComponent<WallRunController>();
		shootContoller = GetComponent<ShootController>();
		//Initiliaze crouch controller variables
		crouchController.CalculateCrouchVars(this.gameObject, playerCamera.gameObject, movementSpeed);
	}

	void FixedUpdate () {
		//Crouching camera changes clash with jump bob camera changes
		if(!crouchController.IsCrouching && !crouchController.cameraResetting) 
		{
			//Apply a head bob when we jump
			Vector3 localPos = playerCamera.transform.localPosition;
			playerCamera.transform.localPosition = new Vector3(localPos.x, ogCamPos.y - jumpBob.Offset(), localPos.z);
		}
	}

	void Update () {
		//Test stuff
		Debug.DrawRay(transform.position, transform.right * 1f);
		Debug.DrawRay(transform.position, -transform.right * 1f);
		Debug.DrawRay(transform.position, transform.forward * 1f);
		Debug.DrawRay(transform.position, -transform.forward * 1f);
		Vector3 testV = new Vector3(velocity.x, 0, velocity.z);
		Debug.DrawRay(transform.position, testV);

		//Keep track ourselves if we are grounded or not
		isGrounded = cc.isGrounded;
		//Update variables based on options menu selections
		GatherOptions();
		//Gather all mouse and keyboard inputs if we aren't paused
		GatherInputs();
		//Handle any mouse input that occurred
		HandleMouseInput();
		//Handle crouching
		crouchController.HandleCrouching(cc, playerCamera, playerBody, isCTRLDown);
		//Handle the movement of the player
		HandleMovement();
		//Apply gravity
		HandleGravity();
		//Handle jumping of the player
		HandleJumping();
		//Tell the wall-run controller to handle any wall-running tasks
		wallRunController.HandleWallRunning(velocity, playerBody, isGrounded, ref jumps);
		//Tell the wall-run controller to also handle any wall-sticking tasks
		wallRunController.HandleWallSticking(shootContoller.isAiming);
		//Set a flag if we're airborne this frame
		if(!isGrounded)
		{
			wasAirborne = true;
		}
		else 
		{
			wasAirborne = false;
		}
		//Linear drag along the X and Z while grounded
		if(isGrounded)
		{
			velocity.x *= 0.9f;
			velocity.z *= 0.9f;
		}
		//Move the char controller
		cc.Move(velocity * Time.deltaTime);
	}

	void GatherOptions()
	{
		//Set some variables based on Optons menu
		if(menuController.mouseSensitivity != null && menuController.mouseSensitivity > 0)
		{
			mouseSensitivity = menuController.mouseSensitivity;
		}
		if(menuController.invertY != null)
		{
			invertY = menuController.invertY;
		}
	}

	void GatherInputs()
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
			//Test stuff
			if(Input.GetKeyDown(KeyCode.B))
			{
				this.GetComponent<AccuracyController>().FuckUpReticles();
			}
			if(Input.GetKeyDown(KeyCode.N))
			{
				this.GetComponent<AccuracyController>().ResetReticles();
			}
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

	//Calculate and apply player rotation based on mouse input
	void ApplyHorizontalRotation()
	{
		bool rotationSet = false;
		//If we are wall-running
		if(wallRunController.isWallRunning())
		{
			//Tell the wall run controller to handle our look rotation.
			//This is so we don't look to far into the wall.
			if(wallRunController.SetWallRunLookRotation())
			{
				rotationSet = true;
			}
		}
		//Rotate normally if we are not wall-running OR our look rotation while wall-running is normal
		if(!rotationSet)
		{
			transform.Rotate(0, horizontalRotation, 0);
		}
	}

	void HandleMouseInput()
	{
		//Apply any horizontal look rotation
		ApplyHorizontalRotation();
		//Clamp the up and down mouse range so we don't look too far in one direction
		verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
		//Calculate the angle we should tilt the camera depending on wall-run side
		float cameraRotZ = wallRunController.CalculateCameraTilt(playerCamera);
		//Rotate the camera Up/Down and Tilt using Euler angles
		Quaternion targetTilt = Quaternion.Euler(verticalRotation, 0, cameraRotZ);
		playerCamera.transform.localRotation = Quaternion.Lerp(playerCamera.transform.localRotation, targetTilt, 30*Time.deltaTime);
	}

	//Handle any WASD or arrow key movement
	void HandleMovement()
	{
		//Decrement the walk/run audio timers
		walkTimer -= Time.deltaTime;
		runTimer -= Time.deltaTime;

		//Grounded movement
		if(isGrounded)
		{
			//Activate sprinting if shift was pressed
			if(isShiftDown && !shootContoller.isAiming)
			{
				//Flag us as sprinting
				isSprinting = !isSprinting;
				//Come out of crouch if we're crouching
				if(isSprinting && crouchController.IsCrouching)
				{
					crouchController.StopCrouching();
				}
			}
			//If we aim while sprinting
			else if(isSprinting && shootContoller.isAiming)
			{
				//Stop sprinting
				isSprinting = false;
			}
			//Only sprint while holding forward
			else if(!isWPressed && isSprinting)
			{
				isSprinting = false;
			}

			//Apply movement speed based on crouching, sprinting, or standing
			if(crouchController.IsCrouching || shootContoller.isAiming)
			{
				forwardSpeed *= crouchController.CrouchMovementSpeed;
				sideSpeed *= crouchController.CrouchMovementSpeed;
				//Disable head bob while crouching
				headBobScript.enabled = false;
			}
			else if(isSprinting)
			{
				forwardSpeed *= movementSpeed * 1.5f;
				sideSpeed *= movementSpeed * 1.5f;
				//Enable head bob while sprinting
				headBobScript.enabled = true;
			}
			else
			{
				forwardSpeed *= movementSpeed;
				sideSpeed *= movementSpeed;
				//Disable head bob while walking
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

			//If we're wall-sticking
			if(wallRunController.wallSticking)
			{
				//Stop all movement
				velocity = Vector3.zero;
			}
			//If we're wall-running
			else if(wallRunController.isWallRunning())
			{
				//Calculate our wall-running velocity
				velocity = wallRunController.SetWallRunVelocity(velocity, isWPressed, isSPressed);
				//Play footstep FX while wall-running
				PlayFootStepAudio(false, true);
			}
			//If we're airborne and not wall-running
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
	void HandleGravity()
	{
		//Add normal gravity if we aren't wall-running
		if(!wallRunController.isWallRunning())
		{
			if(!isGrounded)
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
		else if(!isGrounded)
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

	//Push the player upwards if we jumped
	void HandleJumping()
	{
		//If we just landed on the ground
		if(wasAirborne && isGrounded)
		{
			//Add a head bob to our landing
			StartCoroutine(jumpBob.DoBobCycle());
			//Play a networked landing sound
			fxManager.GetComponent<PhotonView>().RPC("LandingFX", PhotonTargets.All, this.transform.position);
			//Reset our air movement flag
			allowAirMovement = false;
		}
		//If we just jumped
		if(isSpaceDown && jumps > 0)
		{
			//Add a head bob to our jump
			StartCoroutine(jumpBob.DoBobCycle());
			//Decrement our jumps so we can only jump twice
			jumps--;
			//Play a sound of use jumping
			PlayJumpSound(!isGrounded);
			//Stop aiming if we are aiming
			shootContoller.StopAiming();
			//Add an immediate velocity upwards to jump
			velocity.y = jumpSpeed;
			//If we're wall-running, angle our jump outwards
			if(wallRunController.isWallRunning())
			{
				//Handle double jumping
				velocity = wallRunController.WallJump(velocity, jumpSpeed, isDPressed, isWPressed, isAPressed);
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
				//if(crouchController.IsCrouching)
				//{
				//	crouchController.ToggleCrouch();
				//}
				//Add a little horizontal movement if we double jumped while holding a key
				if(!isGrounded)
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
		if (!isGrounded && !isWallRunning && !aSource.isPlaying)
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
}
