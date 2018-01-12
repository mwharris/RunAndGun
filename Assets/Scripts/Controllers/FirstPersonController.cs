﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;

[RequireComponent(typeof (CrouchController))]
[RequireComponent(typeof (WallRunController))]
[RequireComponent(typeof (ShootController))]
[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
[RequireComponent(typeof (LerpControlledBob))]
public class FirstPersonController : AbstractBehavior 
{
	public GameObject playerBody;
	public AudioClip jumpSound;  
	private Vector3 ogCamPos;

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
	//private float movementSpeed = 60.0f;
	private float movementSpeed = 0.96f;
	//private float movementSpeed = 1.98f;
	private float jumpSpeed = 8f;
	[HideInInspector] public float forwardSpeed;
	[HideInInspector] public float sideSpeed;
	private bool wasAirborne;
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


	[SerializeField] private PlayerLook playerLook;
	private Vector2 lookInput;
	private Vector2 moveInput;

	void Start () {
		ogCamPos = playerCamera.transform.localPosition;
		//Initialize a reference to the character controller component
		cc = GetComponent<CharacterController>();
		//Lock the mouse cursor
		//Cursor.lockState = CursorLockMode.Locked;
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
		//Set up the various controllers
		crouchController = GetComponent<CrouchController>();
		wallRunController = GetComponent<WallRunController>();
		shootContoller = GetComponent<ShootController>();
		//Initiliaze crouch controller variables
		crouchController.CalculateCrouchVars(this.gameObject, playerCamera.gameObject, movementSpeed);

		//Initialize player looking mechanics
		playerLook.Init(transform, playerCamera.transform);
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
		Vector3 testV = new Vector3(inputState.playerVelocity.x, 0, inputState.playerVelocity.z);
		Debug.DrawRay(transform.position, testV);

		//Keep track ourselves if we are grounded or not
		inputState.playerIsGrounded = cc.isGrounded;
		//Update variables based on options menu selections
		GatherOptions();
		//Gather all mouse and keyboard inputs if we aren't paused
		GatherInputs();
		//Handle any mouse input that occurred
		HandleMouseInput();
		//Handle crouching
		//crouchController.HandleCrouching(cc, playerCamera, playerBody, isCTRLDown);
		//Handle the movement of the player
		HandleMovement();
		//Apply gravity
		HandleGravity();
		//Handle jumping of the player
		HandleJumping();
		//Tell the wall-run controller to handle any wall-running tasks
		//wallRunController.HandleWallRunning(inputState.playerVelocity, playerBody, playerIsGrounded, ref jumps);
		//Tell the wall-run controller to also handle any wall-sticking tasks
		//wallRunController.HandleWallSticking(shootContoller.isAiming);
		//Set a flag if we're airborne this frame
		if(!inputState.playerIsGrounded)
		{
			wasAirborne = true;
		}
		else 
		{
			wasAirborne = false;
		}
		//Linear drag along the X and Z while grounded
		if(inputState.playerIsGrounded)
		{
			inputState.playerVelocity.x *= 0.9f;
			inputState.playerVelocity.z *= 0.9f;
		}
		//Move the char controller
		cc.Move(inputState.playerVelocity * Time.deltaTime);
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

	private void GetMoveInput()
	{
		if (inputs != null)
		{
			//Gather input axis and set movement accordingly
			float forwardDir = 0f;
			float sideDir = 0f;
			if (inputState.GetButtonPressed(inputs[0])) 
			{
				forwardDir += inputState.GetButtonValue(inputs[0]);
			}
			if (inputState.GetButtonPressed(inputs[1])) 
			{
				forwardDir += inputState.GetButtonValue(inputs[1]);
			}
			if (inputState.GetButtonPressed(inputs[2])) 
			{
				sideDir += inputState.GetButtonValue(inputs[2]);
			}
			if (inputState.GetButtonPressed(inputs[3])) 
			{
				sideDir += inputState.GetButtonValue(inputs[3]);
			}

			//Store values in an input vector
			moveInput = new Vector2(forwardDir, sideDir);

			//Normalize if values exceed 1, this prevents faster diagonal movement
			if (moveInput.sqrMagnitude > 1) 
			{
				moveInput.Normalize();
			}

			forwardSpeed = moveInput.x;
			sideSpeed = moveInput.y;
		}
	}

	private void GetLookInput()
	{
		if (inputs != null)
		{
			float horizontalRot = 0f;
			float verticalRot = 0f;

			verticalRot += inputState.GetButtonValue(inputs[4]);
			verticalRot += inputState.GetButtonValue(inputs[5]);
			horizontalRot += inputState.GetButtonValue(inputs[6]);
			horizontalRot += inputState.GetButtonValue(inputs[7]);

			lookInput = new Vector2(verticalRot, horizontalRot);
			//Debug.Log(lookInput);
		}
	}

	void GatherInputs()
	{
		//Only gather user input if we're not paused
		if(gm.GetGameState() == GameManager.GameState.playing)
		{
			GetLookInput();
			//Keyboard movement inputs
			GetMoveInput();
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

	void HandleMouseInput()
	{
		//Apply any horizontal look rotation
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
			playerLook.LookRotation(transform, playerCamera.transform, lookInput);
		}
		//Calculate the angle we should tilt the camera depending on wall-run side
		float cameraRotZ = wallRunController.CalculateCameraTilt(playerCamera);
	}

	//Handle any WASD or arrow key movement
	void HandleMovement()
	{
		//Decrement the walk/run audio timers
		walkTimer -= Time.deltaTime;
		runTimer -= Time.deltaTime;

		//Grounded movement
		if(inputState.playerIsGrounded)
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
				inputState.playerVelocity.x = 0;
				inputState.playerVelocity.z = 0;
			} 
			else 
			{
				inputState.playerVelocity += forwardSpeed * transform.forward;
				inputState.playerVelocity += sideSpeed * transform.right;
				//inputState.playerVelocity += forwardSpeed * transform.forward * Time.deltaTime;
				//inputState.playerVelocity += sideSpeed * transform.right * Time.deltaTime;
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
				inputState.playerVelocity = Vector3.zero;
			}
			//If we're wall-running
			else if(wallRunController.isWallRunning())
			{
				//Calculate our wall-running velocity
				inputState.playerVelocity = wallRunController.SetWallRunVelocity(inputState.playerVelocity, isWPressed, isSPressed);
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
				if(forwardSpeed != 0 && inputState.allowAirMovement)
				{
					inputState.playerVelocity += forwardSpeed * transform.forward;
					//inputState.playerVelocity += forwardSpeed * transform.forward * Time.deltaTime;
				} 
				if(sideSpeed != 0 && inputState.allowAirMovement) 
				{
					inputState.playerVelocity += sideSpeed * transform.right;
					//inputState.playerVelocity += sideSpeed * transform.right * Time.deltaTime;
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
			if(!inputState.playerIsGrounded)
			{
				//Add gravity only when we aren't on the ground
				//inputState.playerVelocity += Physics.gravity;
				inputState.playerVelocity += Physics.gravity * Time.deltaTime;
			} 
		}
		//If we're wall-running, lower the gravity to simulate it
		else if(!inputState.playerIsGrounded)
		{
			//Slow our descent
			if(inputState.playerVelocity.y <= 0)
			{
				//inputState.playerVelocity += (Physics.gravity/4);
				inputState.playerVelocity += (Physics.gravity/4) * Time.deltaTime;
			}
			//Otherwise use normal gravity
			else 
			{
				inputState.playerVelocity += (Physics.gravity/1.5f) * Time.deltaTime;
			}
		}
	}

	//Push the player upwards if we jumped
	void HandleJumping()
	{
		//If we just landed on the ground
		if(wasAirborne && inputState.playerIsGrounded)
		{
			//Add a head bob to our landing
			StartCoroutine(jumpBob.DoBobCycle());
			//Play a networked landing sound
			fxManager.GetComponent<PhotonView>().RPC("LandingFX", PhotonTargets.All, this.transform.position);
			//Reset our air movement flag
			inputState.allowAirMovement = false;
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
			inputState.playerVelocity = inputState.playerVelocity + (-transform.forward * 7);
		}
		if(isAPressed)
		{
			inputState.playerVelocity = inputState.playerVelocity + (-transform.right * 7);
		}
		if(isDPressed)
		{
			inputState.playerVelocity = inputState.playerVelocity + (transform.right * 7);
		}
		//Apply upward force
		inputState.playerVelocity.y = jumpSpeed;
	}

	private void PlayFootStepAudio(bool isSprinting, bool isWallRunning)
	{
		if (!inputState.playerIsGrounded && !isWallRunning && !aSource.isPlaying)
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
}