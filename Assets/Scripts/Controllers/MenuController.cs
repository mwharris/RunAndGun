using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuController : AbstractBehavior 
{
	[SerializeField] private GameObject pauseMenu;
	
    private bool _paused;
	private bool _options;
	private GameObject _eventSystem;
	private NetworkManager _networkManager;
	private GameManager _gameManager;
    private bool _pickupAvailable = false;

	private Transform _pausePanel;
	private OptionsController _optionsController;

	void Start()
	{
		_eventSystem = GameObject.Find("EventSystem");
		_gameManager = GetComponent<GameManager>();
        _networkManager = GetComponent<NetworkManager>();
        _optionsController = GetComponent<OptionsController>();
		_pausePanel = pauseMenu.transform.GetChild(0);
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
        else if (_pickupAvailable)
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
        //Tell the Options menu to display
        _optionsController.ShowOptionsMenu();
	}

	void HideOptionsMenu()
	{
		if (_gameManager.GetGameState() == GameManager.GameState.playing ||
		    _gameManager.GetGameState() == GameManager.GameState.paused)
		{
			//Hide the pause menu
			ShowPauseMenu();
			//Unlock the mouse cursor
			UnlockMouseCursor();
		}
        //Tell the Options menu to hide
        _optionsController.CloseOptionsMenu();
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
