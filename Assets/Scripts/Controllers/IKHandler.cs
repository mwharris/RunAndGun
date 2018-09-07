using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandler : MonoBehaviour
{
    public Transform playerCamera;
    public Transform weaponHolder;
    public Transform overrideLookTarget;
    public Transform rightShoulder;
    public InputState inputState;

    public Transform rightHandIKTarget;
    private float rightHandIKWeight = 1;
    public Transform leftHandIKTarget;
    private float leftHandIKWeight = 1;

    private PhotonView photonView;

    private Animator anim;
    private Transform aimHelper;
    private Vector3 lookAtPosition;
    private bool isAiming;
    private bool isReloading;
    private float targetWeight = 0f;
    private float lookWeight = 0f;

    private Vector3 rightHandIKPositionOverride = Vector3.zero;
    private Vector3 leftHandIKPositionOverride = Vector3.zero;
    private Quaternion rightHandIKRotationOverride = Quaternion.identity;
    private Quaternion leftHandIKRotationOverride = Quaternion.identity;

    private bool weaponHolderPosSet = false;

    void Start ()
    {
        aimHelper = new GameObject().transform;
        anim = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();
    }

    //Helper function to set isAiming and isReloading variables based on player inputs
    void setInputVars()
    {
        isAiming = inputState.playerIsAiming;
        isReloading = false;
    }

    void LateUpdate ()
    {
        //Make sure our Right Shoulder transform is connected to the right shoulder bone
        if (rightShoulder == null)
        {
            rightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        }
        //If we already have Right Shoulder position then move our Weapon Holder there
        else 
        {
            weaponHolder.position = rightShoulder.position;
        }

        //Set variables based on inputs
        setInputVars();

        //Set up our lookAt position
        lookAtPosition = playerCamera.position + (playerCamera.forward * 2);

        //Determine the direction between our position and where we are looking (aim helper position)
        Vector3 dirTowardsTarget = aimHelper.position - transform.position;

        //The speed at which our IKs will move...do we want different speeds for aiming?
        float multiplier = 30f;

        //Lerp between our current look position and target look position
        targetWeight = 1;
        lookWeight = Mathf.Lerp(lookWeight, targetWeight, Time.deltaTime * multiplier);

        //Store our new look weight as our IK weights
        //Debug.Log("Override Left: " + anim.GetFloat("IKWeightOverrideLeft") + ", Override Right: " + anim.GetFloat("IKWeightOverrideRight"));
        rightHandIKWeight = lookWeight - anim.GetFloat("IKWeightOverrideRight");
        leftHandIKWeight = lookWeight - anim.GetFloat("IKWeightOverrideLeft");

        //Handle the actual rotations of our IKs
        HandleShoulderRotation();
    }

    //Handle the actual rotations of our IKs and GameObjects
    void HandleShoulderRotation()
    {
        if (isAiming)
        {
            //Move the aim helper to our new look position
            aimHelper.position = lookAtPosition;
            //Rotate our weapon holder to point at this new position
            LookAt(weaponHolder, aimHelper);
            //Update the right hand's parent to point at this new position as well
            LookAt(rightHandIKTarget.parent.transform, aimHelper);
        }
        else
        {
            //Move the aim helper to our new look position
            float aimHelperLerpSpeed = 30f;
            aimHelper.position = Vector3.Lerp(aimHelper.position, lookAtPosition, Time.deltaTime * aimHelperLerpSpeed);
            //Rotate our weapon holder to point at this new position
            LerpLookAt(weaponHolder, aimHelper);
            //Update the right hand's parent to point at this new position as well
            LerpLookAt(rightHandIKTarget.parent.transform, aimHelper);
        }
    }

    void LookAt(Transform a, Transform b)
    {
        a.rotation = Quaternion.LookRotation(b.position - a.position);
    }

    //Helper function to perform lookAt except lerped
    void LerpLookAt(Transform a, Transform b)
    {
        //Determine our lerp speed depending on if we're aiming or not
        float lerpSpeed = 30f;
        //Get a quaternion from the vector between the two transforms
        Quaternion targetRotation = Quaternion.LookRotation(b.position - a.position);
        //Lerp the current rotation to the rotation determined above
        a.rotation = Quaternion.Lerp(a.rotation, targetRotation, Time.deltaTime * lerpSpeed);
    }

    //Set the both hand's IK position and rotations based on calculations above
    void OnAnimatorIK(int layerIndex)
    {
        //Head IK
        anim.SetLookAtWeight(lookWeight, 0f, 1f, 1f, 0f);
        anim.SetLookAtPosition(lookAtPosition);
        //Hand IK - Position
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
        //Hand IK - Rotation
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
    }
}
