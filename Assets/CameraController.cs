using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform weaponHolder;
    public InputState inputState;

    public float crouchCamHeight;
    public float crouchCamDepth;
    
    private Vector3 originalLocalPosition;
    private Vector3 originalCrouchLocalPosition;

    private bool crouched = false;
    private bool aimed = false;

	void Start ()
    {
        originalLocalPosition = transform.localPosition;
        originalCrouchLocalPosition = new Vector3(transform.localPosition.x, crouchCamHeight, crouchCamDepth);
    }
	
	void Update ()
    {
        HandleAiming();
	}

    private void HandleAiming()
    {
        if (inputState.playerIsAiming)
        {
            transform.position = weaponHolder.position;
            aimed = true;
        }
        else if (!inputState.playerIsAiming && aimed)
        {
            Vector3 target = inputState.playerIsCrouching ? originalCrouchLocalPosition : originalLocalPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * 8f);
            if (transform.localPosition == originalLocalPosition)
            {
                aimed = false;
            }
        }
    }
}
