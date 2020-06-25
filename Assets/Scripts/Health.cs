using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using Photon.Pun;

public class Health : MonoBehaviour {

	public float hitPoints = 100f;
	public AudioClip[] hurtSounds; 
	public GameObject deathCam;
	public Camera lobbyCam;

	private AudioSource aSource;
	private Image damageImage;
	private float currentHitPoints;
	private float flashSpeed = 2f;
	private FXManager fxManager;
	private GameObject deathOverlay;
	private Grayscale gScale;
	private float regenTimer = 5.0f;
	private float regenTimerMax = 5.0f;
	private float regenRate = 4.0f;
	private bool regenerating = false;
	private PhotonView pView;

	private GameObject damagedArrow;
	private RectTransform rectDamagedArrow;
	private Image imageDamagedArrow;
	private float damagedArrowTime = 0f;
	private float damagedArrowDelay = 1f;

	void Start () 
	{
		//Store the current hit points
		currentHitPoints = hitPoints;
		//Get a reference to an image that will appear whenever we are damaged
		damageImage = GameObject.FindGameObjectWithTag("DamageImage").GetComponent<Image>();
		//Grab a reference to the audio source for hurt sounds
		aSource = this.transform.GetComponent<AudioSource>();
		//Initialize a reference to the FXManager
		fxManager = GameObject.FindObjectOfType<FXManager>();
		//Get a reference to the death overlay
		deathOverlay = GameObject.FindGameObjectWithTag("DeathOverlay");
		//Get a reference to the grayscale effect for when we get hit
		gScale = this.transform.GetComponentInChildren<Grayscale>();
		//Get the attached PhotonView
		pView = GetComponent<PhotonView>();
		//Get the arrow for when we are damaged
		damagedArrow = GameObject.FindGameObjectWithTag("DamagedArrow");
		rectDamagedArrow = damagedArrow.GetComponent<RectTransform>();
		imageDamagedArrow = rectDamagedArrow.GetComponentInChildren<Image>();
	}

	void Update()
	{
		if(this.transform.GetComponent<PhotonView>().IsMine)
		{
			//Decrement the Regen timer
			regenTimer -= Time.deltaTime;
			//If we aren't regenerating
			if(regenTimer <= 0)
			{
				RegenHealth();
			}
			//Slowly make the screen red the more hurt we are
			Color newColor = new Color(1.0f, 0f, 0f, ((100 - currentHitPoints) / 100));
			damageImage.color = Color.Lerp(damageImage.color, newColor, flashSpeed * Time.deltaTime);
			//If we are close to death also apply grayscale
			gScale.effectAmount = ((100F - currentHitPoints) / 100F);
			//Fade out the arrow if the delay is over
			if(Time.time - damagedArrowTime > damagedArrowDelay)
			{
				HideHitAngle(false);
			}
		}
	}

	void RegenHealth()
	{
		//Mark us as regenerating
		regenerating = true;
		//Slowly start regenerating health over time
		if(currentHitPoints < hitPoints)
		{
			currentHitPoints += regenRate * Time.deltaTime;
		}
		//Make sure we don't go over max HP
		if(currentHitPoints > hitPoints)
		{
			currentHitPoints = hitPoints;
		}	
	}

	[PunRPC]
	public void TakeDamage(float damage, string enemyPhotonName, int enemyPhotonID, Vector3 shooterPosition, bool headshot) 
	{
        //Take the damage we received, doubled for headshot
        damage *= headshot ? 2 : 1;
        currentHitPoints -= damage;
        //If this is our local player
		if(GetComponent<PhotonView>().IsMine)
		{
			//Play a sound indicating we were shot
			PlayHurtSound();
			//Indicate where we were shot
			ShowHitAngle(shooterPosition);
		} 
		//Die if our HP is below 0
		if(currentHitPoints <= 0)
		{
			Die(enemyPhotonName, enemyPhotonID, headshot);
		}
		//If we didn't die, we're regenerating, and we were shot
		if(currentHitPoints > 0 && regenerating)
		{
			//Stop regenerating
			regenerating = false;
			//Restart the timer
			regenTimer = regenTimerMax;
		}
	}

