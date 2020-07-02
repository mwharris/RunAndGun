using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D_CrouchController : AbstractBehavior
{
	private float crouchMovementSpeed;
	public float CrouchMovementSpeed {
		get { return crouchMovementSpeed; }
	}
    
    //Special flag to mark the player camera as changing positions, used in FirstPersonController
    [HideInInspector] public bool cameraResetting = false;

	[SerializeField] private float crouchSpeed = 8.0f;
	[SerializeField] private float crouchCamHeight;
	[SerializeField] private float crouchCCHeight;
    [SerializeField] private float crouchCCRadius;
    [SerializeField] private Vector3 crouchCCCenter;
    [SerializeField] private float crouchBodyZ;
    [SerializeField] private float crouchHeadZ;
    [SerializeField] private float crouchHeadY;
    //[SerializeField] private float crouchHeadScale;

    private CharacterController cc;
    private CapsuleCollider shotCollider;
    private BoxCollider headCollider;
    private Transform thirdPersonBody;

    private float crouchDeltaDepth;
    private Vector3 crouchDeltaCCCenter;

	private float standardCamHeight;
	private float standardCCHeight;
    private Vector3 standardCCCenter;
    private float standardCCRadius;
    private float standardBodyZ;
    private float standardHeadZ;
    private float standardHeadY;
    //private Vector3 standardHeadScale;

    /**
	 * Calculate some variables needed for crouching logic
	 */
    public void CalculateCrouchVars(GameObject player, GameObject playerCamera, float movementSpeed)
	{
        //Cache references to our two sphere colliders
        cc = player.GetComponent<CharacterController>();
        shotCollider = player.GetComponent<CapsuleCollider>();
        headCollider = player.GetComponent<BoxCollider>();
        //Store the standard camera heights and depths
        standardCamHeight = playerCamera.transform.localPosition.y;
        //Store the standard body/character controller heights and depths
		standardCCHeight = cc.height;
		standardCCCenter = cc.center;
        standardCCRadius = cc.radius;
        thirdPersonBody = player.transform.GetChild(1);
        standardBodyZ = thirdPersonBody.localPosition.z;
        standardHeadZ = headCollider.center.z;
        standardHeadY = headCollider.center.y;
        //standardHeadScale = headCollider.size;
        //Calculate the movement speed while crouched
        crouchMovementSpeed = movementSpeed / 2;
	}

	/**
	 * Handle the overhead rules before performing an actualy crouch.
     * Called from FirstPersonController.cs.
	 */
	public void HandleCrouching(Transform playerCamera, GameManager.GameState gs)
	{
		if (gs == GameManager.GameState.playing) 
		{
			bool isCrouchDown = inputState.GetButtonPressed(inputs[0]) && inputState.GetButtonHoldTime(inputs[0]) == 0;
			if(isCrouchDown)
			{
				ToggleCrouch();
			}
            DoCrouch(playerCamera, inputState.playerIsCrouching, inputState.playerIsGrounded, cameraResetting);
        }    
    }

    public void HandleMultiplayerCrouch(GameObject player, GameObject playerCamera, bool isCrouching, bool isGrounded, bool isCamResetting)
    {
        //When coming from a multiplayer call, make sure our variables are instantiated
        if (cc == null)
        {
            CalculateCrouchVars(player, playerCamera, 1f);
        }
        //Make this player crouch or stand
        DoCrouch(playerCamera.transform, isCrouching, isGrounded, isCamResetting);
    }

    /**
	 * Handle the actual shrinking / expanding of the player and components when crouching / standing.
     * Public because is called by NetworkCharacter.cs to cut down on sending these vars over the network.
	 */
    public void DoCrouch(Transform playerCamera, bool isCrouching, bool isGrounded, bool isCameraResetting)
    {
        //Store the local position for modification
        Vector3 camLocalPos = playerCamera.transform.localPosition;
        float ccHeight = cc.height;
        float ccRadius = cc.radius;
        Vector3 ccCenter = cc.center;
        float shotColHeight = shotCollider.height;
        Vector3 shotColCenter = shotCollider.center;
        float currBodyZ = thirdPersonBody.localPosition.z;
        float currHeadZ = headCollider.center.z;
        float currHeadY = headCollider.center.y;
        //Modify the local position over time based on if we are/aren't crouching
        if (isCrouching && isGrounded)
        {
            if (ccHeight > crouchCCHeight)
            {
                ccHeight = Mathf.Lerp(ccHeight, crouchCCHeight, Time.deltaTime * 4f);
                shotColHeight = ccHeight;
            }
            if (ccRadius < crouchCCRadius)
            {
                ccRadius = Mathf.Lerp(ccRadius, crouchCCRadius, Time.deltaTime * 4f);
            }
            if (ccCenter != crouchCCCenter)
            {
                ccCenter = new Vector3(0, (ccHeight / 2) + 0.3f, 0);
                shotColCenter = new Vector3(0, (shotColHeight / 2) + 0.3f, 0);
            }
            if (camLocalPos.y > crouchCamHeight)
            {
                camLocalPos.y = Mathf.Lerp(camLocalPos.y, crouchCamHeight, Time.deltaTime * 4f);
            }
            if (currBodyZ > crouchBodyZ)
            {
                currBodyZ = Mathf.Lerp(currBodyZ, crouchBodyZ, Time.deltaTime * 8f);
            }
            if (currBodyZ > crouchBodyZ)
            {
                currBodyZ = Mathf.Lerp(currBodyZ, crouchBodyZ, Time.deltaTime * 8f);
            }
            if (currHeadZ < crouchHeadZ)
            {
                currHeadZ = Mathf.Lerp(currHeadZ, crouchHeadZ, Time.deltaTime * 8f);
            }
            if (currHeadY > crouchHeadY)
            {
                currHeadY = Mathf.Lerp(currHeadY, crouchHeadY, Time.deltaTime * 8f);
            }
        }
        //Coming back up
        else if (isCameraResetting && isGrounded)
        {
            bool allGood = true;
            if (ccHeight < standardCCHeight)
            {
                ccHeight = Mathf.Lerp(ccHeight, standardCCHeight, Time.deltaTime * 8f);
                shotColHeight = ccHeight - 0.2f;
                if (Mathf.Abs(ccHeight - standardCCHeight) <= 0.1f)
                {
                    ccHeight = standardCCHeight;
                }
                else
                {
                    allGood = false;
                }
            }
            if (ccRadius > standardCCRadius)
            {
                ccRadius = Mathf.Lerp(ccRadius, standardCCRadius, Time.deltaTime * 8f);
                if (Mathf.Abs(ccRadius - standardCCRadius) <= 0.1f)
                {
                    ccRadius = standardCCRadius;
                }
                else
                {
                    allGood = false;
                }
            }
            if (ccCenter != standardCCCenter)
            {
                ccCenter = new Vector3(0, (ccHeight / 2) + 0.3f, 0);
                shotColCenter = new Vector3(0, (shotColHeight / 2) + 0.3f, 0);
            }
            if (currBodyZ < standardBodyZ)
            {
                currBodyZ = Mathf.Lerp(currBodyZ, standardBodyZ, Time.deltaTime * 8f);
                if (Mathf.Abs(currBodyZ - standardBodyZ) <= 0.1f)
                {
                    currBodyZ = standardBodyZ;
                }
                else
                {
                    allGood = false;
                }
            }
            if (currHeadZ > standardHeadZ)
            {
                currHeadZ = Mathf.Lerp(currHeadZ, standardHeadZ, Time.deltaTime * 8f);
                if (Mathf.Abs(currHeadZ - standardHeadZ) <= 0.1f)
                {
                    currHeadZ = standardHeadZ;
                }
                else
                {
                    allGood = false;
                }
            }
            if (currHeadY < standardHeadY)
            {
                currHeadY = Mathf.Lerp(currHeadY, standardHeadY, Time.deltaTime * 8f);
                if (Mathf.Abs(currHeadY - standardHeadY) <= 0.1f)
                {
                    currHeadY = standardHeadY;
                }
                else
                {
                    allGood = false;
                }
            }
            if (camLocalPos.y < standardCamHeight)
            {
                camLocalPos.y = Mathf.Lerp(camLocalPos.y, standardCamHeight, Time.deltaTime * 8f);
                if (Mathf.Abs(camLocalPos.y - standardCamHeight) <= 0.1f)
                {
                    camLocalPos.y = standardCamHeight;
                }
                else
                {
                    allGood = false;
                }
            }
            //Special case: when we are standing, we need to mark the camera as being moved since other scripts try to adjust the camera's position while standing
            if (allGood && isCameraResetting)
            {
                isCameraResetting = false;
            }
        }
        //Apply the local position updates
        playerCamera.transform.localPosition = camLocalPos;
        cc.height = ccHeight;
        cc.radius = ccRadius;
        cc.center = ccCenter;
        shotCollider.height = shotColHeight;
        shotCollider.center = shotColCenter;
        headCollider.center = new Vector3(headCollider.center.x, currHeadY, currHeadZ);
        thirdPersonBody.localPosition = new Vector3(thirdPersonBody.localPosition.x, thirdPersonBody.localPosition.y, currBodyZ);
    }

    /**
        * Helper function to toggle crouching flags
        */
    public void ToggleCrouch()
	{
		if(inputState.playerIsCrouching)
		{
			StopCrouching();
		}
		else 
		{
			Crouch();
            //Make sure we disable sprinting
            inputState.playerIsSprinting = false;
        }
	}

	/**
	 * Public methods to modify isCrouching variable 
	 */
	public void Crouch()
	{
		inputState.playerIsCrouching = true;
	}
	public void StopCrouching()
	{
		inputState.playerIsCrouching = false;
        //Flag the camera as being moved
        cameraResetting = true;
    }
}
