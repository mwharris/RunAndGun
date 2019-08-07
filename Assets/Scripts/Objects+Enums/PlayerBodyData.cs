using System;
using UnityEngine;

[Serializable]
public class PlayerBodyData
{
    public Transform playerCamera;
    public Transform body;
    public Transform neck;
    public Transform weapon;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    public Animator GetWeaponAnimator()
    {
        if (weapon != null)
        {
            return weapon.GetComponent<Animator>();
        }
        return null;
    }

    public WeaponData GetWeaponData()
    {
        if (weapon != null)
        {
            return weapon.GetComponent<WeaponData>();
        }
        return null;
    }

    public Animator GetBodyAnimator()
    {
        if (body != null)
        {
            return body.GetComponent<Animator>();
        }
        return null;
    }
}