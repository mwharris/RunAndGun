﻿using UnityEngine;
using Photon.Pun;

public class NetworkCharacter : MonoBehaviourPun, IPunObservable
{
    private InputState _inputState;
    private BodyController _bodyControl;

    //Player position and rotation need to be passed so preserve look rotations
    private Vector3 _realPos = Vector3.zero;
    // TODO: Maybe only the y-rotation is needed here?...
    private Quaternion _realRot = Quaternion.identity;

    //Camera position and rotations 
    // TODO: POSITION MIGHT NOT BE NEEDED
    private Vector3 _camRealPos = Vector3.zero;
    private Quaternion _camRealRot = Quaternion.identity;

    //Animation variables
    private bool _isSprinting;
    private bool _isAiming;
    private bool _isAirborne;
    private bool _isCrouching;
    private bool _wallRunningLeft;
    private bool _wallRunningRight;
    private float _jumpSpeed;

    // TODO: MAYBE COULD BE SEPARATED INTO BOOLEANS: Forward, Backward, Left, Right?...
    private float _forwardSpeed;
    private float _sideSpeed;
    private bool _crouchReset;
    private bool _jumpReset;

    //Character Controller properties need to be passed due to Crouch animations
    private CharacterController _characterController;
    private CapsuleCollider _bodyCollider;
    private D_CrouchController _dCrouchController;
    private D_PlayerJump _jumpController;
    private FixWallRunningAnimation _wrAnimFix;

    void Awake()
	{
        _characterController = GetComponent<CharacterController>();
        _bodyCollider = GetComponent<CapsuleCollider>();
        _bodyControl = GetComponent<BodyController>();
        _dCrouchController = GetComponent<D_CrouchController>();
        _jumpController = GetComponent<D_PlayerJump>();
        _inputState = GetComponent<InputState>();
        _wrAnimFix = GetComponent<FixWallRunningAnimation>();
    }

	/**
	 * Handle updating non-local player's variables sent over the network
	 */
	void Update()
    {
        //Get the body components we need to update based on if we're Third or First person
        PlayerBodyData playerBodyData = _bodyControl.PlayerBodyData;
        Animator bodyAnimator = playerBodyData.GetBodyAnimator();

        //Only update a non-local player. Local players are updated by First Person Controller
        if (!photonView.IsMine)
        {
            float lerpSpeed = Time.deltaTime * 8f;

            //Smooth our movement from the current position to the received position
            //TODO: PREDICTION
            transform.position = SafeLerp(transform.position, _realPos, lerpSpeed);
            transform.rotation = SafeLerp(transform.rotation, _realRot, lerpSpeed);

            //Smooth our camera movement from the current position to the received position
            playerBodyData.playerCamera.localPosition = SafeLerp(playerBodyData.playerCamera.localPosition, _camRealPos, lerpSpeed);
            playerBodyData.playerCamera.localRotation = SafeLerp(playerBodyData.playerCamera.localRotation, _camRealRot, lerpSpeed);

            //Animation variables
            bodyAnimator.SetBool("Sprinting", _isSprinting);
            bodyAnimator.SetBool("Aiming", _isAiming);
            bodyAnimator.SetBool("Jumping", _isAirborne);
            bodyAnimator.SetBool("Crouching", _isCrouching);
            bodyAnimator.SetBool("WallRunningRight", _wallRunningRight);
            bodyAnimator.SetBool("WallRunningLeft", _wallRunningLeft);
            bodyAnimator.SetFloat("ForwardSpeed", _forwardSpeed);
            bodyAnimator.SetFloat("SideSpeed", _sideSpeed);
            bodyAnimator.SetFloat("JumpSpeed", _jumpSpeed);

            //Set Capsule Collider and Character Controller variables for crouching
            _dCrouchController.HandleMultiplayerCrouch(gameObject, playerBodyData.playerCamera.gameObject, _isCrouching, !_isAirborne, _crouchReset);

            //Set Capsule Collider and Character Controller variables for jumping
            _jumpController.HandleHitboxes(gameObject, _isAirborne, _isCrouching, _jumpReset);

            //Fix for wall-running animations being rotated incorrectly
            _wrAnimFix.RunFix(_wallRunningLeft, _wallRunningRight, Time.deltaTime);
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
        var playerBodyData = _bodyControl.PlayerBodyData;

        if (stream.IsWriting)
		{
			//This is our local player, send our position to the network
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
            //Camera position and rotation
            stream.SendNext(playerBodyData.playerCamera.localPosition);
            stream.SendNext(playerBodyData.playerCamera.localRotation);
            //Send animator variable information
            stream.SendNext(_inputState.playerIsSprinting);
            stream.SendNext(_inputState.playerIsAiming);
            stream.SendNext(!_inputState.playerIsGrounded);
            stream.SendNext(_inputState.playerIsCrouching);
            stream.SendNext(_inputState.playerIsWallRunningRight);
            stream.SendNext(_inputState.playerIsWallRunningLeft);
            stream.SendNext(Vector3.Dot(_inputState.playerVelocity, transform.forward));
            stream.SendNext(Vector3.Dot(_inputState.playerVelocity, transform.right));
            stream.SendNext(_inputState.playerVelocity.y);
		}
		else
		{
            //This is a networked player, receive their position an update the player accordingly
            _realPos = (Vector3)stream.ReceiveNext();
            _realRot = (Quaternion)stream.ReceiveNext();
            //Camera position and rotation
            _camRealPos = (Vector3)stream.ReceiveNext();
            _camRealRot = (Quaternion)stream.ReceiveNext();
            //Receive animator variable information
            _isSprinting = (bool)stream.ReceiveNext();
            _isAiming = (bool)stream.ReceiveNext();
            //Jumping requires some extra logic for the reset flag
            bool nowAirborne = (bool)stream.ReceiveNext();
            if (_isAirborne && !nowAirborne) {
                _jumpReset = true;
            }
            _isAirborne = nowAirborne;
            //Crouching requires some extra logic for the reset flag
            bool nowCrouching = (bool)stream.ReceiveNext();
            if (_isCrouching && !nowCrouching) { _crouchReset = true; }
            _isCrouching = nowCrouching;
            _wallRunningRight = (bool)stream.ReceiveNext();
            _wallRunningLeft = (bool)stream.ReceiveNext();
            _forwardSpeed = (float)stream.ReceiveNext();
            _sideSpeed = (float)stream.ReceiveNext();
            _jumpSpeed = (float)stream.ReceiveNext();
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