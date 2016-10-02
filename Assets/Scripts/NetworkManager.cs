﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	public Camera lobbyCamera;
	public bool offlineMode;
	public float respawnTimer = 0f;
	public bool respawnAvailable;
	public GameObject menu;
	public GameObject ammoUI;

	private GameObject respawnOverlay;
	private string username;
	private GameObject[] spawnPoints;
	private const string glyphs = "abcdefghijklmnopqrstuvwxys1234567890";

	// Use this for initialization
	void Start () 
	{
		//Get the list of spawn points for the players
		spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
		//Get a reference to the respawn overlay
		respawnOverlay = GameObject.FindGameObjectWithTag("RespawnOverlay");
	}

	void Update()
	{
		//If we're sitting at the respawn screen
		if(respawnAvailable)
		{
			//Prompt the user to hit space
			if(Input.GetKey(KeyCode.Space))
			{
				//Reset the flag
				respawnAvailable = false;
				//Respawn the player
				SpawnPlayer();
			}
		}
	}

	public void Connect () 
	{
		//Set the player's username
		PhotonNetwork.playerName = username;

		//Unique string to identify our connection
		PhotonNetwork.ConnectUsingSettings("RunNGunFPS");

		//Hide the main menu
		menu.SetActive(false);
	}

	public void ConnectOffline(){
		//Set the player's username
		PhotonNetwork.playerName = username;

		//Mark this session as offline and create
		PhotonNetwork.offlineMode = true;
		PhotonNetwork.CreateRoom("OfflineRoom");

		//Hide the main menu
		menu.SetActive(false);
	}

	void OnGUI()
	{
		//DEBUG - Display the status of our connection on the screen
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
	}

	//Callback for Lobby join success
	void OnJoinedLobby()
	{
		//Immediately join a random room once we join the lobby
		PhotonNetwork.JoinRandomRoom();
	}

	//Callback for failure of JoinRandomRoom()
	void OnPhotonRandomJoinFailed()
	{
		//Create a new room since one does not exist
		PhotonNetwork.CreateRoom(null);
	}
		
	//Callback for successfully joining a room
	void OnJoinedRoom()
	{
		//Spawn the local player
		SpawnPlayer();
	}

	void SpawnPlayer()
	{
		//Make sure the respawn overlay is hidden
		HideRespawnOverlay();

		//Randomly choose a spawn point for the player
		if(spawnPoints == null){
			Debug.Log("SPAWN POINTS ARE NULL IN SpawnPlayer()");
		}
		int spawnNum = Random.Range(0, spawnPoints.Length);
		Vector3 spawnPos = spawnPoints[spawnNum].transform.position;
		Quaternion spawnRot = spawnPoints[spawnNum].transform.rotation;

		//Instantiate the player across all clients
		GameObject myPlayer = PhotonNetwork.Instantiate("Player", spawnPos, spawnRot, 0);
		myPlayer.name = username;

		//Enable local player controls
		myPlayer.GetComponent<FirstPersonController>().enabled = true;
		myPlayer.GetComponent<ShootController>().enabled = true;
		myPlayer.GetComponentInChildren<Camera>().enabled = true;
		myPlayer.GetComponentInChildren<AudioListener>().enabled = true;
		myPlayer.GetComponentInChildren<Health>().lobbyCam = lobbyCamera;

		//Enable the displayed ammo counter
		ammoUI.SetActive(true);

		//Enable the camera reticle
		GameObject.FindGameObjectWithTag("Reticle").GetComponent<Image>().enabled = true;

		//Disable the lobby camera
		lobbyCamera.gameObject.SetActive(false);
	}

	void HideRespawnOverlay()
	{
		//Show respawn overlay components
		respawnOverlay.transform.GetChild(0).GetComponent<Image>().enabled = false;
		respawnOverlay.transform.GetChild(1).GetComponent<Text>().enabled = false;
	}

	public void ExitGame(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	public void SetUsername(string name)
	{
		if(name == null || name == "")
		{
			//Generate a random string of 10 characters
			for(int i = 0; i < 10; i++)
			{
				name += glyphs[Random.Range(0, glyphs.Length)]; 
			}
		}
		//Set the username
		username = name;
	}
}
