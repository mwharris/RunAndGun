using System;
using UnityEngine;

[Serializable]
public class PlayerBodyData
{
    public Transform parentGameObject;
    public Transform playerCamera;
    public Transform body;
    public Transform neck;
    public Transform firstSpine;
    public Transform lastSpine;
    public Transform weapon;
    public Transform weaponHolder;
    public Transform leftHandIKTarget;
    public Transform leftHandTarget;
    public Transform rightHandIKTarget;
    public Transform rightHandTarget;
    public Transform rightShoulder;
    public Transform rightShoulderAxis;

    public Animator bodyAnimator;
    public Animator weaponAnim;
    public Animator weaponIKAnim;
}