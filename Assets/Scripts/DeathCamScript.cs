using UnityEngine;
using System.Collections;

public class DeathCamScript : MonoBehaviour {

	public float timer = 0.0f;
	public string targetName;
	public GameObject target;

	void Start() 
	{
		//Initialize the timer
		timer = 1.5f;
		//Find the target we are trying to look at
		target = FindTarget(targetName);
	}

	void Update () 
	{
		if(target != null)
		{
			//Decrement the timer
			timer -= Time.deltaTime;
			//Check if time is up
			if(timer <= 0)
			{
				//Get the vector of the direction to the killer
				Vector3 dirVector = target.transform.position - this.transform.position;
				//Get the quaternion
				Quaternion quat = Quaternion.LookRotation(dirVector);
				//Rotate towards the killer
				//transform.rotation = quat;
				transform.rotation = Quaternion.Lerp(transform.rotation, quat, 0.2f);
			}
		}
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
