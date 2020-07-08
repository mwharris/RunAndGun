using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkJumpController : MonoBehaviour
{

    private CharacterController _characterController;
    private CapsuleCollider _capsuleCol;
    private BoxCollider _boxCol;
    
    private Vector3 _standardCcCenter;
    private Vector3 _standardCapsuleColCenter;
    private float _standardCapsuleColHeight;
    private Vector3 _standardBoxColCenter;
    
    // TODO: Look these values up from the old build
    public Vector3 jumpCapsuleColCenter;
    public float jumpCapsuleColHeight;
    public Vector3 jumpBoxColCenter;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _capsuleCol = GetComponent<CapsuleCollider>();
        _boxCol = GetComponent<BoxCollider>();
        
        _standardCcCenter = _characterController.center;
        _standardCapsuleColCenter = _capsuleCol.center;
        _standardCapsuleColHeight = _capsuleCol.height;
        _standardBoxColCenter = _boxCol.center;
    }

    // Handle jump colliders for networked characters to cut down on sending this data across the network
    public void HandleNetworkedJump(bool isJumping, bool isCrouching, bool isJumpResetting)
    {
        HandleHitboxes(isJumping, isCrouching, isJumpResetting);
    }
    
    private void HandleHitboxes(bool isJumping, bool isCrouching, bool isJumpResetting)
    {
        //Get the current versions of all our variables
        float currCcHeight = _characterController.height;
        Vector3 currCapColCenter = _capsuleCol.center;
        float currCapColHeight = _capsuleCol.height;
        Vector3 currBoxColCenter = _boxCol.center;
        //Lerp depending on if we're jumping or not
        if (isJumping)
        {
            if (currCapColCenter != jumpCapsuleColCenter)
            {
                currCapColCenter = Vector3.Lerp(currCapColCenter, jumpCapsuleColCenter, Time.deltaTime * 4f);
            }
            if (currCapColHeight != jumpCapsuleColHeight)
            {
                currCapColHeight = jumpCapsuleColHeight;
            }
            if (currBoxColCenter != jumpBoxColCenter)
            {
                currBoxColCenter = Vector3.Lerp(currBoxColCenter, jumpBoxColCenter, Time.deltaTime * 4f);
            }
        }
        else if (!isCrouching && isJumpResetting)
        {
            bool allGood = true;
            if (currCapColCenter != _standardCapsuleColCenter)
            {
                currCapColCenter = Vector3.Lerp(currCapColCenter, _standardCapsuleColCenter, Time.deltaTime * 8f);
                if (Mathf.Abs(currCapColCenter.y - _standardCapsuleColCenter.y) <= 0.1f)
                {
                    currCapColCenter = _standardCapsuleColCenter;
                }
                else
                {
                    allGood = false;
                }
            }
            if (currCapColHeight != _standardCapsuleColHeight)
            {
                currCapColHeight = _standardCapsuleColHeight;
            }
            if (currBoxColCenter != _standardBoxColCenter)
            {
                currBoxColCenter = Vector3.Lerp(currBoxColCenter, _standardBoxColCenter, Time.deltaTime * 8f);
                if (Mathf.Abs(currBoxColCenter.y - _standardBoxColCenter.y) <= 0.1f)
                {
                    currBoxColCenter = _standardBoxColCenter;
                }
                else
                {
                    allGood = false;
                }
            }
            if (allGood)
            {
                isJumpResetting = false;
            }
        }
        //Set our variables to the new lerped values
        _characterController.height = currCcHeight;
        _capsuleCol.center = currCapColCenter;
        _capsuleCol.height = currCapColHeight;
        _boxCol.center = currBoxColCenter;
    }
}
