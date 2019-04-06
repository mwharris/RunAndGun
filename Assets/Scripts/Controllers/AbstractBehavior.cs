using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractBehavior : MonoBehaviour {

	public Buttons[] inputs;

	protected InputState inputState;

	protected virtual void Awake()
	{
		inputState = GetComponent<InputState>();
        if (inputState == null)
        {
            BodyController bc = FindObjectOfType<BodyController>();
            if (bc != null)
            {
                inputState = bc.gameObject.GetComponent<InputState>();
            }
        }
	}

}
