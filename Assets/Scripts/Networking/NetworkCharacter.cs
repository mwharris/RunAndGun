using UnityEngine;
using System.Collections;

[RequireComponent(typeof (InputState))]
public class NetworkCharacter : Photon.MonoBehaviour 
{
	public Camera playerCamera;
    public Animator bodyAnimator;

    private InputState inputState;

    //Player postion and rotation need to be passed so preserve look rotations
	private Vector3 realPos = Vector3.zero;
	private Quaternion realRot = Quaternion.identity; //Maybe only the y-rotation is needed here?...

    //Animation variables
    private bool isSprinting;
    private bool isAiming;
    private bool isJumping;
    private bool isCrouching;
    private bool isShooting;
    private bool isReloading;
    private float jumpSpeed;
    private float lookAngle;

    //MAYBE COULD BE SEPARATED INTO BOOLEANS: Forward, Backward, Left, Right?...
    private float forwardSpeed;
    private float sideSpeed;

    //Character Controller properties need to be passed due to Crouch animations
    private CharacterController cc;
    private float ccHeight = 0f;
    private float ccRadius = 0f;
    private Vector3 ccCenter = Vector3.zero;

	void Awake()
	{
		cc = GetComponent<CharacterController>();
        inputState = GetComponent<InputState>();
    }

	/**
	 * Handle updating non-local player's variables sent over the network
	 */
	void Update()
    {
        //Only update a non-local player. Local players are updated by First Person Controller
        if (!photonView.isMine)
        {
            float lerpSpeed = Time.deltaTime * 8f;
            //Smooth our movement from the current position to the received position
            transform.position = Vector3.Lerp(transform.position, realPos, lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRot, lerpSpeed);
            //Animation variables
            bodyAnimator.SetBool("Sprinting", isSprinting);
            bodyAnimator.SetBool("Aiming", isAiming);
            bodyAnimator.SetBool("Jumping", isJumping);
            bodyAnimator.SetBool("Crouching", isCrouching);
            bodyAnimator.SetBool("Shooting", isShooting);
            bodyAnimator.SetBool("Reloading", isReloading);
            bodyAnimator.SetFloat("ForwardSpeed", forwardSpeed);
            bodyAnimator.SetFloat("SideSpeed", sideSpeed);
            bodyAnimator.SetFloat("JumpSpeed", jumpSpeed);
            float la = Mathf.Lerp(bodyAnimator.GetFloat("LookAngle"), lookAngle, lerpSpeed);
            bodyAnimator.SetFloat("LookAngle", la);
            //Set Capsule Collider and Character Controller variables for crouching
            cc.height = Mathf.Lerp(cc.height, ccHeight, lerpSpeed);
            cc.radius = Mathf.Lerp(cc.radius, ccRadius, lerpSpeed);
            cc.center = Vector3.Lerp(cc.center, ccCenter, lerpSpeed);
        }
	}

	/**
	 * Handle the actual sending / receiving of variables over the network
	 */
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			//This is our local player, send our position to the network
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
            //Send animator variable information
            stream.SendNext(inputState.playerIsSprinting);
            stream.SendNext(inputState.playerIsAiming);
            stream.SendNext(!inputState.playerIsGrounded);
            stream.SendNext(inputState.playerIsCrouching);
            stream.SendNext(inputState.playerIsShooting);
            stream.SendNext(inputState.playerIsReloading);
            stream.SendNext(Vector3.Dot(inputState.playerVelocity, transform.forward));
            stream.SendNext(Vector3.Dot(inputState.playerVelocity, transform.right));
            stream.SendNext(inputState.playerVelocity.y);
            stream.SendNext(inputState.playerLookAngle);
            //Send Character Controller information
            stream.SendNext(cc.height);
            stream.SendNext(cc.radius);
            stream.SendNext(cc.center);
		}
		else
		{
			//This is a networked player, receive their position an update the player accordingly
			realPos = (Vector3) stream.ReceiveNext();
			realRot = (Quaternion) stream.ReceiveNext();
            //Receive animator variable information
            isSprinting =  (bool) stream.ReceiveNext();
            isAiming =     (bool) stream.ReceiveNext();
            isJumping =    (bool) stream.ReceiveNext();
            isCrouching =  (bool) stream.ReceiveNext();
            isShooting =   (bool) stream.ReceiveNext();
            isReloading =   (bool) stream.ReceiveNext();
            forwardSpeed = (float) stream.ReceiveNext();
            sideSpeed =    (float) stream.ReceiveNext();
            jumpSpeed =    (float) stream.ReceiveNext();
            lookAngle =    (float) stream.ReceiveNext();
            //Receive character controller information
            ccHeight = (float) stream.ReceiveNext();
            ccRadius = (float) stream.ReceiveNext();
            ccCenter = (Vector3) stream.ReceiveNext();
		}
	}
}