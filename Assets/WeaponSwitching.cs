using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : AbstractBehavior
{
    private int selectedWeapon = 0;

    //TODO: Interact with the Player Animator to switch to DH animations
    //TODO: Weapon switch animations
    //TODO: Weapon switch delay?

    void Start()
    {
        SetWeaponStyle();
    }

    void Update ()
    {
        //Mouse srolled up
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            //Increase the index of the selected weapon, wrapping around maximum back to 0
            if (selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
        }
        //Mouse scrolled down
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            //Decrease the index of the selected weapon, wrapping around 0 back to maximum
            if (selectedWeapon <= 0)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon--;
            }
        }
        //Make the selected weapon active
        SelectWeapon();
    }

    //Set the selected weapon to Active, other weapons to Inactive.
    //Update the weapon style of the newly equipped weapon in the inputState.
    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
                inputState.playerWeaponStyle = weapon.GetComponent<WeaponData>().WeaponStyle;
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }

    //When we wake, initialize the inputState's player weapon style
    void SetWeaponStyle()
    {
        foreach (Transform weapon in transform)
        {
            if (weapon.gameObject.activeSelf)
            {
                inputState.playerWeaponStyle = weapon.GetComponent<WeaponData>().WeaponStyle;
            }
        }
    }
}