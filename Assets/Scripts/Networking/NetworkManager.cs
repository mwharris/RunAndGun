using UnityEngine;
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
	private GameManager gm;
	private float baseFOV;
	private InputManager inputManager;

	// Use this for initialization
	void Start () 
	{
		//Get the list of spawn points for the players
		spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
		//Get a reference to the respawn overlay
		respawnOverlay = GameObject.FindGameObjectWithTag("RespawnOverlay");
		//Initialize a reference to the GameManager
		gm = GameObject.FindObjectOfType<GameManager>();
		//And the global input manager
		inputManager = GameObject.FindGameObjectWithTag("Input Manager").GetComponent<InputManager>();
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
		//Mark this session as online
		PhotonNetwork.offlineMode = false;
		//Unique string to identify our connection
		PhotonNetwork.ConnectUsingSettings("RunNGunFPS");
		//Hide the main menu
		menu.SetActive(false);
		//Mark our Game State as playing
		gm.ChangeGameState(GameManager.GameState.playing);
	}

	public void ConnectOffline(){
		//Set the player's username
		PhotonNetwork.playerName = username;
		//Mark this session as offline
		PhotonNetwork.offlineMode = true;
		//Unique string to identify our connection
		PhotonNetwork.CreateRoom("OfflineRoom");
		//Hide the main menu
		menu.SetActive(false);
		//Mark our Game State as playing
		gm.ChangeGameState(GameManager.GameState.playing);
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
		int spawnNum = Random.Range(0, spawnPoints.Length);
		Vector3 spawnPos = spawnPoints[spawnNum].transform.position;
		Quaternion spawnRot = spawnPoints[spawnNum].transform.rotation;
		//Instantiate the player across all clients
		GameObject myPlayer = PhotonNetwork.Instantiate("RecoilPlayer", spawnPos, spawnRot, 0);
		myPlayer.name = username;
		//Set the local player up to handle input
		inputManager.inputState = myPlayer.GetComponent<InputState>();
		//Enable local player controls
		myPlayer.GetComponent<FirstPersonController>().enabled = true;
		myPlayer.GetComponent<ShootController>().enabled = true;
		myPlayer.GetComponent<AccuracyController>().enabled = true;
		myPlayer.GetComponent<WallRunController>().enabled = true;
		myPlayer.GetComponent<SprintController>().enabled = true;
		myPlayer.GetComponent<RecoilController>().enabled = true;
		myPlayer.GetComponent<RecoilController>().recoil = 0;
		myPlayer.GetComponent<RecoilController>().currentRecoil = 0;
		myPlayer.GetComponentInChildren<Camera>().enabled = true;
		myPlayer.GetComponentInChildren<AudioListener>().enabled = true;
        myPlayer.GetComponentInChildren<ControlAnimations>().enabled = true;
        myPlayer.GetComponentInChildren<IKHandler>().enabled = true;
        myPlayer.GetComponentInChildren<Health>().lobbyCam = lobbyCamera;
        myPlayer.GetComponentInChildren<Animator>().SetLayerWeight(1, 0);
        //Shrink out head so it's no longer visible
        Transform[] childTransforms = myPlayer.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i].name == "head1_neck")
            {
                childTransforms[i].localScale = new Vector3(0, 0, 0);
            }
        }
		//Enable the displayed ammo counter
		ammoUI.SetActive(true);
		//Enable the camera reticle
		EnableReticle();
        //Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //Disable the lobby camera
        lobbyCamera.gameObject.SetActive(false);
	}

	public void Disconnect()
	{
		//Disconnect from the game if we are in Multiplayer mode
		if(!PhotonNetwork.offlineMode && PhotonNetwork.connected)
		{
			PhotonNetwork.Disconnect();
		}
		else if(PhotonNetwork.offlineMode)
		{
			PhotonNetwork.LeaveRoom();
			Destroy(GameObject.FindGameObjectWithTag("Player"));
		}
		//Enable the lobby camera
		lobbyCamera.gameObject.SetActive(true);
		//Show the main menu
		menu.SetActive(true);
		//Default select the Solo button
		menu.transform.GetChild(5).GetComponent<Selectable>().Select();
		//Unlock the cursor
		Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
		//Mark our Game State as none
		gm.ChangeGameState(GameManager.GameState.none);
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

	private void EnableReticle()
	{
		//Get the reticle parent game object
		GameObject reticle = GameObject.FindGameObjectWithTag("Reticle");
		//Loop through each image
		Image[] images = reticle.GetComponentsInChildren<Image>();
		foreach(Image image in images)
		{
			//Show the reticle image
			image.enabled = true;
		}
	}
}
