using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShootController : MonoBehaviour 
{
	public GameObject gun;
	public Animator animator;
	public Camera playerCamera;
	private float baseFOV;
	public AudioSource aSource;
	public bool isAiming;

	//Weapon specific stuff
	public AudioClip reloadClip;
	private float weaponDamage = 25f;
	private int magazineCapacity = 12;
	private float weaponReloadTime = 2.5f;
	private float reloadTimer = 0f;

	private Text bulletCountText;
	private Text clipSizeText;
	private int bulletCount;
	private GameObject hitIndicator;
	private float hitIndicatorTimer;
	private float hitIndicatorTimerMax;
	[HideInInspector] public float cooldownTimer;
	private float cooldownTimerMax = 0.315f;
	private PhotonView pView;
	private GameObject reticleParent;
	private float aimRecoilAmount = 0.05f;
	private float hipRecoilAmount = 0.1f;

	//Imported Scripts
	private FXManager fxManager;
	private GameManager gm;
	private RecoilController rc;
	private AccuracyController ac;

	void Start()
	{
		//Initialize timers
		cooldownTimer = 0.0f;
		hitIndicatorTimerMax = 0.3f;
		hitIndicatorTimer = hitIndicatorTimerMax;

		//Initialize a reference to the FXManager
		fxManager = GameObject.FindObjectOfType<FXManager>();
		//Initialize a reference to the GameManager
		gm = GameObject.FindObjectOfType<GameManager>();
		//Initialize a reference to the Recoil and Accuracy controllers
		rc = this.gameObject.GetComponent<RecoilController>();
		ac = this.gameObject.GetComponent<AccuracyController>();
		//Initialize a reference to the Hit Indicators
		hitIndicator = GameObject.FindGameObjectWithTag("HitIndicator");

		//Initialize a reference to the bullet count
		GameObject txt = GameObject.FindGameObjectWithTag("BulletCount");
		bulletCountText = txt.GetComponent<Text>();
		//...and max bullet capacity
		txt = GameObject.FindGameObjectWithTag("ClipSize");
		clipSizeText = txt.GetComponent<Text>();
		clipSizeText.text = magazineCapacity.ToString();

		//Get a reference to the Reticle object
		reticleParent = GameObject.FindGameObjectWithTag("Reticle");

		//Default the bullet count to the max magazin capacity
		bulletCount = magazineCapacity;

		//Get the attached PhotonView
		pView = GetComponent<PhotonView>();

		//Get the camera's original FOV range
		baseFOV = playerCamera.fieldOfView;
	}

	void Update() 
	{
		//Reset the reload animation
		if(animator.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
		{
			animator.SetBool("Reload", false);
		}
		//Hide the hit indicators from last frame
		HideHitIndicator();			

		//Make sure we aren't reloading
		if(gm.GetGameState() == GameManager.GameState.playing && reloadTimer <= 0) {
			//Update the displayed bullet count
			bulletCountText.text = bulletCount.ToString();

			//Determine if we are attempting to aim our weapon
			if(Input.GetKey(KeyCode.Mouse1))
			{
				Aim();
			}
			else if(!Input.GetKey(KeyCode.Mouse1))
			{
				StopAiming();
			}

			//Determine if we try to shoot the weapon
			if(Input.GetKeyDown(KeyCode.Mouse0) && cooldownTimer <= 0)
			{
				//Only fire if there are bullets in the clip
				if(bulletCount > 0){
					Shoot();
				}
				//If the clip is empty, reload instead
				else 
				{
					Reload();
				}
			}

			//Reload if 'R' is pressed OR we tried to shoot while the clip is empty
			if(Input.GetKeyDown(KeyCode.R))
			{
				Reload();
			}
		}
		else {
			//Update the displayed bullet count
			bulletCountText.text = "--";
			//Decrement the reload timer if we are reloading
			reloadTimer -= Time.deltaTime;
		}

		//Decrement the cooldown timer
		cooldownTimer -= Time.deltaTime;
	}

	//Handle aiming our weapon
	void Aim()
	{
		//Tell the animator to pull the gun to our face
		animator.SetBool("Aim", true);
		//Disable the crosshairs
		foreach(Transform child in reticleParent.transform)
		{
			child.GetComponent<Image>().enabled = false;
		}
		//Flag ourselves as aiming
		isAiming = true;
		//Zoom the camera in
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 50, 10 * Time.deltaTime);
	}

	//Helper method used by other classes to tell ShootController to stop aiming.
	//Usually due to jumping, running, or wall-running.
	public void StopAiming()
	{
		//Tell the animator to pull the gun to the hip
		animator.SetBool("Aim", false);
		//Flag ourselves as not aiming
		isAiming = false;
		//Make sure reticle parent is NOT null
		if(reticleParent == null)
		{
			reticleParent = GameObject.FindGameObjectWithTag("Reticle");
		}
		//Enable the crosshairs
		foreach(Transform child in reticleParent.transform)
		{
			child.GetComponent<Image>().enabled = true;
		}
		//Zoom the camera out to normal
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, baseFOV, 10 * Time.deltaTime);
	}

	//Handle everything that needs to be done to fire a weapon
	void Shoot()
	{
		//Reset the shot timer
		cooldownTimer = cooldownTimerMax;

		//Decrement the bullet counter
		bulletCount--;

		//Shoot a Ray and find the closest thing we hit that isn't ourselves
		if(ac.totalOffset > 0)
		{
			print("wah");
		}
		Vector3 shootVector = ApplyAccuracy(playerCamera.transform.forward);
		Ray ray = new Ray(playerCamera.transform.position, shootVector);
		Vector3 hitPoint = Vector3.zero;
		Transform hitTransform = FindClosestHitInfo(ray, out hitPoint);

		//Play the recoil animation AFTER determing shoot vector
		animator.SetBool("Shoot", true);
		StartCoroutine(WaitForRecoilDone(0.08f));

		//Handle Recoil and Accuracy updates based on if we're aiming
		if(Input.GetKey(KeyCode.Mouse1))
		{
			rc.StartRecoil(aimRecoilAmount);
			ac.AddShootingOffset(isAiming);
		}
		else
		{
			rc.StartRecoil(hipRecoilAmount);
			ac.AddShootingOffset(isAiming);
		}

		//Check if we hit a red object
		bool hitRed = false;
		if(hitTransform != null && hitTransform.tag == "Red")
		{
			hitRed = true;
		}

		//Flag to show if we hit an enemy or not
		bool hitEnemy = false;

		//Make sure we actually hit something
		if(hitTransform != null)
		{
			//Determine if the object we hit has hit points
			Health h = hitTransform.GetComponent<Health>();
			//If we did not find an object with health
			if(h == null)
			{
				//Loop through it's parents and try to find one that has health
				while(hitTransform.parent && h == null)
				{
					hitTransform = hitTransform.parent;
					h = hitTransform.GetComponent<Health>();
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
					pv.RPC("TakeDamage", PhotonTargets.AllBuffered, weaponDamage, pView.owner.name);
				}
			} 

			//Show some bullet FX
			if(fxManager != null)
			{
				GunFX(hitPoint, hitEnemy, hitRed);
			}
		}
		//Hit nothing, show bull FX anyway
		else if(fxManager != null)
		{
			//Make the FX reach a certain distance from the camera
			hitPoint = playerCamera.transform.position + (playerCamera.transform.forward * 100f);
			GunFX(hitPoint, hitEnemy, hitRed);
		}
	}

	//Take a shooting vector and apply offsets based on recoil, weapon type
	private Vector3 ApplyAccuracy(Vector3 shootVector)
	{
		shootVector.x += WeightedRandomAccuracy(ac.totalOffset);
		shootVector.y += WeightedRandomAccuracy(ac.totalOffset);
		shootVector.z += WeightedRandomAccuracy(ac.totalOffset);
		//shootVector.x += Random.Range(-0.1F, 0.1F);
		//shootVector.y += Random.Range(-0.1F, 0.1F);
		//shootVector.z += Random.Range(-0.1F, 0.1F);
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
		//Start the reload timer
		reloadTimer = weaponReloadTime;
		//Reset the bullet count
		bulletCount = magazineCapacity;
		//Play a sound
		aSource.PlayOneShot(reloadClip);
		//Play the reload animation
		animator.SetBool("Reload", true);
	}

	//Helper function to show the gun FX
	void GunFX(Vector3 hitPoint, bool hitEnemy, bool hitRed)
	{
		//Grab the location of the gun and spawn the FX there
		WeaponData wd = this.transform.GetComponentInChildren<WeaponData>();
		fxManager.GetComponent<PhotonView>().RPC("BulletFX", PhotonTargets.All, wd.transform.position, hitPoint, hitEnemy, hitRed);
	}

	//Raycast in a line and find the closest object hit
	Transform FindClosestHitInfo(Ray ray, out Vector3 hitPoint)
	{
		Transform closestHit = null;
		float distance = 0f;
		hitPoint = Vector3.zero;

		//Get all objects that our raycast hit
		RaycastHit[] hits = Physics.RaycastAll(ray);

		//Loop through all the things we hit
		foreach(RaycastHit hit in hits)
		{
			//Find the closest object we hit that is not ourselves
			if(hit.transform != this.transform && (closestHit == null || hit.distance < distance))
			{
				//Update the closest hit and distance
				closestHit = hit.transform;
				distance = hit.distance;
				hitPoint = hit.point;
			}
		}

		return closestHit;
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
		animator.SetBool("Shoot", false);
	}
}
