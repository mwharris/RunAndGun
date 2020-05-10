using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D_PlayerJump : AbstractBehavior
{
	public int maxJumps;
	public float jumpSpeed = 8f;
	public AudioClip jumpSound;
    
    public Vector3 jumpCCCenter;
    public Vector3 jumpCapsuleColCenter;
    public float jumpCapsuleColHeight;
    public Vector3 jumpBoxColCenter;

    private Vector3 standardCCCenter;
    private Vector3 standardCapsuleColCenter;
    private float standardCapsuleColHeight;
    private Vector3 standardBoxColCenter;

    private Transform playerCamera;
    private int jumps;
    private bool justJumped = false;
    private bool jumpResetting = false;
	private D_WallRunController _dWallRunController;
	private FXManager fxManager;
	private GameManager gm;
	private AudioSource aSource;

    private BodyController bodyControl;
    private CharacterController cc;
    private CapsuleCollider capsuleCol;
    private BoxCollider boxCol;

    void Start()
	{
		jumps = maxJumps;
        bodyControl = GetComponent<BodyController>();
        playerCamera = bodyControl.PlayerBodyData.playerCamera;
		_dWallRunController = GetComponent<D_WallRunController>();
        fxManager = GameObject.FindObjectOfType<FXManager>();
		aSource = GetComponent<AudioSource>();
		gm = GameObject.FindObjectOfType<GameManager>();
        //Get components and variables needed for hitbox manipulation
        SetHitboxControlVars(gameObject);
    }

    private void SetHitboxControlVars(GameObject player)
    {
        cc = GetComponent<CharacterController>();
        capsuleCol = GetComponent<CapsuleCollider>();
        boxCol = GetComponent<BoxCollider>();
        standardCCCenter = cc.center;
        standardCapsuleColCenter = capsuleCol.center;
        standardCapsuleColHeight = capsuleCol.height;
        standardBoxColCenter = boxCol.center;
    }

    void Update()
    {
        if (gm.GetGameState() == GameManager.GameState.playing)
        {
            playerCamera = bodyControl.PlayerBodyData.playerCamera;

            //Gather inputs needed for jumping
            bool canJump = inputState.GetButtonPressed(inputs[0]) && inputState.GetButtonHoldTime(inputs[0]) == 0;

            //Reset our jumps if we're grouded
            if (inputState.playerIsGrounded && !justJumped)
            {
                jumps = maxJumps;
                jumpResetting = true;
            }

            //Perform a jump if we've jumped
            if (canJump && jumps > 0)
            {
                //Decrement our jumps so we can only jump twice
                jumps--;
                justJumped = true;
                inputState.playerIsJumping = true;
                //Play a sound of use jumping
                PlayJumpSound(!inputState.playerIsGrounded);
                //Add an immediate velocity upwards to jump
                inputState.playerVelocity.y = jumpSpeed;
                //If we're wall-running, angle our jump outwards
                if (_dWallRunController.isWallRunning())
                {
                    //Handle double jumping
                    inputState.playerVelocity = _dWallRunController.WallJump(inputState.playerVelocity, jumpSpeed, playerCamera);
                }
                else
                {
                    //Determine if we jumped straight upwards
                    if (inputState.playerVelocity.x == 0 && inputState.playerVelocity.z == 0)
                    {
                        inputState.allowAirMovement = true;
                    }
                    else
                    {
                        inputState.allowAirMovement = false;
                    }
                    //Add a little horizontal movement if we double jumped while holding a key
                    if (!inputState.playerIsGrounded)
                    {
                        //Handle double jumping
                        RotateDoubleJump();
                    }
                }
            }
            else
            {
                justJumped = false;
                inputState.playerIsJumping = false;
            }

            //Update our hitboxes
            if (!GetComponent<PhotonView>().isMine)
            {
                bool hitboxJumping = inputState.playerIsJumping || justJumped || !inputState.playerIsGrounded;
                HandleHitboxes(gameObject, hitboxJumping, inputState.playerIsCrouching, jumpResetting);
            }
        }
    }

    public void HandleHitboxes(GameObject player, bool isJumping, bool isCrouching, bool isJumpResetting)
    {
        //Make sure our character controller is not null
        if (cc == null && player != null)
        {
            SetHitboxControlVars(player);
        }
        //Get the current versions of all our variables
        float currCCHeight = cc.height;
        Vector3 currCapColCenter = capsuleCol.center;
        float currCapColHeight = capsuleCol.height;
        Vector3 currBoxColCenter = boxCol.center;
        //Lerp depending on if we're jumping or not
        if (isJumping)
        {
            if (currCapColCenter != jumpCapsuleColCenter)
            {
                currCapColCenter = Vector3.Lerp(currCapColCenter, jumpCapsuleColCenter, Time.deltaTime * 4f);
            }
            if (currCapColHeight != jumpCapsuleColHeight)
            {
                currCapColHeight = jumpCapsuleColHeight;
            }
            if (currBoxColCenter != jumpBoxColCenter)
            {
                currBoxColCenter = Vector3.Lerp(currBoxColCenter, jumpBoxColCenter, Time.deltaTime * 4f);
            }
        }
        else if (!isCrouching && isJumpResetting)
        {
            bool allGood = true;
            if (currCapColCenter != standardCapsuleColCenter)
            {
                currCapColCenter = Vector3.Lerp(currCapColCenter, standardCapsuleColCenter, Time.deltaTime * 8f);
                if (Mathf.Abs(currCapColCenter.y - standardCapsuleColCenter.y) <= 0.1f)
                {
                    currCapColCenter = standardCapsuleColCenter;
                }
                else
                {
                    allGood = false;
                }
            }
            if (currCapColHeight != standardCapsuleColHeight)
            {
                currCapColHeight = standardCapsuleColHeight;
            }
            if (currBoxColCenter != standardBoxColCenter)
            {
                currBoxColCenter = Vector3.Lerp(currBoxColCenter, standardBoxColCenter, Time.deltaTime * 8f);
                if (Mathf.Abs(currBoxColCenter.y - standardBoxColCenter.y) <= 0.1f)
                {
                    currBoxColCenter = standardBoxColCenter;
                }
                else
                {
                    allGood = false;
                }
            }
            if (isJumpResetting && allGood)
            {
                isJumpResetting = false;
            }
        }
        //Set our variables to the new lerped values
        cc.height = currCCHeight;
        capsuleCol.center = currCapColCenter;
        capsuleCol.height = currCapColHeight;
        boxCol.center = currBoxColCenter;
    }

    /*
	 * This function handles double jumping by rotating the current velocity
	 * toward the direction the player is holding relative to their current
	 * look position.  This applies no boost unlike ForceDoubleJump().
	*/
    private void RotateDoubleJump()
	{
		//Get keyboard or controller input using raw axis values
		float forwardAxis = inputState.GetButtonValue(inputs[1]);
		float backwardAxis = inputState.GetButtonValue(inputs[2]);
		float leftAxis = inputState.GetButtonValue(inputs[3]);
		float rightAxis = inputState.GetButtonValue(inputs[4]);
		//Determine our target jump direction relative to player forward based on input
		Vector3 targetDir = transform.TransformDirection(new Vector3(leftAxis + rightAxis, 0, forwardAxis + backwardAxis));
		//Reset the y-velocity for rotation calculations
		inputState.playerVelocity.y = 0;
		//Find the angle, in radians, between our target direction and current direction
		float degrees = Vector3.Angle(inputState.playerVelocity, targetDir);
		float radians = degrees * Mathf.Deg2Rad;
		//Rotate the current direction the amount of radians determined above
		inputState.playerVelocity = Vector3.RotateTowards(inputState.playerVelocity, targetDir, radians, 0.0f);
		//Jump upwards
		inputState.playerVelocity.y = jumpSpeed;
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

	public void ResetJumps()
	{
		jumps = maxJumps;
	}
}