using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    public Camera lobbyCamera;
    public bool respawnAvailable;
    public GameObject menu;
    public GameObject ammoUI;

    private GameObject _respawnOverlay;
    private string _username;
    private GameObject[] _spawnPoints;
    private const string Glyphs = "abcdefghijklmnopqrstuvwxys1234567890";
    private GameManager _gameManager;
    private float _baseFov;
    private InputManager _inputManager;
    private BodyController _bodyController;

    // Use this for initialization
    void Start()
    {
        //Get the list of spawn points for the players
        _spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        //Get a reference to the respawn overlay
        _respawnOverlay = GameObject.FindGameObjectWithTag("RespawnOverlay");
        //Initialize a reference to the GameManager
        _gameManager = GameObject.FindObjectOfType<GameManager>();
        //And the global input manager
        _inputManager = GameObject.FindGameObjectWithTag("Input Manager").GetComponent<InputManager>();
    }

    void Update()
    {
        //If we're sitting at the respawn screen
        if (respawnAvailable)
        {
            //Prompt the user to hit space
            if (PlayerInput.Instance.SpaceDown)
            {
                //Reset the flag
                respawnAvailable = false;
                //Respawn the player
                SpawnPlayer();
            }
        }
    }

    public void Connect()
    {
	    //Set the player's username
        PhotonNetwork.NickName = _username;
        //Mark this session as online
        PhotonNetwork.OfflineMode = false;
        //Unique string to identify our connection
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = "RunNGunFPS";
        //Hide the main menu
        menu.SetActive(false);
        //Mark our Game State as playing
        _gameManager.ChangeGameState(GameManager.GameState.playing);
    }

    public void ConnectOffline() {
        //Set the player's username
        PhotonNetwork.NickName = _username;
        //Mark this session as offline
        PhotonNetwork.OfflineMode = true;
        //Unique string to identify our connection
        PhotonNetwork.CreateRoom("OfflineRoom");
        //Hide the main menu
        menu.SetActive(false);
        //Mark our Game State as playing
        _gameManager.ChangeGameState(GameManager.GameState.playing);
    }

    public override void OnConnectedToMaster()
    {
	    PhotonNetwork.JoinRandomRoom();
	    base.OnConnectedToMaster();
    }

    //Callback for successfully joining a room
    public override void OnJoinedRoom()
    {
	    //Spawn the local player
	    SpawnPlayer();
	    base.OnJoinedRoom();
    }

    //Callback for failure of JoinRandomRoom()
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
	    PhotonNetwork.CreateRoom("DefaultRoom");
	    base.OnJoinRandomFailed(returnCode, message);
    }

    void SpawnPlayer()
    {
        //Make sure the respawn overlay is hidden
        HideRespawnOverlay();
        //Randomly choose a spawn point for the player
        int spawnNum = Random.Range(0, _spawnPoints.Length);
        Vector3 spawnPos = _spawnPoints[spawnNum].transform.position;
        Quaternion spawnRot = _spawnPoints[spawnNum].transform.rotation;
        //Instantiate the player across all clients
        GameObject myPlayer = PhotonNetwork.Instantiate("RecoilPlayer", spawnPos, spawnRot, 0);
        myPlayer.name = _username;
        //Set the local player up to handle input
        _inputManager.inputState = myPlayer.GetComponent<InputState>();
        //Enable local player controls
        EnableLocalPlayer(myPlayer);
        //Cache our player for easy finding later
        _gameManager.MyPlayer = myPlayer;
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

    private void EnableLocalPlayer(GameObject myPlayer)
    {
        HandlePlayerBody(myPlayer);
        //Enable various scripts that only run on the local player
        myPlayer.GetComponent<PlayerMovementStateMachine>().enabled = true;
        myPlayer.GetComponent<PlayerLookController>().enabled = true;
        myPlayer.GetComponent<CameraController>().enabled = true;
        myPlayer.GetComponent<ShootController>().enabled = true;
        myPlayer.GetComponent<AccuracyController>().enabled = true;
        myPlayer.GetComponent<RecoilController>().enabled = true;
        myPlayer.GetComponent<RecoilController>().recoil = 0;
        myPlayer.GetComponent<RecoilController>().currentRecoil = 0;
        myPlayer.GetComponent<FixWallRunningAnimation>().enabled = false;
        myPlayer.GetComponentInChildren<Camera>().enabled = true;
        myPlayer.GetComponentInChildren<AudioListener>().enabled = true;
        myPlayer.GetComponentInChildren<AnimationController>().enabled = true;
        myPlayer.GetComponentInChildren<WeaponPickup>().enabled = true;
        //myPlayer.GetComponentInChildren<IKHandler>().enabled = true;
        myPlayer.GetComponentInChildren<Health>().lobbyCam = lobbyCamera;
    }

    //Handle Enable/Disable of the FPS and TPS bodies
    private void HandlePlayerBody(GameObject myPlayer)
    {
        //Make sure the FPS arms are activated
        Transform fps = myPlayer.transform.GetChild(0);
        fps.gameObject.SetActive(true);
        //Also activate the the TPS body but...
        Transform tps = myPlayer.transform.GetChild(1);
        tps.gameObject.SetActive(true);
        //Disable the TPS camera gameobject
        Transform other = tps.GetChild(1);
        other.GetChild(0).GetComponent<Camera>().enabled = false;
        //Disable all elements underneath the body gameobject
        Transform animatedBody = tps.GetChild(0);
        for (int i = 0; i < animatedBody.childCount; i++)
        {
            Transform child = animatedBody.GetChild(i);
            child.gameObject.SetActive(false);
        }
    }

    public void Disconnect()
	{
		//Disconnect from the game if we are in Multiplayer mode
		if(!PhotonNetwork.OfflineMode && PhotonNetwork.IsConnected)
		{
			PhotonNetwork.Disconnect();
		}
		else if(PhotonNetwork.OfflineMode)
		{
			PhotonNetwork.LeaveRoom();
            Destroy(_gameManager.MyPlayer);
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
		_gameManager.ChangeGameState(GameManager.GameState.none);
	}

	void HideRespawnOverlay()
	{
		//Show respawn overlay components
		_respawnOverlay.transform.GetChild(0).GetComponent<Image>().enabled = false;
		_respawnOverlay.transform.GetChild(1).GetComponent<Text>().enabled = false;
	}

	public void ExitGame(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	void OnGUI()
	{
		if (_gameManager.GetGameState() != GameManager.GameState.playing)
		{
			//DEBUG - Display the status of our connection on the screen
			GUILayout.Label(PhotonNetwork.NetworkClientState.ToString());
		}
	}

	public void SetUsername(string name)
	{
		if(name == null || name == "")
		{
			//Generate a random string of 10 characters
			for(int i = 0; i < 10; i++)
			{
				name += Glyphs[Random.Range(0, Glyphs.Length)]; 
			}
		}
		//Set the username
		_username = name;
	}

	private void EnableReticle()
	{
        BodyController bc = _gameManager.MyPlayer.GetComponent<BodyController>();
        WeaponData weaponData = bc.PlayerBodyData.weapon.GetComponent<WeaponData>();
		//Loop through each image
		Image[] images = weaponData.ReticleParent.GetComponentsInChildren<Image>();
		foreach(Image image in images)
		{
			//Show the reticle image
			image.enabled = true;
		}
	}
}