using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : AbstractBehavior 
{
	/*
	This is used to enable / disable this script.
	This is to prevent errors because the 3rd Person Body doesn't have this script attached.
	With this we can attach the script and just set this flag to false.
	*/
	[SerializeField] private bool handleEvents = true;

	public const string RELOAD_DONE = "RELOAD_DONE";

	public void HandleEvent(string eventName) 
	{
		if (handleEvents) 
		{
			if (RELOAD_DONE.Equals(eventName)) 
			{
				inputState.playerIsReloading = false;
			}
		}
	}

}
