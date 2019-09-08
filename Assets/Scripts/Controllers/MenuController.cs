﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MenuController : AbstractBehavior 
{
	public float MouseSensitivity { get; set; } = 1f;
	public bool InvertY { get; set; } = false;
	public bool AimAssist { get; set; } = true;

    private bool paused = false;
	private bool options = false;
    private bool pickupAvailable = false;
	private GameObject eventSystem;
	private NetworkManager nm;
	private GameManager gm;

	private GameObject pauseMenu;
	private GameObject optionsMenu;
	private Transform pausePanel;
	private Transform optionsPanel;

	void Start()
	{
		//Find the pause and options menu GameObjects and Panels
		pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
		pausePanel = pauseMenu.transform.GetChild(0);
		optionsMenu = GameObject.FindGameObjectWithTag("OptionsMenu");
		optionsPanel = optionsMenu.transform.GetChild(0);
        //Initialize a reference to the NetworkManager
        nm = GameObject.FindObjectOfType<NetworkManager>();
		//Initialize a reference to the GameManager
		gm = GameObject.FindObjectOfType<GameManager>();
		//Find the event system
		eventSystem = GameObject.Find("EventSystem");
    }

	void Update () 
	{
		//Gather button presses for processing below
		bool isPauseDown = inputState.GetButtonPressed(inputs[0]) && inputState.GetButtonHoldTime(inputs[0]) == 0;
		bool isCancelDown = inputState.GetButtonPressed(inputs[1]) && inputState.GetButtonHoldTime(inputs[1]) == 0;
		//Game state variables
		bool isGamePlaying = gm.GetGameState() == GameManager.GameState.playing;
		bool isGamePaused = gm.GetGameState() == GameManager.GameState.paused;
		//Toggle the pause menu when pause button is pressed while playing
		if((isGamePlaying || isGamePaused) && isPauseDown && !options)
		{
			TogglePauseMenu(false);
		}
		//Back button pressed while game is paused
		else if (isGamePaused && isCancelDown) 
		{
			//Back out of Options menu
			if (options) 
			{
				ToggleOptionsMenu();				
			}
			//Back out of Pause menu
			else 
			{
				TogglePauseMenu(false);
			}
		}
        else if (pickupAvailable)
        {

        }
	}

	public void TogglePauseMenu(bool mainMenu)
	{
		//Flip the flag
		paused = !paused;
		//Deselect any buttons that may have carried over
		eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
		//Show/Hide the Paused menu accordingly and update Game State
		if(paused)
		{
			ShowPauseMenu();
			gm.ChangeGameState(GameManager.GameState.paused);
		}
		else 
		{
			HidePauseMenu();
			gm.ChangeGameState(GameManager.GameState.playing);
		}
		//If we are navigating to the Main Menu
		if(mainMenu)
		{
			//Call the NetworkManager to disconnect from the server
			nm.Disconnect();
            //Unlock the mouse cursor
            UnlockMouseCursor();
        }
	}

	void ShowPauseMenu()
	{
        //Unlock the mouse cursor
        UnlockMouseCursor();
        //Simply set the pause panel to active
        pausePanel.gameObject.SetActive(true);
		//Default select the main menu button
		Transform menuButton = pausePanel.GetChild(1);
		menuButton.GetComponent<Selectable>().Select();
	}

	void HidePauseMenu()
	{
        //Lock the mouse cursor
        LockMouseCursor();
        //Simply set the pause panel to inactive
        pausePanel.gameObject.SetActive(false);
	}

	public void ToggleOptionsMenu()
	{
		//Flip the flag
		options = !options;
		//Deselect any buttons that may have carried over
		eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
		//Show/Hide the Paused menu accordingly and update Game State
		if(options)
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
        optionsPanel.gameObject.SetActive(true);
		//Default select the Close button
		Transform closeButton = optionsPanel.transform.GetChild(4);
		closeButton.GetComponent<Selectable>().Select();
	}

	void HideOptionsMenu()
	{
		//Hide the pause menu
		ShowPauseMenu();
        //Unlock the mouse cursor
        UnlockMouseCursor();
        //Simply set the Options menu to active
        optionsPanel.gameObject.SetActive(false);
	}

	//Update the mouse sensitivity in the First Person Controller
	public void changeMouseSensitivity(float newValue)
	{
		MouseSensitivity = newValue;
	}

	//Update the invert y setting in the First Person Controller
	public void changeInvertY(bool newValue)
	{
		InvertY = newValue;
    }

    //Update whether aim assist to enabled or disabled in the First Person Controller
    public void changeAimAssist(bool newValue)
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
