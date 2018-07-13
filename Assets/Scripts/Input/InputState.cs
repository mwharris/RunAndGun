using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonState {
	
	public bool pressed;
	public float value;
	public float holdTime = 0f;

}

public class InputState : MonoBehaviour {

	Dictionary<Buttons, ButtonState> buttonStates = new Dictionary<Buttons, ButtonState>();

	[HideInInspector] public bool playerIsGrounded = false;
	[HideInInspector] public bool playerIsSprinting = false;
	[HideInInspector] public bool playerIsCrouching = false;
    [HideInInspector] public bool playerIsShooting = false;
    [HideInInspector] public Vector3 playerLookVec = new Vector3(0, 0, 0);
    [HideInInspector] public Vector3 playerVelocity = Vector3.zero;
	[HideInInspector] public bool allowAirMovement = false;
    [HideInInspector] public float playerLookAngle = 0f;

    public void SetButtonState(Buttons key, bool pressed, float value)
	{
		//Add this button to our states list if it doesn't exist already
		if (!buttonStates.ContainsKey(key))
		{
			buttonStates.Add(key, new ButtonState());
		}	

		//Get the existing button state
		ButtonState state = buttonStates[key];

		//The button was pressed but is no longer
		if (state.pressed && !pressed)
		{
			state.holdTime = 0f;
		}
		//The button is being held
		else if (state.pressed && pressed) 
		{
			state.holdTime += Time.deltaTime;
		}	

		//Update the existing value with the passed-in values
		buttonStates[key].pressed = pressed;
		buttonStates[key].value = value; 
	}

	//Simply get the value of a button from our button states
	public bool GetButtonPressed(Buttons key)
	{
		if (buttonStates.ContainsKey(key))
		{
			return buttonStates[key].pressed;
		}
		else
		{
			return false;
		}
	}

	//Simply get the value of a button from our button states
	public float GetButtonValue(Buttons key)
	{
		if (buttonStates.ContainsKey(key))
		{
			return buttonStates[key].value;
		}
		else
		{
			return 0f;
		}
	}

	//Simply get the value of a button from our button states
	public float GetButtonHoldTime(Buttons key)
	{
		if (buttonStates.ContainsKey(key))
		{
			return buttonStates[key].holdTime;
		}
		else
		{
			return 0f;
		}
	}
}
