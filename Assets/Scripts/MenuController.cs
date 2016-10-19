using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour {

	private bool paused = false;
	private GameObject pauseMenu;
	private GameObject eventSystem;
	private NetworkManager nm;
	private GameManager gm;

	void Start()
	{
		//Find the pause menu GameObject
		pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
		//Initialize a reference to the NetworkManager
		nm = GameObject.FindObjectOfType<NetworkManager>();
		//Initialize a reference to the GameManager
		gm = GameObject.FindObjectOfType<GameManager>();
		//Find the event system
		eventSystem = GameObject.Find("EventSystem");
	}

	void Update () {
		//Toggle the pause menu when ESC key is pressed
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			TogglePauseMenu(false);
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
		Transform panel = pauseMenu.transform.GetChild(0);
		Transform menuButton = panel.GetChild(2);
		Transform closeButton = panel.GetChild(3);
		//The panel
		panel.GetComponent<Image>().enabled = true;
		//Pause menu text
		panel.GetChild(0).GetComponent<Text>().enabled = true;
		panel.GetChild(1).GetComponent<Text>().enabled = true;
		//Main menu button
		menuButton.GetComponent<Image>().enabled = true;
		menuButton.GetComponentInChildren<Text>().enabled = true;
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
		Transform menuButton = panel.GetChild(2);
		Transform closeButton = panel.GetChild(3);
		//The panel
		panel.GetComponent<Image>().enabled = false;
		//Pause menu text
		panel.GetChild(0).GetComponent<Text>().enabled = false;
		panel.GetChild(1).GetComponent<Text>().enabled = false;
		//Main menu button
		menuButton.GetComponent<Image>().enabled = false;
		menuButton.GetComponentInChildren<Text>().enabled = false;
		//Close button
		closeButton.GetComponent<Image>().enabled = false;
		closeButton.GetComponentInChildren<Text>().enabled = false;
	}
}
