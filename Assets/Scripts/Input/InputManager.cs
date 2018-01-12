using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Buttons {
	Right,
	Left,
	Forward,
	Backward,
	LookRight,
	LookLeft,
	LookUp,
	LookDown,
	A,
	B
}

public enum Condition {
	GreaterThan,
	LessThan
}

[System.Serializable]
public class InputAxisState {
	
	public string axisName;
	public float offValue;
	public Buttons button;
	public Condition condition;

	public bool pressed {
		get {
			//Get the value of the axis using axisName
			var val = Input.GetAxis(axisName);

			//Check if our axis value passes our threshold value via the condition (Greater or Less)
			switch (condition) {
			case Condition.GreaterThan:
				return val > offValue;
			case Condition.LessThan:
				return val < offValue;
			}

			//Default return false
			return false;
		}
	}

	public float value {
		get {
			//Get the value of the acis using axisName
			float val = Input.GetAxis(axisName);

			//Check if our axis value passes our threshold value via the condition (Greater or Less)
			switch (condition) {
			case Condition.GreaterThan:
				if (val > offValue)
				{
					return val;
				}
				else
				{
					return 0f;
				}
			case Condition.LessThan:
				if (val < offValue)
				{
					return val;
				}
				else
				{
					return 0f;
				}
			}

			//Default return 0
			return 0f;
		}
	}
}

public class InputManager : MonoBehaviour {

	public InputAxisState[] inputs;
	public InputState inputState;

	void Update()
	{
		if (inputState != null)
		{
			foreach(InputAxisState input in inputs)
			{
				inputState.SetButtonState(input.button, input.pressed, input.value);
			}
		}
	}
}
