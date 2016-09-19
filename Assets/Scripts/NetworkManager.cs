using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	public Camera lobbyCamera;
	public bool offlineMode;
	public float respawnTimer = 0f;
	public GameObject menu;
	public GameObject ammoUI;

	private string username;
	private GameObject[] spawnPoints;
	private const string glyphs = "abcdefghijklmnopqrstuvwxys1234567890";

	// Use this for initialization
	void Start () 
	{
		//Get the list of spawn points for the players
		spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
	}

	void Update()
	{
		if(respawnTimer > 0)
		{
			//Reduce the respawn timer every frame
			respawnTimer -= Time.deltaTime;

			//Respawn the player if our respawn timer has run out
			if(respawnTimer <= 0)
			{
				SpawnPlayer();
			}
		}
	}

	public void Connect () 
	{
		//Set the player's username
		PhotonNetwork.playerName = SetUsername(name);

		//Unique string to identify our connection
		PhotonNetwork.ConnectUsingSettings("RunNGunFPS");

		//Hide the main menu
		menu.SetActive(false);
	}

	public void ConnectOffline(){
		//Set the player's username
		PhotonNetwork.playerName = SetUsername(name);

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

		//Enable the displayed ammo counter
		ammoUI.SetActive(true);

		//Enable the camera reticle
		GameObject.FindGameObjectWithTag("Reticle").GetComponent<Image>().enabled = true;

		//Disable the lobby camera
		lobbyCamera.gameObject.SetActive(false);
	}

	public void ExitGame(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	public string SetUsername(string name)
	{
		string returnName = "";

		if(name == null || name == "")
		{
			//Generate a random string of 10 characters
			for(int i = 0; i < 10; i++)
			{
				name += glyphs[Random.Range(0, glyphs.Length)]; 
			}
		}

		//Set the username
		return name;
	}
}
