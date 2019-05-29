using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : AbstractBehavior
{
    private int selectedWeapon = 1;
    private bool switchInProgress = false;
    
    [SerializeField] private Transform weaponHolder;

    void Start()
    {
        SelectWeapon();
        SetWeaponStyle();
    }

    void Update ()
    {
        bool weaponScrollUp = inputState.GetButtonPressed(inputs[0]) && inputState.GetButtonHoldTime(inputs[0]) == 0;
        bool weaponScrollDown = inputState.GetButtonPressed(inputs[1]) && inputState.GetButtonHoldTime(inputs[1]) == 0;
        //Don't allow switching weapons if we're already switching
        if (!switchInProgress)
        {
            if (weaponScrollUp)
            {
                //Increase the index of the selected weapon, wrapping around maximum back to 0
                if (selectedWeapon >= weaponHolder.childCount - 1)
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
                    selectedWeapon = weaponHolder.childCount - 1;
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
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }
                i++;
            }
        }
    }

    //When we wake, initialize the inputState's player weapon style
    private void SetWeaponStyle()
    {
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
                    if (weapon != null && wd != null)
                    {
                        inputState.playerWeaponStyle = wd.WeaponStyle;
                    }
                }
                i++;
            }
        }
    }
}