using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathCamScript : MonoBehaviour {

	public float lookTimer = 0.0f;
	public float lifeTimer = 0.0f;
	public string targetName;
	public GameObject target;

	private GameObject deathOverlay;

	void Start() 
	{
		//Initialize the look timer
		lookTimer = 0.5f;
		//Initialize the death timer
		lifeTimer = 3.0f;
		//Get a reference to the death overlay
		deathOverlay = GameObject.FindGameObjectWithTag("DeathOverlay");
		//Find the target we are trying to look at
		target = FindTarget(targetName);
	}

	void Update () 
	{
		//Decrement the timers
		lookTimer -= Time.deltaTime;
		lifeTimer -= Time.deltaTime;
		//Check if we should start looking at the killer
		if(lookTimer <= 0 && target != null)
		{
			//Get the vector of the direction to the killer
			Vector3 dirVector = target.transform.position - this.transform.position;
			//Get the quaternion
			Quaternion quat = Quaternion.LookRotation(dirVector);
			//Rotate towards the killer
			transform.rotation = Quaternion.Lerp(transform.rotation, quat, 0.2f);
		}
		//Check if we should turn this camera off
		if(lifeTimer <= 0)
		{
			//Hide the gray screen and show the player's camera
			HideDeathOverlay();
			//Destroy this camera
			Destroy(this.gameObject);
		}
	}

	void HideDeathOverlay()
	{
		//Hide death overlay components
		deathOverlay.transform.GetChild(0).GetComponent<Image>().enabled = false;
		deathOverlay.transform.GetChild(1).GetComponent<Text>().enabled = false;
		deathOverlay.transform.GetChild(2).GetComponent<Text>().enabled = false;
	}

	GameObject FindTarget(string targetName)
	{
		//Get the full list of players
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		//Loop through each
		foreach(GameObject player in players)
		{
			//Return this player object if it's the player we're looking for
			if(player.GetComponent<PhotonView>().owner.name == targetName)
			{
				return player;
			}
		}
		//Player not found
		return null;
	}
}
