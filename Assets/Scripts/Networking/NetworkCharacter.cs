using UnityEngine;
using System.Collections;
using Photon.Pun;

public class NetworkCharacter : Photon.Pun.MonoBehaviourPun, IPunObservable
{
    private InputState inputState;
    private BodyController bodyControl;

    //Player postion and rotation need to be passed so preserve look rotations
    private Vector3 realPos = Vector3.zero;
    private Quaternion realRot = Quaternion.identity; //Maybe only the y-rotation is needed here?...

    //Camera position and rotations 
    //POSITION MIGHT NOT BE NEEDED
    private Vector3 camRealPos = Vector3.zero;
    private Quaternion camRealRot = Quaternion.identity;

    //Animation variables
    private bool isSprinting;
    private bool isAiming;
    private bool isAirborne;
    private bool isCrouching;
    private bool wallRunningLeft;
    private bool wallRunningRight;
    private float jumpSpeed;

    //MAYBE COULD BE SEPARATED INTO BOOLEANS: Forward, Backward, Left, Right?...
    private float forwardSpeed;
    private float sideSpeed;
    private bool crouchReset = false;
    private bool jumpReset = false;

    //Character Controller properties need to be passed due to Crouch animations
    private CharacterController cc;
    private D_CrouchController _dCrouchController;
    private D_PlayerJump jumpController;
    private CapsuleCollider bodyCollider;
    private FixWallRunningAnimation wrAnimFix;

    void Awake()
	{
        cc = GetComponent<CharacterController>();
        bodyCollider = GetComponent<CapsuleCollider>();
        bodyControl = GetComponent<BodyController>();
        _dCrouchController = GetComponent<D_CrouchController>();
        jumpController = GetComponent<D_PlayerJump>();
        inputState = GetComponent<InputState>();
        wrAnimFix = GetComponent<FixWallRunningAnimation>();
    }

	/**
	 * Handle updating non-local player's variables sent over the network
	 */
	void Update()
    {
        //Get the body components we need to update based on if we're Third or First person
        PlayerBodyData playerBodyData = bodyControl.PlayerBodyData;
        Animator bodyAnimator = playerBodyData.GetBodyAnimator();

        //Only update a non-local player. Local players are updated by First Person Controller
        if (!photonView.IsMine)
        {
            float lerpSpeed = Time.deltaTime * 8f;

            //Smooth our movement from the current position to the received position
            //TODO: PREDICTION
            transform.position = SafeLerp(transform.position, realPos, lerpSpeed);
            transform.rotation = SafeLerp(transform.rotation, realRot, lerpSpeed);

            //Smooth our camera movement from the current position to the received position
            playerBodyData.playerCamera.localPosition = SafeLerp(playerBodyData.playerCamera.localPosition, camRealPos, lerpSpeed);
            playerBodyData.playerCamera.localRotation = SafeLerp(playerBodyData.playerCamera.localRotation, camRealRot, lerpSpeed);

            //Animation variables
            bodyAnimator.SetBool("Sprinting", isSprinting);
            bodyAnimator.SetBool("Aiming", isAiming);
            bodyAnimator.SetBool("Jumping", isAirborne);
            bodyAnimator.SetBool("Crouching", isCrouching);
            bodyAnimator.SetBool("WallRunningRight", wallRunningRight);
            bodyAnimator.SetBool("WallRunningLeft", wallRunningLeft);
            bodyAnimator.SetFloat("ForwardSpeed", forwardSpeed);
            bodyAnimator.SetFloat("SideSpeed", sideSpeed);
            bodyAnimator.SetFloat("JumpSpeed", jumpSpeed);

            //Set Capsule Collider and Character Controller variables for crouching
            _dCrouchController.HandleMultiplayerCrouch(gameObject, playerBodyData.playerCamera.gameObject, isCrouching, !isAirborne, crouchReset);

            //Set Capsule Collider and Character Controller variables for jumping
            jumpController.HandleHitboxes(gameObject, isAirborne, isCrouching, jumpReset);

            //Fix for wall-running animations being rotated incorrectly
            wrAnimFix.RunFix(wallRunningLeft, wallRunningRight, Time.deltaTime);
        }
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		OnPhotonSerializeView(stream, info);
	}

	/**
	 * Handle the actual sending / receiving of variables over the network
	 */
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        var playerBodyData = bodyControl.PlayerBodyData;

        if (stream.IsWriting)
		{
			//This is our local player, send our position to the network
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
            //Camera position and rotation
            stream.SendNext(playerBodyData.playerCamera.localPosition);
            stream.SendNext(playerBodyData.playerCamera.localRotation);
            //Send animator variable information
            stream.SendNext(inputState.playerIsSprinting);
            stream.SendNext(inputState.playerIsAiming);
            stream.SendNext(!inputState.playerIsGrounded);
            stream.SendNext(inputState.playerIsCrouching);
            stream.SendNext(inputState.playerIsWallRunningRight);
            stream.SendNext(inputState.playerIsWallRunningLeft);
            stream.SendNext(Vector3.Dot(inputState.playerVelocity, transform.forward));
            stream.SendNext(Vector3.Dot(inputState.playerVelocity, transform.right));
            stream.SendNext(inputState.playerVelocity.y);
		}
		else
		{
            //This is a networked player, receive their position an update the player accordingly
            realPos = (Vector3)stream.ReceiveNext();
            realRot = (Quaternion)stream.ReceiveNext();
            //Camera position and rotation
            camRealPos = (Vector3)stream.ReceiveNext();
            camRealRot = (Quaternion)stream.ReceiveNext();
            //Receive animator variable information
            isSprinting = (bool)stream.ReceiveNext();
            isAiming = (bool)stream.ReceiveNext();
            //Jumping requires some extra logic for the reset flag
            bool nowAirborne = (bool)stream.ReceiveNext();
            if (isAirborne && !nowAirborne) {
                jumpReset = true;
            }
            isAirborne = nowAirborne;
            //Crouching requires some extra logic for the reset flag
            bool nowCrouching = (bool)stream.ReceiveNext();
            if (isCrouching && !nowCrouching) { crouchReset = true; }
            isCrouching = nowCrouching;
            wallRunningRight = (bool)stream.ReceiveNext();
            wallRunningLeft = (bool)stream.ReceiveNext();
            forwardSpeed = (float)stream.ReceiveNext();
            sideSpeed = (float)stream.ReceiveNext();
            jumpSpeed = (float)stream.ReceiveNext();
        }
	}

	private Vector3 SafeLerp(Vector3 dest, Vector3 source, float speed)
	{
		Vector3 v = Vector3.Lerp(dest, source, speed);
		if (!float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z))
		{
			return v;
		}
		Debug.LogError("NetworkCharacter.Vector3SafeLerp: Input position was NaN!");
		return dest;
	}
	
	private Quaternion SafeLerp(Quaternion dest, Quaternion source, float speed)
	{
		Quaternion q = Quaternion.Lerp(dest, source, speed);
		if (!float.IsNaN(q.x) && !float.IsNaN(q.y) && !float.IsNaN(q.z))
		{
			return q;
		}
		Debug.LogError("NetworkCharacter.QuaternionSafeLerp: Input position was NaN!");
		return dest;
	}
}