using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccuracyController : AbstractBehavior 
{
	//Public class globals
	[HideInInspector] public float totalOffset;

	//Private class globals
	private float baseOffset;
	private float shootingOffset;
	private float maxAccuracyOffset = 0.06F;
	private float sprintAccuracy = 0.0225F;
	private float walkAccuracy = 0.01125F;
	private float crouchAccuracy = 0.0075F;
	private float accuracyReduceTimer = 0F;
	private float accuracyReduceTimerMax = 0.45F;
    /*
    private float maxAccuracyOffset = 0.08F;
    private float sprintAccuracy = 0.03F;
    private float walkAccuracy = 0.015F;
    private float crouchAccuracy = 0.01F;
    private float accuracyReduceTimer = 0F;
    private float accuracyReduceTimerMax = 0.45F;
    */

    //Reticles
    private Transform reticleParent;
	private RectTransform topRet; 
	private RectTransform botRet;
	private RectTransform leftRet;
	private RectTransform rightRet;
	private float topRetY;
	private float botRetY;
	private float leftRetX;
	private float rightRetX;
	private bool reticlesMisplaced = false;

    private BodyController bodyController;
    private PlayerBodyData playerBodyData;

	void Start () {
        //Get a reference to the player body data so we can get weapon information
        bodyController = GetComponent<BodyController>();
        //Get a reference to the weapon's Reticle object
        SetBodyControlVars();
        //Get references to all reticles in the crosshair
        topRet = reticleParent.GetChild(0).GetComponent<RectTransform>(); 
		botRet = reticleParent.GetChild(1).GetComponent<RectTransform>();
		leftRet = reticleParent.GetChild(2).GetComponent<RectTransform>();
		rightRet = reticleParent.GetChild(3).GetComponent<RectTransform>();
		//Get the initial x and y values for our reticles
		topRetY = topRet.anchoredPosition3D.y;
		botRetY = botRet.anchoredPosition3D.y;
		leftRetX = leftRet.anchoredPosition3D.x;
		rightRetX = rightRet.anchoredPosition3D.x;
		//Default to perfect accuracy
		totalOffset = 0F;
	}

    private void SetBodyControlVars()
    {
        playerBodyData = bodyController.PlayerBodyData;
        reticleParent = playerBodyData.weapon.GetComponent<WeaponData>().ReticleParent.transform;
    }

    void Update()
    {
        //Make sure reticle parent stays up to date in case our weapon data changes
        SetBodyControlVars();

        //Some short-hand variables
        bool isMoving = inputState.playerVelocity.x != 0 || inputState.playerVelocity.z != 0;

        //Determine if we should apply a base accuracy offset based on movement
        if (isMoving && inputState.playerIsGrounded)
        {
            //Check if we're sprinting, crouching, or walking and apply corresponding accuracy
            if (inputState.playerIsSprinting)
            {
                baseOffset = sprintAccuracy;
            }
            else if (inputState.playerIsCrouching)
            {
                baseOffset = crouchAccuracy;
            }
            else
            {
                baseOffset = walkAccuracy;
            }
        }
        else if (inputState.playerIsGrounded)
        {
            baseOffset = 0F;
        }
        //Wall-running OR jumping
        else
        {
            baseOffset = sprintAccuracy;
        }

        //Add any mods due to rapid firing
        if (shootingOffset > 0 && accuracyReduceTimer <= 0)
        {
            //Quickly decrease accuracy penalty after not shooting for X amount of time
            shootingOffset -= Time.deltaTime;
        }
        else if (shootingOffset < 0)
        {
            shootingOffset = 0;
        }

        //Calculate the total offset due to moving + shooting
        totalOffset = baseOffset + shootingOffset;
        totalOffset = Mathf.Clamp(totalOffset, 0, maxAccuracyOffset);

        //Handle spread of the reticles based on accuracy offset
        if (totalOffset > 0)
        {
            SpreadReticles();
        }
        else
        {
            CloseReticles();
        }

        //Change the color of the reticles if we're aiming at another player
        ColorReticles(inputState.playerLockedOnEnemy);

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
		if(!reticlesMisplaced)
		{
            //Lerp values to pass to the new positions
            float lerpSpeed = Time.deltaTime * 2.25f;
            float topRetLerp = Mathf.Lerp(topRet.anchoredPosition3D.y, (totalOffset * 600) + topRetY, lerpSpeed);
			float botRetLerp = Mathf.Lerp(botRet.anchoredPosition3D.y, (-totalOffset * 600) + botRetY, lerpSpeed);
			float leftRetLerp = Mathf.Lerp(leftRet.anchoredPosition3D.x, (-totalOffset * 600) + leftRetX, lerpSpeed);
			float rightRetLerp = Mathf.Lerp(rightRet.anchoredPosition3D.x, (totalOffset * 600) + rightRetX, lerpSpeed);
			//Increase the distance of reticles
			topRet.anchoredPosition3D = new Vector3(topRet.anchoredPosition3D.x, topRetLerp, topRet.anchoredPosition3D.z);
			botRet.anchoredPosition3D = new Vector3(botRet.anchoredPosition3D.x, botRetLerp, botRet.anchoredPosition3D.z);
			leftRet.anchoredPosition3D = new Vector3(leftRetLerp, leftRet.anchoredPosition3D.y, leftRet.anchoredPosition3D.z);
			rightRet.anchoredPosition3D = new Vector3(rightRetLerp, rightRet.anchoredPosition3D.y, rightRet.anchoredPosition3D.z);
		}
	}

	private void CloseReticles()
	{
		if(!reticlesMisplaced)
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
        reticlesMisplaced = false;
		topRet.anchoredPosition3D = new Vector3(topRet.anchoredPosition3D.x, topRetY, topRet.anchoredPosition3D.z);
		botRet.anchoredPosition3D = new Vector3(botRet.anchoredPosition3D.x, botRetY, botRet.anchoredPosition3D.z);
		leftRet.anchoredPosition3D = new Vector3(leftRetX, leftRet.anchoredPosition3D.y, leftRet.anchoredPosition3D.z);
		rightRet.anchoredPosition3D = new Vector3(rightRetX, rightRet.anchoredPosition3D.y, rightRet.anchoredPosition3D.z);
	}

	public void MoveReticles()
	{
        reticlesMisplaced = true;
		topRet.anchoredPosition3D = new Vector3(topRet.anchoredPosition3D.x, 50, topRet.anchoredPosition3D.z);
		botRet.anchoredPosition3D = new Vector3(botRet.anchoredPosition3D.x, -50, botRet.anchoredPosition3D.z);
		leftRet.anchoredPosition3D = new Vector3(-50, leftRet.anchoredPosition3D.y, leftRet.anchoredPosition3D.z);
		rightRet.anchoredPosition3D = new Vector3(50, rightRet.anchoredPosition3D.y, rightRet.anchoredPosition3D.z);
	}

    private void ColorReticles(bool lockedOn)
    {
        topRet.GetComponent<Image>().color = lockedOn ? Color.red : Color.white;
        botRet.GetComponent<Image>().color = lockedOn ? Color.red : Color.white;
        leftRet.GetComponent<Image>().color = lockedOn ? Color.red : Color.white;
        rightRet.GetComponent<Image>().color = lockedOn ? Color.red : Color.white;
    }
}