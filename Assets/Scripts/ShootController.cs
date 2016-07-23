using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//public class ShootController : NetworkBehaviour
public class ShootController : MonoBehaviour 
{

	public GameObject gun;
	public Animator recoilAnim;
	public Camera playerCamera;
	public AudioSource aSource;

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
	private FXManager fxManager;
	private float cooldownTimer;
	private PhotonView pView;

	void Start()
	{
		cooldownTimer = 20.0f;
		hitIndicatorTimerMax = 0.3f;
		hitIndicatorTimer = hitIndicatorTimerMax;

		//Initialize a reference to the FXManager
		fxManager = GameObject.FindObjectOfType<FXManager>();

		//Initialize a reference to the Hit Indicators
		hitIndicator = GameObject.FindGameObjectWithTag("HitIndicator");

		//Initialize a reference to the bullet count
		GameObject txt = GameObject.FindGameObjectWithTag("BulletCount");
		bulletCountText = txt.GetComponent<Text>();
		//...and max bullet capacity
		txt = GameObject.FindGameObjectWithTag("ClipSize");
		clipSizeText = txt.GetComponent<Text>();
		clipSizeText.text = magazineCapacity.ToString();

		//Default the bullet count to the max magazin capacity
		bulletCount = magazineCapacity;
	}

	void Update() 
	{
		//Reset the recoil animation
		if(recoilAnim.GetCurrentAnimatorStateInfo(0).IsName("Recoil"))
		{
			recoilAnim.SetBool("Shoot", false);
		}

		//Hide the hit indicators from last frame
		HideHitIndicator();

		/*
		//TEST CODE FOR GETTING SHOT
		if(Input.GetButtonDown("Fire2"))
		{
			CmdHitPlayer(this.gameObject);
		}
		*/

		//Make sure we aren't reloading
		if(reloadTimer <= 0) {
			//Update the displayed bullet count
			bulletCountText.text = bulletCount.ToString();

			//Determine if we fired (left mouse)
			if(Input.GetKeyDown(KeyCode.Mouse0) && cooldownTimer <= 0)
			{
				//Only fire if there are bullets in the clip
				if(bulletCount > 0){
					//Reset the shot timer
					cooldownTimer = 20.0f;

					//Decrement the bullet counter
					bulletCount--;

					//Play the recoil animation
					recoilAnim.SetBool("Shoot", true);

					//Shoot a Ray and find the closest thing we hit that isn't ourselves
					Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
					Vector3 hitPoint = Vector3.zero;
					Transform hitTransform = FindClosestHitInfo(ray, out hitPoint);

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
								pv.RPC("TakeDamage", PhotonTargets.AllBuffered, weaponDamage);
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
		cooldownTimer--;
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
		//TODO: Play an animation
	}

	//Helper function to show the gun FX
	void GunFX(Vector3 hitPoint, bool hitEnemy, bool hitRed)
	{
		//Grab the location of the gun and spawn the FX there
		WeaponData wd = this.transform.GetComponentInChildren<WeaponData>();
		fxManager.GetComponent<PhotonView>().RPC("BulletFX", PhotonTargets.All, wd.transform.position, hitPoint, hitEnemy, hitRed);
	}

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

	void ShowHitIndicator()
	{
		hitIndicatorTimer = hitIndicatorTimerMax;

		//Show them all
		foreach (Transform child in hitIndicator.transform)
		{
			child.gameObject.SetActive(true);
		}
	}


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
}
