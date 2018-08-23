using System;
using UnityEngine;

[Serializable]
public class PlayerBodyData
{
    /*
    - Arms / Body enabled
    - FirstPersonController - Player Look 
	    - Neck
	    - First Spine
	    - Last Spine
    - ControlAnimations
	    - Body
	    - Weapon
	    - Weapon IK Anim
	    - LH Target
	    - RH Target
    */
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

    public Animator bodyAnimator;
    public Animator weaponAnim;
    public Animator weaponIKAnim;
}