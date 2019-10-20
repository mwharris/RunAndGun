using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShootController : AbstractBehavior 
{
    //General global variables - Set by Unity
    [SerializeField] private float kickDampening = 0.25f;
    [SerializeField] private AudioSource aSource;

    //General global variables - private
    private Camera playerCamera;
    private Animator reloadAnimator;
	private float baseFOV;
	private PhotonView pView;
	private float aimRecoilAmount = 0.05f;
	private float hipRecoilAmount = 0.1f;

    //Weapon specific stuff
	private float cooldownTimer;
	private float reloadTimer = 0f;

	//HUD variables
	private Text bulletCountText;
	private Text clipSizeText;
	private GameObject reloadIndicator;
	private GameObject hitIndicator;
	private float hitIndicatorTimer;
	private float hitIndicatorTimerMax;

	//Imported Scripts
	private FXManager fxManager;
    private RPCManager rpcManager;
	private GameManager gm;
	private RecoilController rc;
	private AccuracyController ac;
    private BodyController bodyControl;
    private PlayerBodyData pbd;
    private WeaponData currWeaponData;

    void Start()
	{
        //Get components from the player body data
        bodyControl = GetComponent<BodyController>();
        SetBodyControlVars();
        playerCamera = pbd.playerCamera.GetComponent<Camera>();
        reloadAnimator = pbd.GetBodyAnimator();
        //Initialize timers
        cooldownTimer = 0.0f;
		hitIndicatorTimerMax = 0.3f;
		hitIndicatorTimer = hitIndicatorTimerMax;
		//Initialize a reference to the FX and RPC Managers
		fxManager = GameObject.FindObjectOfType<FXManager>();
        rpcManager = GameObject.FindObjectOfType<RPCManager>();
        //Initialize a reference to the GameManager
        gm = GameObject.FindObjectOfType<GameManager>();
		//Initialize a reference to the Recoil and Accuracy controllers
		rc = GetComponent<RecoilController>();
		ac = GetComponent<AccuracyController>();
        //Initialize a reference to the Hit Indicators
        hitIndicator = GameObject.FindGameObjectWithTag("HitIndicator");
		reloadIndicator = GameObject.FindGameObjectWithTag("ReloadIndicator");
		//Initialize a reference to the bullet count
		GameObject txt = GameObject.FindGameObjectWithTag("BulletCount");
		bulletCountText = txt.GetComponent<Text>();
        bulletCountText.text = currWeaponData.MagazineCapacity.ToString();
        //...and max bullet capacity
        txt = GameObject.FindGameObjectWithTag("ClipSize");
		clipSizeText = txt.GetComponent<Text>();
		clipSizeText.text = currWeaponData.MagazineCapacity.ToString();
		//Get the attached PhotonView
		pView = GetComponent<PhotonView>();
		//Get the camera's original FOV range
		baseFOV = playerCamera.fieldOfView;
	}

    void Update()
    {
        //Make sure we keep our variables from the body controller up to date
        SetBodyControlVars();

        //Gather inputs needed below
        bool isShootDown = inputState.GetButtonPressed(inputs[0]);
		bool isAimDown = inputState.GetButtonPressed(inputs[1]);
		bool isReloadDown = inputState.GetButtonPressed(inputs[2]);

		//Hide the hit indicators from last frame
		HideHitIndicator();			

		//Make sure we aren't reloading
		if(gm.GetGameState() == GameManager.GameState.playing && reloadTimer <= 0) {
			//Update the displayed bullet count
			bulletCountText.text = currWeaponData.BulletCount.ToString();
            clipSizeText.text = currWeaponData.MagazineCapacity.ToString();

			//Determine if we are attempting to aim our weapon
			if(isAimDown)
			{
				Aim();
			}
			else if (inputState.playerIsAiming)
			{
				StopAiming();
			}

			//Determine if we try to shoot the weapon
			if(isShootDown && cooldownTimer <= 0)
			{
				//Only fire if there are bullets in the clip
				if(currWeaponData.BulletCount > 0)
				{
					Shoot(isAimDown);
				}
				//If the clip is empty, reload instead
				else
                {
                    Reload();
                }
			}
            //If we're not shooting than make sure we don't flag ourselves as such
            else
            {
                inputState.playerIsShooting = false;
            }

			//Reload if 'R' is pressed OR we tried to shoot while the clip is empty
			if(isReloadDown)
			{
				Reload();
			}
		}
		else if (gm.GetGameState() == GameManager.GameState.playing) {
			//Update the displayed bullet count
			bulletCountText.text = "--";
			//Decrement the reload timer if we are reloading
			reloadTimer -= Time.deltaTime;
		}

		//Decrement the cooldown timer
		cooldownTimer -= Time.deltaTime;
	}

    private void SetBodyControlVars()
    {
        if (bodyControl != null)
        {
            pbd = bodyControl.PlayerBodyData;
            currWeaponData = pbd.weapon.GetComponent<WeaponData>();
        }
    }

    //Handle aiming our weapon
    void Aim()
	{
		//Tell the animator to pull the gun to our face
		inputState.playerIsAiming = true;
		//Disable the crosshairs
        if (currWeaponData.HideReticleOnAim)
        {
            foreach (Transform child in currWeaponData.ReticleParent.transform)
            {
                child.GetComponent<Image>().enabled = false;
            }
        }
        //Make sure we turn off sprinting
        inputState.playerIsSprinting = false;
		//Zoom the camera in
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 50, 10 * Time.deltaTime);
	}

	//Helper method used by other classes to tell ShootController to stop aiming.
	//Usually due to jumping, running, or wall-running.
	public void StopAiming()
	{
		//Tell the animator to pull the gun to the hip
        inputState.playerIsAiming = false;
		//Enable the crosshairs
		foreach(Transform child in currWeaponData.ReticleParent.transform)
		{
			child.GetComponent<Image>().enabled = true;
		}
		//Zoom the camera out to normal
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, baseFOV, 10 * Time.deltaTime);
	}

	//Handle everything that needs to be done to fire a weapon
	void Shoot(bool isAimDown)
	{
		//Reset the shot timer
		cooldownTimer = currWeaponData.ShotDelayTime;

		//Decrement the bullet counter
		currWeaponData.BulletCount--;

		//Shoot a Ray and find the closest thing we hit that isn't ourselves
		Vector3 shootVector = ApplyAccuracy(playerCamera.transform.forward);
		Ray ray = new Ray(playerCamera.transform.position, shootVector);
		HitPlayerInfo info = FindClosestHitInfo(ray);

        //Notify other controllers and players that we are shooting
        inputState.playerIsShooting = true;
        rpcManager.GetComponent<PhotonView>().RPC("PlayerShot", PhotonTargets.AllBuffered, pView.owner.ID);

		//Handle Recoil and Accuracy updates based on if we're aiming
		if(isAimDown)
		{
			rc.StartRecoil(aimRecoilAmount);
			ac.AddShootingOffset(inputState.playerIsAiming);
            pbd.body.transform.localPosition += kickDampening * currWeaponData.KickAmount;
            pbd.body.transform.localEulerAngles += kickDampening * currWeaponData.RotKickAmount;
        }
		else
		{
			rc.StartRecoil(hipRecoilAmount);
			ac.AddShootingOffset(inputState.playerIsAiming);
            pbd.body.transform.localPosition += currWeaponData.KickAmount;
            pbd.body.transform.localEulerAngles += currWeaponData.RotKickAmount;
        }

		//Check if we hit a red object
		bool hitRed = false;
		if(info.hitTransform != null && info.hitTransform.tag == "Red")
		{
			hitRed = true;
		}

		//Flag to show if we hit an enemy or not
		bool hitEnemy = false;

		//Make sure we actually hit something
		if(info.hitTransform != null)
		{
			//Determine if the object we hit has hit points
			Health h = info.hitTransform.GetComponent<Health>();
			//If we did not find an object with health
			if(h == null)
			{
				//Loop through it's parents and try to find one that has health
				while(info.hitTransform.parent && h == null)
				{
                    info.hitTransform = info.hitTransform.parent;
					h = info.hitTransform.GetComponent<Health>();
				}
			}
			//Check if we eventually found an object with health
			if(h != null)
			{
				//Show the hit indicator
				hitEnemy = true;
				ShowHitIndicator();
				//Use an RPC to send damage over the network
				PhotonView pv = h.GetComponent<PhotonView>();
				if(pv != null)
				{
                    pv.RPC(
                        "TakeDamage", 
                        PhotonTargets.AllBuffered,
                        currWeaponData.WeaponDamage, 
                        pView.owner.NickName, 
                        pView.owner.ID, 
                        transform.position,
                        info.headshot
                    );
				}
			} 

			//Show some bullet FX
			if(fxManager != null)
			{
				GunFX(info.hitPoint, hitEnemy, hitRed);
			}
		}
		//Hit nothing, show bullet FX anyway
		else if(fxManager != null)
		{
            //Make the FX reach a certain distance from the camera
            info.hitPoint = playerCamera.transform.position + (playerCamera.transform.forward * 100f);
			GunFX(info.hitPoint, hitEnemy, hitRed);
		}
	}

	//Take a shooting vector and apply offsets based on recoil, weapon type
	private Vector3 ApplyAccuracy(Vector3 shootVector)
	{
		shootVector.x += WeightedRandomAccuracy(ac.totalOffset);
		shootVector.y += WeightedRandomAccuracy(ac.totalOffset);
		shootVector.z += WeightedRandomAccuracy(ac.totalOffset);
		return shootVector;
	}

	private float WeightedRandomAccuracy(float accuracyRange)
	{
		//This is 1/3 of the range
		float quarter = ac.totalOffset / 4;
		float twoThirds = (ac.totalOffset / 3) * 2;
		//Get a random value between 0 and 1
		float rand = Random.value;
		//40% change to shoot in the middle 25% of the accuracy range
		if(rand <= 0.4F)
		{
			return Random.Range(-quarter, quarter);
		}
		//35% chance to shoot in the middle 60% of the accuracy range
		if(rand <= 0.75F)
		{
			return Random.Range(-twoThirds, twoThirds);
		} 
		//20% to shoot anywhere within the accuracy range
		return Random.Range(-accuracyRange, accuracyRange);
	}

	//Helper function to reload our weapon
	void Reload()
	{
        //Notify other players that we are reloading (for animations)
        rpcManager.GetComponent<PhotonView>().RPC("PlayerReloaded", PhotonTargets.AllBuffered, pView.owner.ID);
        //Start the reload timer
        reloadTimer = currWeaponData.ReloadTime;
		//Reset the bullet count
		currWeaponData.BulletCount = currWeaponData.MagazineCapacity;
		//Play a sound
		aSource.PlayOneShot(currWeaponData.ReloadClip);
		//Play the reload animation
        inputState.playerIsReloading = true;
        inputState.playerIsShooting = false;
	}

	//Helper function to show the gun FX
	void GunFX(Vector3 hitPoint, bool hitEnemy, bool hitRed)
	{
		//Grab the location of the gun and spawn the FX there
		fxManager.GetComponent<PhotonView>().RPC("BulletFX", PhotonTargets.All, pView.owner.ID, hitPoint, hitEnemy, hitRed);
	}

    //Raycast in a line and find the closest object hit
    private HitPlayerInfo FindClosestHitInfo(Ray ray)
    {
        HitPlayerInfo info = new HitPlayerInfo();
        //Get all objects that our raycast hit
        RaycastHit[] hits = Physics.RaycastAll(ray);
		//Loop through all the things we hit
		foreach(RaycastHit hit in hits)
		{
			//Find the closest object we hit that is not ourselves
			if(hit.transform != this.transform 
				&& !hit.transform.IsChildOf(this.transform) 
				&& (info.hitTransform == null || hit.distance < info.distance))
			{
                //Update the closest hit and distance
                info.hitTransform = hit.transform;
                info.distance = hit.distance;
                info.hitPoint = hit.point;
                info.headshot = hit.collider.GetType() == typeof(BoxCollider);
            }
		}
		return info;
	}

	//Display the hit indicators
	void ShowHitIndicator()
	{
		hitIndicatorTimer = hitIndicatorTimerMax;
		//Show them all
		foreach (Transform child in hitIndicator.transform)
		{
			child.gameObject.SetActive(true);
		}
	}

	//Hide the hit indicators after some time
	void HideHitIndicator()
	{
		hitIndicatorTimer -= Time.deltaTime;
		//Show them all
		if(hitIndicatorTimer <= 0)
		{
			foreach (Transform child in hitIndicator.transform)
			{
				child.gameObject.SetActive(false);
			}
		}
	}

    //Wait until the animation is done to allow shooting
    public IEnumerator WaitForRecoilDone(float time)
	{
		yield return new WaitForSeconds(time);
        inputState.playerIsShooting = false;
	}
}
