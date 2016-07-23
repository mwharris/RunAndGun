using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour {

	public float speed = 3.0f;
	public Vector3 relativePosition;
	public Camera playerCamera;

	void Update () {
		//Rotate the gun with the camera
		transform.rotation = Quaternion.Slerp(transform.rotation, playerCamera.transform.rotation, Time.deltaTime * speed);
		//Translate the position of the gun with the camera
		transform.position = playerCamera.transform.TransformPoint(relativePosition);
	}
}
