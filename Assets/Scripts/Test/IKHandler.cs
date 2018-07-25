using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandler : MonoBehaviour
{
    Animator anim;

    public Transform playerCamera;

    public Transform weaponHolder;
    public Transform overrideLookTarget;
    public Transform rightShoulder;

    public Transform rightHandIKTarget;
    public float rightHandIKWeight = 1;

    public Transform leftHandIKTarget;
    public float leftHandIKWeight = 1;

    Transform aimHelper;

    bool isAiming;
    bool isReloading;

    float targetWeight = 0f;
    float lookWeight = 0f;

    void Start ()
    {
        aimHelper = new GameObject().transform;
        anim = GetComponent<Animator>();
    }
	
	void FixedUpdate ()
    {
        if (rightShoulder == null)
        {
            rightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        }
        else
        {
            weaponHolder.position = rightShoulder.position;
        }

        setInputVars();

        Vector3 dirTowardsTarget = aimHelper.position - transform.position;
        float angle = Vector3.Angle(transform.forward, dirTowardsTarget);

        if (angle < 90)
        {
            targetWeight = 1;
        }
        else
        {
            targetWeight = 0;
        }

        float multiplier = 50f;// isAiming ? 30f : 30f;

        lookWeight = Mathf.Lerp(lookWeight, targetWeight, Time.deltaTime * multiplier);

        rightHandIKWeight = lookWeight;

        leftHandIKWeight = lookWeight;// - anim.GetFloat("LeftHandIKWeightOverride");

        HandleShoulderRotation();
    }

    void HandleShoulderRotation()
    {
        aimHelper.position = Vector3.Lerp(aimHelper.position, getLookPosition(playerCamera), Time.deltaTime * 5);
        weaponHolder.LookAt(aimHelper.position);
        rightHandIKTarget.parent.transform.LookAt(aimHelper.position);
    }

    Vector3 getLookPosition(Transform camera)
    {
        //Raycast out from Camera.forward and get all hit objects
        RaycastHit[] hits = Physics.RaycastAll(camera.position, camera.forward);
        //If we hit something...
        if (hits.Length > 0)
        {
            Transform closestHit = null;
            float distance = 0f;
            Vector3 hitPoint = Vector3.zero;
            //Try and find the closest thing we hit that isn't ourselves
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform != this.transform
                    && !hit.transform.IsChildOf(this.GetComponentInParent<Transform>())
                    && (closestHit == null || hit.distance < distance))
                {
                    closestHit = hit.transform;
                    distance = hit.distance;
                    hitPoint = hit.point;
                }
            }
            //Return that closest thing's position
            return hitPoint;
        }
        //If we didn't hit anything...
        else
        {
            //Simply return a point 25 units away from camera.forward
            return playerCamera.position + (playerCamera.forward * 10);
        }
    }

    void setInputVars()
    {
        isAiming = Input.GetKey(KeyCode.Mouse1);
        isReloading = false;
    }

    void OnAnimatorIK(int layerIndex)
    {
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);

        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);

        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);

        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
    }
}