	void ShowHitAngle(Vector3 shooterPosition)
	{
		//Calculate the direction between this player's position and the shooter's position
		Vector3 direction = transform.position - shooterPosition;
		//Rotate the arrow to point to where we were damaged
		Quaternion rotation = Quaternion.LookRotation(direction);
		float rot = Quaternion.Angle(rotation, transform.localRotation);
		if(Vector3.Dot(transform.right, direction) > 0f)
		{
			rot = -rot;
		}
		rectDamagedArrow.localRotation = Quaternion.Euler(0,0,rot);
		//Show the arrow
		Color currCol = imageDamagedArrow.color;
		currCol.a = 1;
		imageDamagedArrow.color = currCol;
		imageDamagedArrow.enabled = true;
		//Start the timer to hide the arrow
		damagedArrowTime = Time.time;
	}

	void HideHitAngle(bool immediate)
	{
		//Store the current color of the arrow
		Color currCol = imageDamagedArrow.color;
		//Immediately fade it out
		if(immediate)
		{
			currCol.a = 0;
		}
		//Slowly fade it out
		else
		{
			currCol.a = Mathf.Lerp(currCol.a, 0, 0.05f);
		}
		imageDamagedArrow.color = currCol;
		//Enable or disable the arrow when fading
		if (currCol.a == 0) 
		{
			imageDamagedArrow.enabled = false;
		}
		else 
		{
			imageDamagedArrow.enabled = true;
		}
	}

	void Die(string enemyPhotonName, int enemyPhotonID, bool headshot)
	{
		//If we did not instantiate this object over the network
		if (GetComponent<PhotonView>().InstantiationId == 0){
			//Simply destroy it in our scene
			Destroy(gameObject);
		} 
		//If we did instantiate it over the network
		else if(GetComponent<PhotonView>().IsMine){
			//Respawn this player if it is ours
			if(gameObject.tag == "Player"){
				//Reset the reticles before death
				GetComponent<AccuracyController>().ResetReticles();
				//Play effects on death
				Vector3 deathEffectPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
				fxManager.GetComponent<PhotonView>().RPC("DeathFX", RpcTarget.All, deathEffectPos);
				//Gray out the screen and display killer
				ShowDeathOverlay(enemyPhotonName);
				//Handle spawning a Death Camera
				HandleKillCam(enemyPhotonName, enemyPhotonID);
				//Make sure the hit angle is hidden
				HideHitAngle(true);
			}
			//Send out a notification this player was killed
			fxManager.GetComponent<PhotonView>().RPC("KillNotification", RpcTarget.All, pView.Owner.NickName, enemyPhotonName, headshot);
			//Delete it over the network
			PhotonNetwork.Destroy(gameObject);
		}
	}

