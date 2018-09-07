using UnityEngine;
using System.Collections;

public class RecoilController : MonoBehaviour 
{	
	public float maxRecoil_x;
	public float recoilSpeed;
	public float recoil;
	public float currentRecoil;

    private Transform recoilMod;

    private void Start()
    {
        recoilMod = GetComponent<BodyController>().PlayerBodyData.playerCamera.parent;
    }

    void Update () {
		//Handle the recoiling
		Recoiling();
	}

	public void StartRecoil(float recoilAmount)
	{
		//Add to the recoil amount
		recoil += recoilAmount;
	}

	private void Recoiling()
	{
		if(recoil > 0)
		{
			//Create a Quaternion target representing our max recoil rotation
			Quaternion maxRecoil = Quaternion.Euler(-maxRecoil_x, 0, 0);
			//Slerp towards the target rotation
			recoilMod.localRotation = Quaternion.Slerp(recoilMod.localRotation, maxRecoil, Time.deltaTime * recoilSpeed);
			//Decrease the recoil slowly
			recoil -= Time.deltaTime;
		}
		else
		{
			recoil = 0;
			//Slerp towards the above Quaternion
			recoilMod.localRotation = Quaternion.Slerp(recoilMod.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
		}
		currentRecoil = recoilMod.localRotation.x;
	}
}
