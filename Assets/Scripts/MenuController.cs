using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour {

	private bool paused = false;
	private GameObject pauseMenu;
	private NetworkManager nm;

	void Start()
	{
		//Find the pause menu GameObject
		pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
		//Initialize a reference to the NetworkManager
		nm = GameObject.FindObjectOfType<NetworkManager>();
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
		//Show/Hide the Paused menu accordingly
		if(paused)
		{
			ShowPauseMenu();
		}
		else 
		{
			HidePauseMenu();
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
