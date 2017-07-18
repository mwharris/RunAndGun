using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccuracyController : MonoBehaviour {

	//Public class globals
	[HideInInspector] public float totalOffset;

	//Private class globals
	private float baseOffset;
	private float shootingOffset;
	private FirstPersonController fpsController;
	private CrouchController crouchController;
	private float maxAccuracyOffset = 0.08F;
	private float accuracyChangeSpeed = 1.5F;
	private float sprintAccuracy = 0.03F;
	private float walkAccuracy = 0.015F;
	private float crouchAccuracy = 0.01F;
	private float accuracyReduceTimer = 0F;
	private float accuracyReduceTimerMax = 0.45F;

	//Reticles
	private GameObject reticleParent;
	private RectTransform topRet; 
	private RectTransform botRet;
	private RectTransform leftRet;
	private RectTransform rightRet;
	private float topRetY;
	private float botRetY;
	private float leftRetX;
	private float rightRetX;
	private float reticleSpreadSpeed = 0.01F;

	private bool reticlesFuckedUp = false;

	void Start () {
		//Get a reference to the FPS Controller
		fpsController = GetComponent<FirstPersonController>();
		crouchController = GetComponent<CrouchController>();
		//Get a reference to the Reticle object
		reticleParent = GameObject.FindGameObjectWithTag("Reticle");
		//Get references to all reticles in the crosshair
		topRet = reticleParent.transform.GetChild(0).GetComponent<RectTransform>(); 
		botRet = reticleParent.transform.GetChild(1).GetComponent<RectTransform>();
		leftRet = reticleParent.transform.GetChild(2).GetComponent<RectTransform>();
		rightRet = reticleParent.transform.GetChild(3).GetComponent<RectTransform>();
		//Get the initial x and y values for our reticles
		topRetY = topRet.anchoredPosition3D.y;
		botRetY = botRet.anchoredPosition3D.y;
		leftRetX = leftRet.anchoredPosition3D.x;
		rightRetX = rightRet.anchoredPosition3D.x;
		//Default to perfect accuracy
		totalOffset = 0F;
	}
	
	void Update () {
		//Some short-hand variables
		bool isMoving = fpsController.forwardSpeed != 0 || fpsController.sideSpeed != 0;
		bool isGrounded = fpsController.cc.isGrounded;

		//Determine if we should apply a base accuracy offset based on movement
		if(isMoving && isGrounded)
		{
			//Check if we're sprinting, crouching, or walking and apply corresponding accuracy
			if(fpsController.isSprinting)
			{
				baseOffset = sprintAccuracy;
			}
			else if(crouchController.isCrouching)
			{
				baseOffset = crouchAccuracy;
			}
			else
			{
				baseOffset = walkAccuracy;
			}
		}
		else if(isGrounded)
		{
			baseOffset = 0F;
		}
		//Wall-running OR jumping
		else
		{
			baseOffset = sprintAccuracy;
		}

		//Add any mods due to rapid firing
		if(shootingOffset > 0 && accuracyReduceTimer <= 0)
		{
			//Quickly decrease accuracy penalty after not shooting for X amount of time
			shootingOffset -= Time.deltaTime;
		}
		else if(shootingOffset < 0)
		{
			shootingOffset = 0;
		}

		//Calculate the total offset due to moving + shooting
		totalOffset = baseOffset + shootingOffset;
		totalOffset = Mathf.Clamp(totalOffset, 0, maxAccuracyOffset);

		//Handle spread of the reticles based on accuracy offset
		if(totalOffset > 0)
		{
			SpreadReticles();
		}
		else
		{
			CloseReticles();
		}

		//Reduce the timer if running
		accuracyReduceTimer -= Time.deltaTime;
	}

	//Hlper function called by ShootController to decrease accuracy after every shot
	public void AddShootingOffset(bool aimFire)
	{
		//Aiming should be right down the sights...?
		if(aimFire)
		{
			shootingOffset += 0;
		}
		//Hip fire should have a larger shot spread
		else
		{
			shootingOffset += Time.deltaTime / 1.5F;
		}
		//Start our timer
		accuracyReduceTimer = accuracyReduceTimerMax;
	}

	private void SpreadReticles()
	{
		if(!reticlesFuckedUp)
		{
			//Lerp values to pass to the new positions
			float topRetLerp = Mathf.Lerp(topRet.anchoredPosition3D.y, (totalOffset * 800) + topRetY, Time.deltaTime * 3F);
			float botRetLerp = Mathf.Lerp(botRet.anchoredPosition3D.y, (-totalOffset * 800) + botRetY, Time.deltaTime * 3F);
			float leftRetLerp = Mathf.Lerp(leftRet.anchoredPosition3D.x, (-totalOffset * 800) + leftRetX, Time.deltaTime * 3F);
			float rightRetLerp = Mathf.Lerp(rightRet.anchoredPosition3D.x, (totalOffset * 800) + rightRetX, Time.deltaTime * 3F);
			//Increase the distance of reticles
			topRet.anchoredPosition3D = new Vector3(topRet.anchoredPosition3D.x, topRetLerp, topRet.anchoredPosition3D.z);
			botRet.anchoredPosition3D = new Vector3(botRet.anchoredPosition3D.x, botRetLerp, botRet.anchoredPosition3D.z);
			leftRet.anchoredPosition3D = new Vector3(leftRetLerp, leftRet.anchoredPosition3D.y, leftRet.anchoredPosition3D.z);
			rightRet.anchoredPosition3D = new Vector3(rightRetLerp, rightRet.anchoredPosition3D.y, rightRet.anchoredPosition3D.z);
		}
	}

	private void CloseReticles()
	{
		if(!reticlesFuckedUp)
		{
			//Lerp values to pass to the new positions
			float topRetLerp = Mathf.Lerp(topRet.anchoredPosition3D.y, topRetY, Time.deltaTime * 2);
			float botRetLerp = Mathf.Lerp(botRet.anchoredPosition3D.y, botRetY, Time.deltaTime * 2);
			float leftRetLerp = Mathf.Lerp(leftRet.anchoredPosition3D.x, leftRetX, Time.deltaTime * 2);
			float rightRetLerp = Mathf.Lerp(rightRet.anchoredPosition3D.x, rightRetX, Time.deltaTime * 2);
			//Decrease the distance of reticles
			topRet.anchoredPosition3D = new Vector3(topRet.anchoredPosition3D.x, topRetLerp, topRet.anchoredPosition3D.z);
			botRet.anchoredPosition3D = new Vector3(botRet.anchoredPosition3D.x, botRetLerp, botRet.anchoredPosition3D.z);
			leftRet.anchoredPosition3D = new Vector3(leftRetLerp, leftRet.anchoredPosition3D.y, leftRet.anchoredPosition3D.z);
			rightRet.anchoredPosition3D = new Vector3(rightRetLerp, rightRet.anchoredPosition3D.y, rightRet.anchoredPosition3D.z);
		}
	}

	public void ResetReticles()
	{
		reticlesFuckedUp = false;
		topRet.anchoredPosition3D = new Vector3(topRet.anchoredPosition3D.x, topRetY, topRet.anchoredPosition3D.z);
		botRet.anchoredPosition3D = new Vector3(botRet.anchoredPosition3D.x, botRetY, botRet.anchoredPosition3D.z);
		leftRet.anchoredPosition3D = new Vector3(leftRetX, leftRet.anchoredPosition3D.y, leftRet.anchoredPosition3D.z);
		rightRet.anchoredPosition3D = new Vector3(rightRetX, rightRet.anchoredPosition3D.y, rightRet.anchoredPosition3D.z);
	}

	public void FuckUpReticles()
	{
		reticlesFuckedUp = true;
		topRet.anchoredPosition3D = new Vector3(topRet.anchoredPosition3D.x, 50, topRet.anchoredPosition3D.z);
		botRet.anchoredPosition3D = new Vector3(botRet.anchoredPosition3D.x, -50, botRet.anchoredPosition3D.z);
		leftRet.anchoredPosition3D = new Vector3(-50, leftRet.anchoredPosition3D.y, leftRet.anchoredPosition3D.z);
		rightRet.anchoredPosition3D = new Vector3(50, rightRet.anchoredPosition3D.y, rightRet.anchoredPosition3D.z);
	}
}
