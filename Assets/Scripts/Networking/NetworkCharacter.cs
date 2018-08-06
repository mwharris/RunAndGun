using UnityEngine;
using System.Collections;

[RequireComponent(typeof (InputState))]
public class NetworkCharacter : Photon.MonoBehaviour 
{
	public Camera playerCamera;
    public Animator bodyAnimator;

    private InputState inputState;

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
            //Send Character Controller information
            stream.SendNext(cc.height);
            stream.SendNext(cc.radius);
            stream.SendNext(cc.center);
		}
		else
		{
            //Receive character controller information
            ccHeight = (float) stream.ReceiveNext();
            ccRadius = (float) stream.ReceiveNext();
            ccCenter = (Vector3) stream.ReceiveNext();
		}
	}
}