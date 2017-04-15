using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {

	public Camera playerCamera;
	public GameObject body;

	//private FirstPersonController fpcScript;
	private Vector3 realPos = Vector3.zero;
	private Quaternion realRot = Quaternion.identity;
	private Vector3 bodyScale = Vector3.zero;
	private Vector3 bodyPos = Vector3.zero;
	private Quaternion weaponRot = Quaternion.identity;
	private Vector3 weaponPos = Vector3.zero;
	private Vector3 bodyCenter = Vector3.zero;
	private float bodyHeight = 0f;
	private float ccHeight = 0f;
	private CapsuleCollider capCol;
	private CharacterController cc;

	void Awake()
	{
		/*
		if(photonView.isMine){
			fpcScript = this.gameObject.GetComponent<FirstPersonController>();
		}
		*/
		capCol = body.GetComponent<CapsuleCollider>();
		cc = transform.GetComponent<CharacterController>();
	}

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
			capCol.center = Vector3.Lerp(capCol.center, bodyCenter, Time.deltaTime * 5);
			capCol.height = bodyHeight;
			cc.height = ccHeight;
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
			stream.SendNext(capCol.center);
			stream.SendNext(capCol.height);
			stream.SendNext(cc.height);
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
			bodyCenter = (Vector3) stream.ReceiveNext();
			bodyHeight = (float) stream.ReceiveNext();
			ccHeight = (float) stream.ReceiveNext();
		}
	}
}
