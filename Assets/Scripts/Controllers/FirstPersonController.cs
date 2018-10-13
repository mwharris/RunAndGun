using UnityEngine;
using System.Collections;

[RequireComponent(typeof (CrouchController))]
[RequireComponent(typeof (WallRunController))]
[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
public class FirstPersonController : AbstractBehavior 
{
	[HideInInspector] public CharacterController cc;
	public AudioClip jumpSound;  
   
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
    public float crouchTimer = 0f;
	private float origWalkTimer;
	private float origRunTimer;
    private float origCrouchTimer;
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
	private FXManager fxManager;
	private BobController bobScript;
    private BodyController bodyControl;
    [SerializeField] private PlayerLook playerLook;
    ////////////////////////////////////////////

    private Camera playerCamera;

    private bool dontMove = false;

    void Start () {
        //Setup our body controller, body data, and camera
        bodyControl = GetComponent<BodyController>();
        playerCamera = bodyControl.PlayerBodyData.playerCamera.GetComponent<Camera>();
		ogCamPos = playerCamera.transform.localPosition;
		//Initialize a reference to the character controller component
		cc = GetComponent<CharacterController>();
		//Get a reference to the audio source
		aSource = GetComponent<AudioSource>();
		//Keep track of how long our walk audio delay should be
		origWalkTimer = walkTimer;
		origRunTimer = runTimer;
        origCrouchTimer = crouchTimer;
		//Initialize a reference to various scripts we need
		fxManager = GameObject.FindObjectOfType<FXManager>();
        bobScript = playerCamera.GetComponent<BobController>();
		gm = GameObject.FindObjectOfType<GameManager>();
		menuController = GameObject.FindObjectOfType<MenuController>();
		//Set up the various controllers
		crouchController = GetComponent<CrouchController>();
		wallRunController = GetComponent<WallRunController>();
        //Initiliaze crouch controller variables
        crouchController.CalculateCrouchVars(this.gameObject, playerCamera.gameObject, movementSpeed);
        //Initialize player looking mechanics
        playerLook.Init(transform, playerCamera.transform, bodyControl.PlayerBodyData);
    }

    private void LateUpdate()
    {
        HandleControllerInput();
    }

    void Update () {
        //Test stuff
        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        Debug.DrawRay(rayPos, transform.right * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Debug.DrawRay(rayPos, -transform.right * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Debug.DrawRay(rayPos, transform.forward * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Debug.DrawRay(rayPos, -transform.forward * (wallRunController.isWallRunning() ? 1.5f : 0.825f));
		Vector3 testV = new Vector3(inputState.playerVelocity.x, 0, inputState.playerVelocity.z);
		Debug.DrawRay(rayPos, testV);

		//Keep track ourselves if we are grounded or not
		inputState.playerIsGrounded = cc.isGrounded;
		//Update variables based on options menu selections
		GatherOptions();
		//Gather all mouse and keyboard inputs if we aren't paused
		GatherInputs();
		//Handle crouching
		crouchController.HandleCrouching(cc, playerCamera, gm.GetGameState());
		//Handle the movement of the player
		HandleMovement();
        //Handle any mouse input that occurred
        HandleControllerInput();
        //Apply gravity
        HandleGravity();
		//Handle jumping of the player
		HandleJumping();
        //Tell the wall-run controller to also handle any wall-sticking tasks
        wallRunController.HandleWallSticking();
        //Tell the wall-run controller to handle any wall-running tasks
        wallRunController.HandleWallRunning(inputState.playerVelocity, inputState.playerIsGrounded);//, ref jumps);
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
        if (Input.GetKey(KeyCode.L))
        {
            dontMove = !dontMove;
        }
        if (dontMove)
        {
            Vector3 stopV = new Vector3(0, inputState.playerVelocity.y, 0);
            cc.Move(stopV * Time.deltaTime);
        }
        else
        {
            cc.Move(inputState.playerVelocity * Time.deltaTime);
        }
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
        //Dynamically update player body data variables
        playerCamera = bodyControl.PlayerBodyData.playerCamera.GetComponent<Camera>();
        //Only gather user input if we're not paused
        if (gm.GetGameState() == GameManager.GameState.playing)
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
        LookRotationInput lri = new LookRotationInput(transform, playerCamera.transform, lookInput, mouseSensitivity, 
            invertY, inputState.playerIsAiming, wallRunController.isWallRunning(), 0f, 0f, 0f, false);
        //Handle any look rotation updates due to wall-running
        wallRunController.SetWallRunLookRotationInputs(lri, playerCamera, inputState.playerVelocity);
        //Finally apply our look rotations and calculate head angle
        inputState.playerLookAngle = playerLook.LookRotation(lri);
    }

	//Handle any WASD or arrow key movement
	void HandleMovement()
	{
		//Decrement the walk/run audio timers
		walkTimer -= Time.deltaTime;
		runTimer -= Time.deltaTime;
        crouchTimer -= Time.deltaTime;

		//Grounded movement
		if(inputState.playerIsGrounded)
        {
            //Make sure we reset wall-sticking vars
            wallRunController.wallStickVelocitySet = false;
            //Apply movement speed based on crouching, sprinting, or standing
            if (inputState.playerIsCrouching || inputState.playerIsAiming)
			{
				forwardSpeed *= crouchController.CrouchMovementSpeed;
				sideSpeed *= crouchController.CrouchMovementSpeed;
                //Disable head/body bob while crouching
                bobScript.enabled = false;
			}
			else if(inputState.playerIsSprinting)
			{
				forwardSpeed *= movementSpeed * 1.5f;
				sideSpeed *= movementSpeed * 1.5f;
                //Enable head/body bob while sprinting
                bobScript.enabled = true;
			}
			else
			{
				forwardSpeed *= movementSpeed;
				sideSpeed *= movementSpeed;
                //Disable head bob while walking
                bobScript.enabled = false;
			}

			//Play sprinting FX while sprinting
			if(forwardSpeed != 0 || sideSpeed != 0)
			{
				//Increase footstep rate
				PlayFootStepAudio(inputState.playerIsSprinting, false, inputState.playerIsCrouching, inputState.playerIsAiming);
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
            //Disable head/body bob
            bobScript.enabled = false;

            //If we're wall-sticking
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
                PlayFootStepAudio(false, true, false, false);
			}
			//If we're airborne and not wall-running
			else
            {
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

	private void PlayFootStepAudio(bool isSprinting, bool isWallRunning, bool isCrouching, bool isAiming)
	{
        //No sounds when we're traveling through the air
		if (!inputState.playerIsGrounded && !isWallRunning && !aSource.isPlaying)
		{
			return;
		}
        //Only play sounds after one of the timers are at 0
        bool walkTimerUp = !isSprinting && !isCrouching && !isAiming && walkTimer <= 0;
        bool sprintTimerUp = (isSprinting || isWallRunning) && runTimer <= 0;
        bool crouchTimerUp = (isCrouching || isAiming) && crouchTimer <= 0;
        if (walkTimerUp || sprintTimerUp || crouchTimerUp)
		{
			//Reset the audio timers
			walkTimer = origWalkTimer;
			runTimer = origRunTimer;
            crouchTimer = origCrouchTimer;
			//Play a networked walking sound
			fxManager.GetComponent<PhotonView>().RPC("FootstepFX", PhotonTargets.All, this.transform.position);
		}
	}

    //Suicide button for testing respawn
    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 40), "Third Person!"))
        {
            //Disable the First Person arms
            Animator anim = this.transform.GetChild(0).GetComponentInChildren<Animator>();
            foreach (Transform t in anim.transform.GetComponentInChildren<Transform>())
            {
                t.gameObject.SetActive(false);
            }
            //Enable the third person body
            Transform body = this.transform.GetChild(1);
            Transform animatedBody = body.GetChild(0);
            for (int i = 0; i < animatedBody.childCount; i++)
            {
                Transform child = animatedBody.GetChild(i);
                child.gameObject.SetActive(true);
            }
            //TPS Camera
            Transform cam = body.GetChild(1);
            cam.GetChild(0).GetComponent<Camera>().enabled = true;
            //cam.GetChild(0).localPosition = new Vector3(0f, 2.67f, -3.69f);
            //cam.GetChild(0).localRotation = Quaternion.Euler(7.94f, 0f, 0f);
            //Tell the Player Body Data to switch
            GetComponent<BodyController>().ForceThird();
            playerLook.UpdateBodyData(GetComponent<BodyController>().PlayerBodyData);
        }
    }
}
