using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private Camera sceneCamera;
	/*
	void Start () {
		//Find the lobby camera
		for(int i = 0; i < Camera.allCamerasCount; i++){
			if(Camera.allCameras[i].name == "Lobby"){
				sceneCamera = Camera.allCameras[i];
			}
		}
		sceneCamera.enabled = false;
		this.GetComponent<Camera>().enabled = true;
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.C)){
			this.GetComponent<Camera>().enabled = !this.GetComponent<Camera>().enabled;
			sceneCamera.enabled = !sceneCamera.enabled;
		}
	}

	void OnDestroy(){
		this.GetComponent<Camera>().enabled = false;
		sceneCamera.enabled = true;
	}
	*/
}
