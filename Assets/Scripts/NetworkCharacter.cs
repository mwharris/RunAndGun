﻿using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {

	public Camera playerCamera;
	public GameObject body;

	private FirstPersonController fpcScript;
	private bool isCrouching = false;
	private Vector3 realPos = Vector3.zero;
	private Quaternion realRot = Quaternion.identity;
	private Vector3 bodyScale = Vector3.zero;
	private Vector3 bodyPos = Vector3.zero;
	private Quaternion weaponRot = Quaternion.identity;
	private Vector3 weaponPos = Vector3.zero;

	void Awake()
	{
		if(photonView.isMine){
			fpcScript = this.gameObject.GetComponent<FirstPersonController>();
		}
	}

	void Update()
	{
		//Only update a non-local player. Local players are updated by First Person Controller
		if(!photonView.isMine)
		{
			//Smooth our movement from the current position to the received position
			transform.position = Vector3.Lerp(transform.position, realPos, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRot, 0.1f);
			body.transform.localScale = Vector3.Lerp(body.transform.localScale, bodyScale, 0.1f);
			body.transform.position = Vector3.Lerp(body.transform.position, bodyPos, 0.1f);
			playerCamera.transform.rotation = Quaternion.Lerp(playerCamera.transform.rotation, weaponRot, 0.1f);
			playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, weaponPos, 0.1f);
			/*
			if(isCrouching){
				Vector3 lerpLPos = new Vector3(playerCamera.transform.localPosition.x, 1.8f, playerCamera.transform.localPosition.z);
				playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, lerpLPos, 0.1f);
					//new Vector3(playerCamera.transform.localPosition.x, 1.8f, playerCamera.transform.localPosition.z);
			} else {
				Vector3 lerpLPos = new Vector3(playerCamera.transform.localPosition.x, 2.5f, playerCamera.transform.localPosition.z);
				playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, lerpLPos, 0.1f);
					//new Vector3(playerCamera.transform.localPosition.x, 2.5f, playerCamera.transform.localPosition.z);
			}
			*/
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			//This is our local player, send our position to the network
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(body.transform.localScale);
			stream.SendNext(body.transform.position);
			stream.SendNext(playerCamera.transform.rotation);
			stream.SendNext(playerCamera.transform.position);
			//stream.SendNext(playerCamera.transform.localPosition);
			//stream.SendNext(fpcScript.isCrouching);
		}
		else
		{
			//This is a networked player, receive their position an update the player accordingly
			realPos = (Vector3) stream.ReceiveNext();
			realRot = (Quaternion) stream.ReceiveNext();
			bodyScale = (Vector3) stream.ReceiveNext();
			bodyPos = (Vector3) stream.ReceiveNext();
			weaponRot = (Quaternion) stream.ReceiveNext();
			weaponPos = (Vector3) stream.ReceiveNext();
			//isCrouching = (bool) stream.ReceiveNext();
		}
	}
}
