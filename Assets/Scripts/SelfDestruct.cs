using Photon.Pun;
using UnityEngine;

public class SelfDestruct : MonoBehaviour {

	public float selfDestructTime = 1.0f;

	void Update () 
	{
		//Decrement the timer every frame
		selfDestructTime -= Time.deltaTime;
		//Destroy this game object after the timer is up
		if(selfDestructTime <= 0)
		{
			//Check if we were instantiated on the network or not
			PhotonView pv = GetComponent<PhotonView>();
			if(pv != null && pv.InstantiationId != 0){
				PhotonNetwork.Destroy(gameObject);
			} else {
				Destroy(gameObject);
			}
		}
	}
}
