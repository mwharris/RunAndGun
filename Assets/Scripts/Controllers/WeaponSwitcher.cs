using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : AbstractBehavior
{
    private int selectedWeapon = 0;
    private bool switchInProgress = false;

    [SerializeField] private BodyController bodyController;
    [SerializeField] private Transform weaponHolder;

    private RPCManager rpcManager;
    private PhotonView rpcPView;
    private PhotonView playerPView;

    void Start()
    {
        GetPhotonViews();
        //Equip the first weapon that is attached
        SelectWeapon();
        //Initialize the inputState's player weapon style
        if (weaponHolder != null)
        {
            foreach (Transform weapon in weaponHolder)
            {
                if (weapon.gameObject.activeSelf)
                {
                    inputState.playerWeaponStyle = weapon.GetComponent<WeaponData>().WeaponStyle;
                }
            }
        }
    }

    private void GetPhotonViews()
    {
        //Get a reference to this player's photon view
        playerPView = bodyController.gameObject.GetComponent<PhotonView>();
        //Get a reference to the RPCManager's photon view for weapon swapping RPCs
        rpcManager = FindObjectOfType<RPCManager>();
        if (rpcManager != null)
        {
            rpcPView = rpcManager.GetComponent<PhotonView>();
        }
    }

    void Update()
    {
        int origSelectedWeapon = selectedWeapon;
        int weaponCount = weaponHolder.childCount;
        bool weaponScrollUp = inputState.GetButtonPressed(inputs[0]);
        bool weaponScrollDown = inputState.GetButtonPressed(inputs[1]);

        //Don't allow switching weapons if we're already switching
        if (weaponCount > 1 && !switchInProgress)
        {
            if (weaponScrollUp)
            {
                //Increase the index of the selected weapon, wrapping around maximum back to 0
                if (selectedWeapon >= weaponCount - 1)
                {
                    selectedWeapon = 0;
                }
                else
                {
                    selectedWeapon++;
                }
                switchInProgress = true;
            }
            if (weaponScrollDown)
            {
                //Decrease the index of the selected weapon, wrapping around 0 back to maximum
                if (selectedWeapon <= 0)
                {
                    selectedWeapon = weaponCount - 1;
                }
                else
                {
                    selectedWeapon--;
                }
                switchInProgress = true;
            }
            //Store the weapon we switch to if we are switching
            SwitchWeaponStyle();
        }
    }

    //Set the selected weapon to Active, other weapons to Inactive.
    //Update the weapon style of the newly equipped weapon in the inputState.
    private void SelectWeapon()
    {
        if (weaponHolder != null)
        {
            int i = 0;
            foreach (Transform weapon in weaponHolder)
            {
                if (i == selectedWeapon)
                {
                    weapon.gameObject.SetActive(true);
                    bodyController.PlayerBodyData.weapon = weapon;
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }
                i++;
            }
        }
    }

    //Determine and store the weapon style that we are switching to for the animation event callback
    private void SwitchWeaponStyle()
    {
        if (weaponHolder != null)
        {
            int i = 0;
            foreach (Transform weapon in weaponHolder)
            {
                if (i == selectedWeapon)
                {
                    WeaponData wd = weapon.GetComponent<WeaponData>();
                    ItemInfo wdInfo = weapon.GetComponent<Item>().info;
                    if (weapon != null && wd != null)
                    {
                        //TODO: Send an RPC to update our other instances
                        //rpcPView.RPC("PlayerWeaponChange", PhotonTargets.AllBuffered, playerPView.owner.ID, wdInfo.itemId);
                        inputState.playerWeaponStyle = wd.WeaponStyle;
                    }
                }
                i++;
            }
        }
    }

    //Switch to the weapon at the passed in index
    public void SwitchWeaponTo(int weaponIndex)
    {
        //Select the waepon at the passed in index
        selectedWeapon = weaponIndex;
        //Make sure we update our weapon style
        SwitchWeaponStyle();
    }

    //Handle activating/deactivating switched weapon game objects.
    //Called by the Animator after the hands lower during a weapon switch animation.
    public void SwitchWeaponEvent()
    {
        SelectWeapon();
    }

    //When the animation is done, reset our weapon switch flag
    public void WeaponSwitchDoneEvent()
    {
        switchInProgress = false;
    }
}