	void HandleKillCam(string enemyPhotonName, int enemyPhotonId)
	{
		int currPos = 0;

		//Vectors for our raycasting
		Vector3 r = transform.right*5;
		Vector3 l = -transform.right*5;
		Vector3 f = transform.forward*5;
		Vector3 fr = Vector3.RotateTowards(transform.forward, transform.right, 45 * Mathf.Deg2Rad, 5)*5;
		Vector3 fl = Vector3.RotateTowards(transform.forward, -transform.right, 45 * Mathf.Deg2Rad, 5)*5;
		Vector3 b = -transform.forward*5;
		Vector3 br = Vector3.RotateTowards(-transform.forward, transform.right, 45 * Mathf.Deg2Rad, 5)*5;
		Vector3 bl = Vector3.RotateTowards(-transform.forward, -transform.right, 45 * Mathf.Deg2Rad, 5)*5;

		//Loop until we find a place suitable for the death camera
		bool force = false;
		while(true)
		{
			//Raycast in a direction we want to try and spawn the camera
			Vector3 camDirection;

			//Backwards
			if(currPos == 0){ camDirection = b; }
			//Back-right
			else if(currPos == 1){ camDirection = br; }
			//Back-left
			else if(currPos == 2){ camDirection = bl; }
			//Left
			else if(currPos == 3){ camDirection = l; }
			//Right
			else if(currPos == 4){ camDirection = r; }
			//Forwards
			else if(currPos == 5){ camDirection = f; }
			//Forward-right
			else if(currPos == 6){ camDirection = fr; }
			//Forward-left
			else if(currPos == 7){ camDirection = fl; }
			//End if we operated too long
			else { 
				camDirection = new Vector3(transform.position.x, transform.position.y - 5, transform.position.z);
				force = true;
			}
				
			//Add a height to the expected camera direction
			camDirection = new Vector3(camDirection.x, camDirection.y + 5, camDirection.z);

			//Create a ray and find the closest object we hit that isn't ourselves
			Ray ray = new Ray(transform.position, camDirection);
			HitPlayerInfo info = FindClosestHitInfo(ray);

			//If we hit nothing
			if(info.hitTransform == null || force)
			{
				//Make a Vector3 location and rotation for the death camera
				Vector3 dCamLoc = transform.position;
				if(!force)
				{
					dCamLoc = dCamLoc + camDirection;
				}
				//Spawn a death camera and set it active, pointing
				GameObject dCam = (GameObject) Instantiate(deathCam, dCamLoc, Quaternion.identity);
				dCam.SetActive(true);
				dCam.transform.LookAt(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z));
				//Attach a script to the camera
				DeathCamScript deathCamScript = dCam.AddComponent<DeathCamScript>();
                deathCamScript.targetId = enemyPhotonId;
				deathCamScript.lobbyCamera = lobbyCam;
				//Exit loop
				break;
			}
			else
			{
				currPos++;
			}
		}
	}

    private HitPlayerInfo FindClosestHitInfo(Ray ray)
    {
        HitPlayerInfo info = new HitPlayerInfo();
		//Get all objects that our raycast hit
		RaycastHit[] hits = Physics.RaycastAll(ray);
		//Loop through all the things we hit
		foreach(RaycastHit hit in hits)
		{
			//Find the closest object we hit that is not ourselves
			if(hit.transform != this.transform && (info.hitTransform == null || hit.distance < info.distance))
			{
                //Update the closest hit and distance
                info.hitTransform = hit.transform;
                info.hitPoint = hit.point;
                info.distance = hit.distance;
                info.headshot = hit.collider.GetType() == typeof(BoxCollider);
			}
		}
		return info;
    }
    
	void PlayHurtSound()
	{
		AudioClip clipToPlay;

		//Pick & play a random footstep sound from the array,
		int n = Random.Range(1, hurtSounds.Length);
		clipToPlay = hurtSounds[n];
		aSource.PlayOneShot(clipToPlay);

		//Move picked sound to index 0 so it's not picked next time
		hurtSounds[n] = hurtSounds[0];
		hurtSounds[0] = clipToPlay;
	}

	void ShowDeathOverlay(string enemyName)
	{
		//Unhide death overlay components
		deathOverlay.transform.GetChild(0).GetComponent<Text>().enabled = true;
		deathOverlay.transform.GetChild(1).GetComponent<Text>().enabled = true;
		//Update the killer's name text
		deathOverlay.transform.GetChild(1).GetComponent<Text>().text = enemyName;
	}
	
    /*
	//Suicide button for testing respawn
	void OnGUI()
	{
		if(this.transform.GetComponent<PhotonView>().isMine && gameObject.tag == "Player")
		{
			if(GUI.Button(new Rect(Screen.width-100, 0, 100, 40), "Suicide!"))
			{
				Die(this.gameObject.name);
			}
			if(GUI.Button(new Rect(Screen.width-200, 0, 100, 40), "Damage!"))
			{
				TakeDamage(25, "", new Vector3(0,0,0)); 
			}
			if(regenerating && hitPoints > currentHitPoints)
			{
				if(GUI.Button(new Rect(Screen.width-300, 0, 100, 40), "REGEN!"))
				{
					//
				}
			}	
		}
	}
    */
}