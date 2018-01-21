using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamepadInputFieldHelper : MonoBehaviour {

	private InputField inputField;

	void Start () 
	{
		inputField = GetComponent<InputField>();
	}
	
	void Update () 
	{
		//Add ability to exit an input field when A or Enter is pressed
		if (Input.GetButtonUp("Submit") || Input.GetButtonUp("Jump")) 
		{
			inputField.DeactivateInputField();
		}
	}
}
