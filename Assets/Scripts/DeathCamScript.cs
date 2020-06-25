﻿using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DeathCamScript : MonoBehaviour {

	public float lookTimer = 0.0f;
	public float lifeTimer = 0.0f;
    public int targetId;
	public GameObject target;
	public Camera lobbyCamera;

	private GameObject _deathOverlay;
	private GameObject _respawnOverlay;
	private NetworkManager _networkManager;

	void Start() 
	{
		//Initialize the look timer
		lookTimer = 0.5f;
		//Initialize the death timer
		lifeTimer = 3.0f;
		//Get a reference to the death overlay
		_deathOverlay = GameObject.FindGameObjectWithTag("DeathOverlay");
		//Get a reference to the respawn overlay
		_respawnOverlay = GameObject.FindGameObjectWithTag("RespawnOverlay");
		//Find the target we are trying to look at
		target = FindTarget(targetId);
		//Get a reference to our NetworkManager in order to manipulate variables
		// TODO: Pass this in some other way
		_networkManager = GameObject.FindObjectOfType<NetworkManager>();
	}

	void Update () 
	{
		//Decrement the timers
		lookTimer -= Time.deltaTime;
		lifeTimer -= Time.deltaTime;
		//Check if we should start looking at the killer
		if(lookTimer <= 0 && target != null)
		{
			//Get the vector of the direction to the killer
			Vector3 dirVector = target.transform.position - this.transform.position;
			//Get the quaternion
			Quaternion quat = Quaternion.LookRotation(dirVector);
			//Rotate towards the killer
			transform.rotation = Quaternion.Lerp(transform.rotation, quat, 0.2f);
		}
		//Check if we should turn this camera off
		if(lifeTimer <= 0)
		{
			//Hide the gray screen and show the player's camera
			HideDeathOverlay();
			//Show the respawn overlay
			ShowRespawnOverlay();
			//Make the Lobby Camera active
			lobbyCamera.gameObject.SetActive(true);
			//Set the respawn screen active
			_networkManager.respawnAvailable = true;
			//Destroy this camera
			Destroy(this.gameObject);
		}
	}

	void ShowRespawnOverlay()
	{
		//Show respawn overlay components
		_respawnOverlay.transform.GetChild(0).GetComponent<Image>().enabled = true;
		_respawnOverlay.transform.GetChild(1).GetComponent<Text>().enabled = true;
	}

	void HideDeathOverlay()
	{
		//Hide death overlay components
		_deathOverlay.transform.GetChild(0).GetComponent<Text>().enabled = false;
		_deathOverlay.transform.GetChild(1).GetComponent<Text>().enabled = false;
	}

	GameObject FindTarget(int targetId)
	{
		//Get the full list of players
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		//Loop through each
		foreach(GameObject player in players)
		{
			PhotonView pView = player.GetComponent<PhotonView>();
            //Return this player object if it's the player we're looking for
            if (pView != null && pView.Owner.ActorNumber == targetId)
			{
				return player;
			}
		}
		//Player not found
		return null;
	}
}