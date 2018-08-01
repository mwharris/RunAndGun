﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof (CrouchController))]
public class SprintController : AbstractBehavior {

	private CrouchController crouchController;

	void Start()
	{
		crouchController = GetComponent<CrouchController>();
	}

	void Update()
	{
		//Gather the inputs to process below
		bool isSprintDown = inputState.GetButtonPressed(inputs[0]);
		float sprintHoldTime = inputState.GetButtonHoldTime(inputs[0]);
		bool isForwardDown = inputState.GetButtonPressed(inputs[1]);
		//Toggle Sprinting whenever we hit the sprint button except while aiming
		if (isSprintDown && sprintHoldTime == 0f && !inputState.playerIsAiming)
		{
			//Flag us as sprinting
			inputState.playerIsSprinting = !inputState.playerIsSprinting;
			//Come out of crouch if we're crouching
			if(inputState.playerIsSprinting && inputState.playerIsCrouching)
			{
				crouchController.StopCrouching();
			}
		}
		//If we Aim while Sprinting, toggle Sprinting off in order to allow Aim out of Sprint
		else if (isSprintDown && inputState.playerIsAiming)
		{
			inputState.playerIsSprinting = false;
		}
		//Only sprint while holding forward
		else if(!isForwardDown && inputState.playerIsSprinting)
		{
			inputState.playerIsSprinting = false;
		}
	}

    //Outside functions can call this to disable sprinting
    public void DisableSprinting()
    {
        inputState.playerIsSprinting = false;
    }

}
