using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MenuController : AbstractBehavior 
{
	public float MouseSensitivity { get; set; } = 1f;
	public bool InvertY { get; set; } = false;
	public bool AimAssist { get; set; } = true;

    private bool _paused;
	private bool _options;
    private bool pickupAvailable = false;
	private GameObject _eventSystem;
	private NetworkManager _networkManager;
	private GameManager _gameManager;

	private GameObject _pauseMenu;
	private GameObject _optionsMenu;
	private Transform _pausePanel;
	private Transform _optionsPanel;

	void Start()
	{
		//Find the pause and options menu GameObjects and Panels
		_pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
		_pausePanel = _pauseMenu.transform.GetChild(0);
		_optionsMenu = GameObject.FindGameObjectWithTag("OptionsMenu");
		_optionsPanel = _optionsMenu.transform.GetChild(0);
        //Initialize a reference to the NetworkManager
        _networkManager = FindObjectOfType<NetworkManager>();
		//Initialize a reference to the GameManager
		_gameManager = FindObjectOfType<GameManager>();
		//Find the event system
		_eventSystem = GameObject.Find("EventSystem");
    }

	void Update () 
	{
		//Gather button presses for processing below
		bool isPauseDown = inputState.GetButtonPressed(inputs[0]) && inputState.GetButtonHoldTime(inputs[0]) == 0;
		bool isCancelDown = inputState.GetButtonPressed(inputs[1]) && inputState.GetButtonHoldTime(inputs[1]) == 0;
		//Game state variables
		bool isGamePlaying = _gameManager.GetGameState() == GameManager.GameState.playing;
		bool isGamePaused = _gameManager.GetGameState() == GameManager.GameState.paused;
		//Toggle the pause menu when pause button is pressed while playing
		if((isGamePlaying || isGamePaused) && (isPauseDown || isCancelDown) && !_options)
		{
			TogglePauseMenu(false);
		}
		//Back button pressed while game is paused
		else if (isGamePaused && isCancelDown) 
		{
			//Back out of Options menu
			if (_options) 
			{
				ToggleOptionsMenu();				
			}
			//Back out of Pause menu
			else 
			{
				TogglePauseMenu(false);
			}
		}
	    // TODO: Why is this here?...
        else if (pickupAvailable)
        {
        }
	}

	public void TogglePauseMenu(bool mainMenu)
	{
		//Flip the flag
		_paused = !_paused;
		//Deselect any buttons that may have carried over
		_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
		//Show/Hide the Paused menu accordingly and update Game State
		if(_paused)
		{
			ShowPauseMenu();
			_gameManager.ChangeGameState(GameManager.GameState.paused);
		}
		else 
		{
			HidePauseMenu();
			_gameManager.ChangeGameState(GameManager.GameState.playing);
		}
		//If we are navigating to the Main Menu
		if(mainMenu)
		{
			//Call the NetworkManager to disconnect from the server
			_networkManager.Disconnect();
            //Unlock the mouse cursor
            UnlockMouseCursor();
        }
	}

	void ShowPauseMenu()
	{
        //Unlock the mouse cursor
        UnlockMouseCursor();
        //Simply set the pause panel to active
        _pausePanel.gameObject.SetActive(true);
		//Default select the main menu button
		Transform menuButton = _pausePanel.GetChild(1);
		menuButton.GetComponent<Selectable>().Select();
	}

	void HidePauseMenu()
	{
        //Lock the mouse cursor
        LockMouseCursor();
        //Simply set the pause panel to inactive
        _pausePanel.gameObject.SetActive(false);
	}

	public void ToggleOptionsMenu()
	{
		//Flip the flag
		_options = !_options;
		//Deselect any buttons that may have carried over
		_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
		//Show/Hide the Paused menu accordingly and update Game State
		if(_options)
		{
			ShowOptionsMenu();
		}
		else 
		{
			HideOptionsMenu();
		}
	}

	void ShowOptionsMenu()
	{
		//Hide the pause menu
		HidePauseMenu();
        //Unlock the mouse cursor
        UnlockMouseCursor();
        //Simply set the Options menu to active
        _optionsPanel.gameObject.SetActive(true);
		//Default select the Close button
		Transform closeButton = _optionsPanel.transform.GetChild(4);
		closeButton.GetComponent<Selectable>().Select();
	}

	void HideOptionsMenu()
	{
		//Hide the pause menu
		ShowPauseMenu();
        //Unlock the mouse cursor
        UnlockMouseCursor();
        //Simply set the Options menu to active
        _optionsPanel.gameObject.SetActive(false);
	}

	//Update the mouse sensitivity in the First Person Controller
	public void ChangeMouseSensitivity(float newValue)
	{
		MouseSensitivity = newValue;
	}

	//Update the invert y setting in the First Person Controller
	public void ChangeInvertY(bool newValue)
	{
		InvertY = newValue;
    }

    //Update whether aim assist to enabled or disabled in the First Person Controller
    public void ChangeAimAssist(bool newValue)
    {
        AimAssist = newValue;
    }

    private void LockMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockMouseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
