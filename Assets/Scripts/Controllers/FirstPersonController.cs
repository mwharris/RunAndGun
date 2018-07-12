using UnityEngine;
using System.Collections;

[RequireComponent(typeof (CrouchController))]
[RequireComponent(typeof (WallRunController))]
[RequireComponent(typeof (ShootController))]
[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
[RequireComponent(typeof (LerpControlledBob))]
public class FirstPersonController : AbstractBehavior 
{
	[HideInInspector] public CharacterController cc;
	public GameObject playerBody;
	public AudioClip jumpSound;  

	[SerializeField] private Camera playerCamera;   
	private Vector3 ogCamPos;
	private AudioSource aSource;    
	private Vector2 lookInput;
	private Vector2 moveInput;

	/// OPTIONS VARIABLES //////////////////////
	private float mouseSensitivity = 5.0f;
	private bool invertY = false;
	////////////////////////////////////////////

	/// MOUSE CONTROL VARIABLES ////////////////
	public float horizontalRotation = 0f;
	public float verticalRotation = 0f;
	public float verticalVelocity = 0f;
	public float upDownRange = 60.0f;
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
	private bool isADown = false;
	private bool isSDown = false;
	private bool isDDown = false;
	////////////////////////////////////////

	/// IMPORTED SCRIPTS //////////////////
	private GameManager gm;
	private CrouchController crouchController;
	private WallRunController wallRunController;
	private MenuController menuController;
	private ShootController shootContoller;
	private FXManager fxManager;
	private MyHeadBob headBobScript;
	private LerpControlledBob jumpBob;
    [SerializeField] private PlayerLook playerLook;
    ////////////////////////////////////////////

    void Start () {
		ogCamPos = playerCamera.transform.localPosition;
		//Initialize a reference to the character controller component
		cc = GetComponent<CharacterController>();
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
        jumpBob = GetComponent<LerpControlledBob>();
        //Initiliaze crouch controller variables
        crouchController.CalculateCrouchVars(this.gameObject, playerCamera.gameObject, movementSpeed);
        //Initialize player looking mechanics
        playerLook.Init(transform, playerCamera.transform);
    }

    void Update () {
		//Test stuff
		Debug.DrawRay(transform.position, transform.right * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Debug.DrawRay(transform.position, -transform.right * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Debug.DrawRay(transform.position, transform.forward * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Debug.DrawRay(transform.position, -transform.forward * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Vector3 testV = new Vector3(inputState.playerVelocity.x, 0, inputState.playerVelocity.z);
		Debug.DrawRay(transform.position, testV);

		//Keep track ourselves if we are grounded or not
		inputState.playerIsGrounded = cc.isGrounded;
		//Update variables based on options menu selections
		GatherOptions();
		//Gather all mouse and keyboard inputs if we aren't paused
		GatherInputs();
		//Handle crouching
		crouchController.HandleCrouching(cc, playerCamera, playerBody, gm.GetGameState());
		//Handle the movement of the player
		HandleMovement();
        //Handle any mouse input that occurred
        HandleControllerInput();
        //Apply gravity
        HandleGravity();
		//Handle jumping of the player
		HandleJumping();
        //Tell the wall-run controller to also handle any wall-sticking tasks
        //wallRunController.HandleWallSticking(shootContoller.isAiming);
        //Tell the wall-run controller to handle any wall-running tasks
        //wallRunController.HandleWallRunning(inputState.playerVelocity, playerBody, inputState.playerIsGrounded);//, ref jumps);
        //Set a flag if we're airborne this frame
        if (!inputState.playerIsGrounded)
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

    //Used to apply rotation and position updates to the camera
    void FixedUpdate()
    {
        /*
        //Crouching camera changes clash with jump bob camera changes
		if(!inputState.playerIsCrouching && !crouchController.cameraResetting) 
		{
			//Apply updates to local position from crouching and head bob
			Vector3 localPos = playerCamera.transform.localPosition;
            //Not crouching so reset camera to normal position
			localPos = new Vector3(localPos.x, ogCamPos.y - jumpBob.Offset(), localPos.z);
            //Apply the changes we made above
            playerCamera.transform.localPosition = localPos;
        }
        */
    }

    //Used to apply changes to body after animations have run
    private void LateUpdate()
    {
        //Build a LookRotationInput object for better passing of arguments in the following function calls
        LookRotationInput lri = new LookRotationInput(transform, playerCamera.transform, lookInput, mouseSensitivity, invertY, 0f, new Vector3(), 0f, 0f, false);
        //Rotate the head up/down depending of mouse input
        playerLook.HeadRotation(lri);
    }

    void GatherOptions()
	{
		if (menuController != null) {
			mouseSensitivity = menuController.mouseSensitivity;
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
			if (isWDown) 
			{
				forwardDir += inputState.GetButtonValue(inputs[0]);
			}
			if (isSDown) 
			{
				forwardDir += inputState.GetButtonValue(inputs[1]);
			}
			if (isADown) 
			{
				sideDir += inputState.GetButtonValue(inputs[2]);
			}
			if (isDDown) 
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
		}
	}

	void GatherInputs()
	{
		//Only gather user input if we're not paused
		if(gm.GetGameState() == GameManager.GameState.playing)
		{
			//Input from Mouse or Right Stick
			GetLookInput();
			//Input from WASD or Left Stick
			GetMoveInput();
			//Map inputState values to more manageable variables
			isWDown = inputState.GetButtonPressed(inputs[0]);
			isADown = inputState.GetButtonPressed(inputs[2]);
			isSDown = inputState.GetButtonPressed(inputs[1]);
			isDDown = inputState.GetButtonPressed(inputs[3]);
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
			isADown = false;
			isSDown = false;
			isDDown = false;
		}
	}

	void HandleControllerInput()
	{
        //Build a LookRotationInput object for better passing of arguments in the following function calls
        LookRotationInput lri = new LookRotationInput(transform, playerCamera.transform, lookInput, mouseSensitivity, invertY, 0f, new Vector3(), 0f, 0f, false);
        //Handle any look rotation updates due to wall-running
        wallRunController.SetWallRunLookRotationInputs(lri, playerCamera, inputState.playerVelocity);
        //Finally apply our look rotation
        playerLook.LookRotation(lri);
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
            //Make sure we reset wall-sticking vars
            wallRunController.wallStickVelocitySet = false;
            //Apply movement speed based on crouching, sprinting, or standing
            if (inputState.playerIsCrouching || shootContoller.isAiming)
			{
				forwardSpeed *= crouchController.CrouchMovementSpeed;
				sideSpeed *= crouchController.CrouchMovementSpeed;
				//Disable head bob while crouching
				headBobScript.enabled = false;
			}
			else if(inputState.playerIsSprinting)
			{
				forwardSpeed *= movementSpeed * 1.5f;
				sideSpeed *= movementSpeed * 1.5f;
				//Enable head bob while sprinting
				//headBobScript.enabled = true;
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
				PlayFootStepAudio(inputState.playerIsSprinting, false);
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
            /*
            if (wallRunController.wallSticking)
            {
                //Calculate our wall-running velocity in order to set clamp angles
                if (!wallRunController.wallStickVelocitySet)
                {
                    wallRunController.wallStickVelocitySet = true;
                    wallRunController.SetWallRunVelocity(inputState.playerVelocity, isWDown, isSDown);
                }
                //Stop all movement
                inputState.playerVelocity = Vector3.zero;
            }
			//If we're wall-running
			else if(wallRunController.isWallRunning())
            {
                //Make sure we reset wall-sticking vars
                wallRunController.wallStickVelocitySet = false;
                //Calculate our wall-running velocity
                inputState.playerVelocity = wallRunController.SetWallRunVelocity(inputState.playerVelocity, isWDown, isSDown);
                //Play footstep FX while wall-running
                PlayFootStepAudio(false, true);
			}
            */
			//If we're airborne and not wall-running
			//else
            //{
                //Make sure we reset wall-sticking vars
                wallRunController.wallStickVelocitySet = false;
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
			//}
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
		if(isSDown)
		{
			inputState.playerVelocity = inputState.playerVelocity + (-transform.forward * 7);
		}
		if(isADown)
		{
			inputState.playerVelocity = inputState.playerVelocity + (-transform.right * 7);
		}
		if(isDDown)
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
