using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class WeaponSwitcher : AbstractBehavior
{
    private int _selectedWeapon;
    private bool _switchInProgress;

    [SerializeField] private BodyController bodyController;
    [SerializeField] private Transform weaponHolder;

    private RPCManager _rpcManager;
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
        _rpcManager = FindObjectOfType<RPCManager>();
        if (_rpcManager != null)
        {
            rpcPView = _rpcManager.GetComponent<PhotonView>();
        }
    }
    
    void Update()
    {
        int origSelectedWeapon = _selectedWeapon;
        int weaponCount = weaponHolder.childCount;
        bool weaponScrollUp = inputState.GetButtonPressed(inputs[0]);
        bool weaponScrollDown = inputState.GetButtonPressed(inputs[1]);
        bool weapon1Select = inputState.GetButtonPressed(inputs[2]);
        bool weapon2Select = inputState.GetButtonPressed(inputs[3]);

        //Don't allow switching weapons if we're already switching
        if (weaponCount > 1 && !_switchInProgress && !inputState.playerIsReloading)
        {
            if (weaponScrollUp)
            {
                //Increase the index of the selected weapon, wrapping around maximum back to 0
                if (_selectedWeapon >= weaponCount - 1)
                {
                    _selectedWeapon = 0;
                }
                else
                {
                    _selectedWeapon++;
                }
                _switchInProgress = true;
            }
            else if (weaponScrollDown)
            {
                //Decrease the index of the selected weapon, wrapping around 0 back to maximum
                if (_selectedWeapon <= 0)
                {
                    _selectedWeapon = weaponCount - 1;
                }
                else
                {
                    _selectedWeapon--;
                }
                _switchInProgress = true;
            }
            else if (weapon1Select && _selectedWeapon != 0)
            {
                _selectedWeapon = 0;
                _switchInProgress = true;
            }
            else if (weapon2Select && _selectedWeapon != 1)
            {
                _selectedWeapon = 1;
                _switchInProgress = true;
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
                if (i == _selectedWeapon)
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
                if (i == _selectedWeapon)
                {
                    WeaponData wd = weapon.GetComponent<WeaponData>();
                    ItemInfo wdInfo = weapon.GetComponent<Item>().info;
                    if (weapon != null && wd != null)
                    {
                        //TODO: Send an RPC to update our other instances
                        // rpcPView.RPC("PlayerWeaponChange", PhotonTargets.AllBuffered, playerPView.owner.ID, wdInfo.itemId);
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
        _selectedWeapon = weaponIndex;
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
        _switchInProgress = false;
    }
}