using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour 
{
	public Camera playerCamera;
	public GameObject body;

	private Vector3 realPos = Vector3.zero;
	private Quaternion realRot = Quaternion.identity;
	private Vector3 bodyScale = Vector3.zero;
	private Vector3 bodyPos = Vector3.zero;
	private Quaternion weaponRot = Quaternion.identity;
	private Vector3 weaponPos = Vector3.zero;
	private Vector3 bodyCenter = Vector3.zero;
	private float ccHeight = 0f;
	private float ccCenter = 0f;
	private CharacterController cc;

	void Awake()
	{
		cc = transform.GetComponent<CharacterController>();
	}

	/**
	 * Handle updating non-local player's variables sent over the network
	 */
	void Update()
	{
		//Only update a non-local player. Local players are updated by First Person Controller
		if(!photonView.isMine)
		{
			//Smooth our movement from the current position to the received position
			transform.position = Vector3.Lerp(transform.position, realPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRot, Time.deltaTime * 5);
			body.transform.localScale = Vector3.Lerp(body.transform.localScale, bodyScale, Time.deltaTime * 5);
			body.transform.position = Vector3.Lerp(body.transform.position, bodyPos, Time.deltaTime * 5);
			playerCamera.transform.rotation = Quaternion.Lerp(playerCamera.transform.rotation, weaponRot, Time.deltaTime * 5);
			playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, weaponPos, Time.deltaTime * 5);
			//Set Capsule Collider and Character Controller variables for crouching
			cc.height = Mathf.Lerp(cc.height, ccHeight, Time.deltaTime * 5);
			cc.center = Vector3.Lerp(cc.center, new Vector3(cc.center.x, ccCenter, cc.center.z), Time.deltaTime * 5);
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
			stream.SendNext(body.transform.localScale);
			stream.SendNext(body.transform.position);
			stream.SendNext(playerCamera.transform.rotation);
			stream.SendNext(playerCamera.transform.position);
			stream.SendNext(cc.height);
			stream.SendNext(cc.center.y);
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
			ccHeight = (float) stream.ReceiveNext();
			ccCenter = (float) stream.ReceiveNext();
		}
	}
}
