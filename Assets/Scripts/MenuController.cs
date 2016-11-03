using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour {

	private bool paused = false;
	private bool options = false;
	private GameObject pauseMenu;
	private GameObject optionsMenu;
	private GameObject eventSystem;
	private NetworkManager nm;
	private GameManager gm;

	public float mouseSensitivity;

	void Start()
	{
		//Find the pause and options menu GameObject
		pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
		optionsMenu = GameObject.FindGameObjectWithTag("OptionsMenu");
		//Initialize a reference to the NetworkManager
		nm = GameObject.FindObjectOfType<NetworkManager>();
		//Initialize a reference to the GameManager
		gm = GameObject.FindObjectOfType<GameManager>();
		//Find the event system
		eventSystem = GameObject.Find("EventSystem");
	}

	void Update () {
		//Toggle the pause menu when ESC key is pressed if we're playing and the Options menu is not open
		if((gm.GetGameState() == GameManager.GameState.playing || gm.GetGameState() == GameManager.GameState.paused) 
			&& Input.GetKeyDown(KeyCode.P) && !options)
		{
			TogglePauseMenu(false);
		}
		//Lock the cursor if L was hit
		if(gm.GetGameState () == GameManager.GameState.playing && Input.GetKeyDown (KeyCode.L)) 
		{
			Cursor.lockState = CursorLockMode.Locked;
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
		}
	}

	void ShowPauseMenu()
	{
		//Unlock the mouse cursor
		Cursor.lockState = CursorLockMode.None;
		//Show all elements
		Transform panel = pauseMenu.transform.GetChild(0);
		Transform menuButton = panel.GetChild(1);
		Transform optionsButton = panel.GetChild(2);
		Transform closeButton = panel.GetChild(3);
		//The panel
		panel.GetComponent<Image>().enabled = true;
		//Pause menu text
		panel.GetChild(0).GetComponent<Text>().enabled = true;
		//Main menu button
		menuButton.GetComponent<Image>().enabled = true;
		menuButton.GetComponentInChildren<Text>().enabled = true;
		//Options menu button
		optionsButton.GetComponent<Image>().enabled = true;
		optionsButton.GetComponentInChildren<Text>().enabled = true;
		//Close button
		closeButton.GetComponent<Image>().enabled = true;
		closeButton.GetComponentInChildren<Text>().enabled = true;
	}

	void HidePauseMenu()
	{
		//Lock the mouse cursor
		Cursor.lockState = CursorLockMode.Locked;
		//Hide all elements
		Transform panel = pauseMenu.transform.GetChild(0);
		Transform menuButton = panel.GetChild(1);
		Transform optionsButton = panel.GetChild(2);
		Transform closeButton = panel.GetChild(3);
		//The panel
		panel.GetComponent<Image>().enabled = false;
		//Pause menu text
		panel.GetChild(0).GetComponent<Text>().enabled = false;
		//Main menu button
		menuButton.GetComponent<Image>().enabled = false;
		menuButton.GetComponentInChildren<Text>().enabled = false;
		//Options menu button
		optionsButton.GetComponent<Image>().enabled = false;
		optionsButton.GetComponentInChildren<Text>().enabled = false;
		//Close button
		closeButton.GetComponent<Image>().enabled = false;
		closeButton.GetComponentInChildren<Text>().enabled = false;
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
		Cursor.lockState = CursorLockMode.None;
		//Enable the options menu elements
		Transform panel = optionsMenu.transform.GetChild(0);
		Transform slider = panel.transform.GetChild(2);
		Transform closeButton = panel.transform.GetChild(3);
		//Enable the panel
		panel.GetComponent<Image>().enabled = true;
		//Enable the title text
		panel.GetChild(0).GetComponent<Text>().enabled = true;
		//Enable the mouse sensitivity text
		panel.GetChild(1).GetComponent<Text>().enabled = true;
		//Enable the slider
		slider.GetChild(0).GetComponent<Image>().enabled = true;
		slider.GetChild(1).GetChild(0).GetComponent<Image>().enabled = true;
		slider.GetChild(2).GetChild(0).GetComponent<Image>().enabled = true;
		//Enable the close button
		closeButton.GetComponent<Image>().enabled = true;
		closeButton.GetChild(0).GetComponent<Text>().enabled = true;
	}

	void HideOptionsMenu()
	{
		//Hide the pause menu
		ShowPauseMenu();
		//Unlock the mouse cursor
		Cursor.lockState = CursorLockMode.None;
		//Enable the options menu elements
		Transform panel = optionsMenu.transform.GetChild(0);
		Transform slider = panel.transform.GetChild(2);
		Transform closeButton = panel.transform.GetChild(3);
		//Enable the panel
		panel.GetComponent<Image>().enabled = false;
		//Enable the title text
		panel.GetChild(0).GetComponent<Text>().enabled = false;
		//Enable the mouse sensitivity text
		panel.GetChild(1).GetComponent<Text>().enabled = false;
		//Enable the slider
		slider.GetChild(0).GetComponent<Image>().enabled = false;
		slider.GetChild(1).GetChild(0).GetComponent<Image>().enabled = false;
		slider.GetChild(2).GetChild(0).GetComponent<Image>().enabled = false;
		//Enable the close button
		closeButton.GetComponent<Image>().enabled = false;
		closeButton.GetChild(0).GetComponent<Text>().enabled = false;
	}

	//Update the mouse sensitivity in the First Person Controller
	public void changeMouseSensitivity(float newValue)
	{
		mouseSensitivity = newValue;
	}
}
