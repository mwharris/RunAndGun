using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class PlayerJump : AbstractBehavior {

	public int maxJumps;
	public float jumpSpeed = 8f;
	public AudioClip jumpSound;  

	private int jumps;
    private bool justJumped = false;
	private WallRunController wallRunController;
	private FXManager fxManager;
	private GameManager gm;
	private AudioSource aSource;

	[SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();

	void Start()
	{
		jumps = maxJumps;
		wallRunController = GetComponent<WallRunController>();
		fxManager = GameObject.FindObjectOfType<FXManager>();
		aSource = GetComponent<AudioSource>();
		gm = GameObject.FindObjectOfType<GameManager>();
	}

	void Update()
	{
		if (gm.GetGameState() == GameManager.GameState.playing) 
		{
			//Gather inputs needed for jumping
			bool canJump = inputState.GetButtonPressed(inputs[0]) && inputState.GetButtonHoldTime(inputs[0]) == 0;
            Debug.Log("Num Jumps: " + jumps);

			//Reset our jumps if we're grouded
			if (inputState.playerIsGrounded && !justJumped) 
			{
                jumps = maxJumps;
			}

			//Perform a jump if we've jumped
			if (canJump && jumps > 0) 
			{
				//Add a head bob to our jump
				StartCoroutine(jumpBob.DoBobCycle());
				//Decrement our jumps so we can only jump twice
				jumps--;
                justJumped = true;
                //Play a sound of use jumping
                PlayJumpSound(!inputState.playerIsGrounded);
				//Add an immediate velocity upwards to jump
				inputState.playerVelocity.y = jumpSpeed;
				//If we're wall-running, angle our jump outwards
				if(wallRunController.isWallRunning())
				{
					//Handle double jumping
					inputState.playerVelocity = wallRunController.WallJump(inputState.playerVelocity, jumpSpeed);
				}
				else
				{
					//Determine if we jumped straight upwards
					if(inputState.playerVelocity.x == 0 && inputState.playerVelocity.z == 0){
						inputState.allowAirMovement = true;
					} else {
						inputState.allowAirMovement = false;
					}
					//Add a little horizontal movement if we double jumped while holding a key
					if(!inputState.playerIsGrounded)
					{
						//Handle double jumping
						RotateDoubleJump();
					}
				}	
			}
            else
            {
                justJumped = false;
            }
		}
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
