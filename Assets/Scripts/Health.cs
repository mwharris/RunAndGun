using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour {

	public float hitPoints = 100f;
	public AudioClip[] hurtSounds; 
	public GameObject deathCam;

	private AudioSource aSource;
	private Image damageImage;
	private float currentHitPoints;
	private float respawnTimer = 3f;
	private float flashSpeed = 5f;
	private Color flashColor = new Color(1.0f, 0f, 0f, 0.2f);
	private FXManager fxManager;

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
	}

	void Update()
	{
		damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
	}

	[PunRPC]
	public void TakeDamage(float damage, string enemyPhotonName) 
	{
		currentHitPoints -= damage;

		//If this is our local player
		if(this.transform.GetComponent<PhotonView>().isMine){
			//Display a damage image
			damageImage.color = flashColor;
			//Play a sound indicating we were shot
			PlayHurtSound();
		} 

		if(currentHitPoints <= 0)
		{
			Die(enemyPhotonName);
		}
	}

	void Die(string enemyPhotonName)
	{
		//If we did not instantiate this object over the network
		if(this.transform.GetComponent<PhotonView>().instantiationId == 0){
			//Simply destroy it in our scene
			Destroy(this.gameObject);
		} 
		//If we did instantiate it over the network
		else if(this.transform.GetComponent<PhotonView>().isMine){
			//Respawn this player if it is ours
			if(gameObject.tag == "Player"){
				//Play effects on death
				Vector3 deathEffectPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
				fxManager.GetComponent<PhotonView>().RPC("DeathFX", PhotonTargets.All, deathEffectPos);
				//Get a reference to our NetworkManager in order to manipulate variables
				NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();
				//Handle spawning a Death Camera
				HandleKillCam(enemyPhotonName);
				//Enable the lobby camera so we don't get a blank screen
				//nm.lobbyCamera.gameObject.SetActive(true);
				//GameObject.FindGameObjectWithTag("Reticle").GetComponent<Image>().enabled = false;
				//Start our respawn timer
				nm.respawnTimer = respawnTimer;
			}
			//Delete it over the network
			PhotonNetwork.Destroy(this.gameObject);
		}
	}

	void HandleKillCam(string enemyPhotonName)
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
			else { break; }
				
			//Add a height to the expected camera direction
			camDirection = new Vector3(camDirection.x, camDirection.y + 5, camDirection.z);

			//Create a ray and find the closest object we hit that isn't ourselves
			Ray ray = new Ray(transform.position, camDirection);
			Vector3 hitPoints = Vector3.zero;
			Transform hitTransform = FindClosestHitInfo(ray, out hitPoints);

			//If we hit nothing
			if(hitTransform == null)
			{
				//Make a Vector3 location and rotation for the death camera
				Vector3 dCamLoc = transform.position;
				dCamLoc = dCamLoc + camDirection;
				//Spawn a death camera and set it active, pointing
				GameObject dCam = (GameObject) Instantiate(deathCam, dCamLoc, Quaternion.identity);
				dCam.SetActive(true);
				dCam.transform.LookAt(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z));
				//Attach a script to the camera
				DeathCamScript deathCamScript = dCam.AddComponent<DeathCamScript>();
				deathCamScript.targetName = enemyPhotonName;
				//Exit loop
				break;
			}
			else
			{
				currPos++;
			}
		}
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
		
	//Suicide button for testing respawn
	void OnGUI()
	{
		if(this.transform.GetComponent<PhotonView>().isMine && gameObject.tag == "Player")
		{
			if(GUI.Button(new Rect(Screen.width-100, 0, 100, 40), "Suicide!"))
			{
				Die(this.gameObject.name);
			}
		}
	}
}
