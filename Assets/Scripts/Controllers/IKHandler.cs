using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandler : MonoBehaviour
{
    public Transform playerCamera;
    public Transform weaponHolder;
    public Transform overrideLookTarget;
    public Transform rightShoulder;

    public Transform rightHandIKTarget;
    public float rightHandIKWeight = 1;
    public Transform leftHandIKTarget;
    public float leftHandIKWeight = 1;

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

    void Start ()
    {
        aimHelper = new GameObject().transform;
        anim = GetComponent<Animator>();
        photonView = transform.parent.GetComponent<PhotonView>();
    }

    //Helper function to set isAiming and isReloading variables based on player inputs
    void setInputVars()
    {
        isAiming = Input.GetKey(KeyCode.Mouse1);
        isReloading = false;
    }

    void LateUpdate ()
    {
        //IK is controller by network when player is not ours
        if (photonView.isMine)
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
            lookAtPosition = playerCamera.position + (playerCamera.forward * 10);

            //Determine the direction between our position and where we are looking (aim helper position)
            Vector3 dirTowardsTarget = aimHelper.position - transform.position;

            //Find the angle between where we are currently looking and where we want to look.
            //REMOVE THIS?...this may not apply in our case since we are a FPS are are technically constantly aiming.
            float angle = Vector3.Angle(transform.forward, dirTowardsTarget);
            if (angle < 90)
            {
                targetWeight = 1;
            }
            else
            {
                targetWeight = 0;
            }

            //The speed at which our IKs will move...do we want different speeds for aiming?
            float multiplier = isAiming ? 5f : 30f;

            //Lerp between our current look position and target look position
            lookWeight = Mathf.Lerp(lookWeight, targetWeight, Time.deltaTime * multiplier);

            //Store our new look weight as our IK weights
            rightHandIKWeight = lookWeight - anim.GetFloat("IKWeightOverride");
            leftHandIKWeight = lookWeight - anim.GetFloat("IKWeightOverride");

            //Handle the actual rotations of our IKs
            HandleShoulderRotation();
        }
    }

    //Handle the actual rotations of our IKs and GameObjects
    void HandleShoulderRotation()
    {
        //Move the aim helper to our new look position
        aimHelper.position = Vector3.Lerp(aimHelper.position, lookAtPosition, Time.deltaTime * 10f);
        //Rotate our weapon holder to point at this new position
        LerpLookAt(weaponHolder, aimHelper);
        //Update the right hand's parent to point at this new position as well
        LerpLookAt(rightHandIKTarget.parent.transform, aimHelper);
    }

    //Helper function to perform lookAt except lerped
    void LerpLookAt(Transform a, Transform b)
    {
        //Determine our lerp speed depending on if we're aiming or not
        float lerpSpeed = isAiming ? 20f : 15f; 
        //Get a quaternion from the vector between the two transforms
        Quaternion targetRotation = Quaternion.LookRotation(b.position - a.position);
        //Lerp the current rotation to the rotation determined above
        a.rotation = Quaternion.Lerp(a.rotation, targetRotation, Time.deltaTime * lerpSpeed);
    }

    //Set the both hand's IK position and rotations based on calculations above
    void OnAnimatorIK(int layerIndex)
    {
        //If we're controlling our player then set calculated position/rotation values
        if (photonView.isMine)
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
        //If we're controlling a network character, override with networked values
        else
        {
            //Hand IK - Position
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKPositionOverride);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKPositionOverride);
            //Hand IK - Rotation
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKRotationOverride);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKRotationOverride);
        }
    }

    public void setNetworkedIKValues(Vector3 rightHandPos, Vector3 leftHandPos, Quaternion rightHandRot, Quaternion leftHandRot)
    {
        rightHandIKPositionOverride = rightHandPos;
        leftHandIKPositionOverride = leftHandPos;
        rightHandIKRotationOverride = rightHandRot;
        leftHandIKRotationOverride = leftHandRot;
    }
}
