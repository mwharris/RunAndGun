using UnityEngine;
using System.Collections;

public class RecoilController : MonoBehaviour 
{	
	public float maxRecoil_x;
	public float recoil;
	public float currentRecoil;

    private float recoilSpeed = 4f;

    private Transform recoilMod;
    private BodyController bodyControl;
    private PlayerBodyData playerBodyData;
    private WeaponData currWeaponData;

    private void Start()
    {
        bodyControl = GetComponent<BodyController>();
        SetVars();
    }

    private void SetVars()
    {
        playerBodyData = bodyControl.PlayerBodyData;
        recoilMod = playerBodyData.playerCamera.parent;
        currWeaponData = playerBodyData.GetWeaponData();
        recoilSpeed = currWeaponData.RecoilSpeed;
    }

    void Update ()
    {
        SetVars();
        HandleRecoil();
	}

    //Add a value to the recoil amount
    public void StartRecoil(float recoilAmount)
	{
		recoil += recoilAmount;
	}

    //Slerp the Recoil Transform depending on the recoil amount
	private void HandleRecoil()
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
