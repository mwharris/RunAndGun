using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkCameraController : NetworkBehaviour {

	void Start () {
		NetworkView nView = GetComponent<NetworkView>();
		if(nView.isMine) {
			GetComponent<Camera>().enabled = true;
		}
		else {
			GetComponent<Camera>().enabled = false;
		}
	}
}
