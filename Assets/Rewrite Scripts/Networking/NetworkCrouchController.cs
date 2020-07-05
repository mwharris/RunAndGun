﻿using UnityEngine;

public class NetworkCrouchController : MonoBehaviour
{
    [SerializeField] private float crouchCamHeight;
    [SerializeField] private float crouchCcHeight;
    [SerializeField] private float crouchCcRadius;
    [SerializeField] private Vector3 crouchCcCenter;
    [SerializeField] private float crouchBodyZ;
    [SerializeField] private float crouchHeadZ;
    [SerializeField] private float crouchHeadY;
    
    private CharacterController _characterController;
    private CapsuleCollider _shotCollider;
    private BoxCollider _headCollider;
    private BodyController _bodyController;
    private Transform _thirdPersonBody;
    private Transform _playerCamera;

    private float _standardCamHeight;
    private float _standardCcHeight;
    private Vector3 _standardCcCenter;
    private float _standardCcRadius;
    private float _standardBodyZ;
    private float _standardHeadZ;
    private float _standardHeadY;

    private void Awake()
    {
        _shotCollider = GetComponent<CapsuleCollider>();
        
        _characterController = GetComponent<CharacterController>();
        _standardCcHeight = _characterController.height;
        _standardCcCenter = _characterController.center;
        _standardCcRadius = _characterController.radius;

        _bodyController = GetComponent<BodyController>();
        _playerCamera = _bodyController.ThirdPersonBody.playerCamera;
        _thirdPersonBody = _bodyController.ThirdPersonBody.body;
        _standardBodyZ = _thirdPersonBody.localPosition.z;
        
        _headCollider = GetComponent<BoxCollider>();
        _standardHeadZ = _headCollider.center.z;
        _standardHeadY = _headCollider.center.y;
    }
    
    // Handle crouching for networked characters to cut down on sending this data across the network
    public void HandleNetworkedCrouch(bool isCrouching, bool isGrounded, bool isCamResetting)
    {
        if (_standardCamHeight == 0f)
        {
            _standardCamHeight = _playerCamera.localPosition.y;
        }
        DoCrouch(_playerCamera, isCrouching, isGrounded, isCamResetting);
    }
    
    // Handle the actual shrinking/expanding of the player and components when crouching or standing
    private void DoCrouch(Transform playerCamera, bool isCrouching, bool isGrounded, bool isCameraResetting)
    {
        //Store the local position for modification
        Vector3 camLocalPos = playerCamera.localPosition;
        float ccHeight = _characterController.height;
        float ccRadius = _characterController.radius;
        Vector3 ccCenter = _characterController.center;
        float shotColHeight = _shotCollider.height;
        Vector3 shotColCenter = _shotCollider.center;
        float currBodyZ = _thirdPersonBody.localPosition.z;
        float currHeadZ = _headCollider.center.z;
        float currHeadY = _headCollider.center.y;
        //Modify the local position over time based on if we are/aren't crouching
        if (isCrouching && isGrounded)
        {
            if (ccHeight > crouchCcHeight)
            {
                ccHeight = Mathf.Lerp(ccHeight, crouchCcHeight, Time.deltaTime * 4f);
                shotColHeight = ccHeight;
            }
            if (ccRadius < crouchCcRadius)
            {
                ccRadius = Mathf.Lerp(ccRadius, crouchCcRadius, Time.deltaTime * 4f);
            }
            if (ccCenter != crouchCcCenter)
            {
                ccCenter = new Vector3(0, (ccHeight / 2) + 0.3f, 0);
                shotColCenter = new Vector3(0, (shotColHeight / 2) + 0.3f, 0);
            }
            if (camLocalPos.y > crouchCamHeight)
            {
                camLocalPos.y = Mathf.Lerp(camLocalPos.y, crouchCamHeight, Time.deltaTime * 4f);
            }
            if (currBodyZ > crouchBodyZ)
            {
                currBodyZ = Mathf.Lerp(currBodyZ, crouchBodyZ, Time.deltaTime * 8f);
            }
            if (currBodyZ > crouchBodyZ)
            {
                currBodyZ = Mathf.Lerp(currBodyZ, crouchBodyZ, Time.deltaTime * 8f);
            }
            if (currHeadZ < crouchHeadZ)
            {
                currHeadZ = Mathf.Lerp(currHeadZ, crouchHeadZ, Time.deltaTime * 8f);
            }
            if (currHeadY > crouchHeadY)
            {
                currHeadY = Mathf.Lerp(currHeadY, crouchHeadY, Time.deltaTime * 8f);
            }
        }
        //Coming back up
        else if (isCameraResetting && isGrounded)
        {
            bool allGood = true;
            if (ccHeight < _standardCcHeight)
            {
                ccHeight = Mathf.Lerp(ccHeight, _standardCcHeight, Time.deltaTime * 8f);
                shotColHeight = ccHeight - 0.2f;
                if (Mathf.Abs(ccHeight - _standardCcHeight) <= 0.1f)
                {
                    ccHeight = _standardCcHeight;
                }
                else
                {
                    allGood = false;
                }
            }
            if (ccRadius > _standardCcRadius)
            {
                ccRadius = Mathf.Lerp(ccRadius, _standardCcRadius, Time.deltaTime * 8f);
                if (Mathf.Abs(ccRadius - _standardCcRadius) <= 0.1f)
                {
                    ccRadius = _standardCcRadius;
                }
                else
                {
                    allGood = false;
                }
            }
            if (ccCenter != _standardCcCenter)
            {
                ccCenter = new Vector3(0, (ccHeight / 2) + 0.3f, 0);
                shotColCenter = new Vector3(0, (shotColHeight / 2) + 0.3f, 0);
            }
            if (currBodyZ < _standardBodyZ)
            {
                currBodyZ = Mathf.Lerp(currBodyZ, _standardBodyZ, Time.deltaTime * 8f);
                if (Mathf.Abs(currBodyZ - _standardBodyZ) <= 0.1f)
                {
                    currBodyZ = _standardBodyZ;
                }
                else
                {
                    allGood = false;
                }
            }
            if (currHeadZ > _standardHeadZ)
            {
                currHeadZ = Mathf.Lerp(currHeadZ, _standardHeadZ, Time.deltaTime * 8f);
                if (Mathf.Abs(currHeadZ - _standardHeadZ) <= 0.1f)
                {
                    currHeadZ = _standardHeadZ;
                }
                else
                {
                    allGood = false;
                }
            }
            if (currHeadY < _standardHeadY)
            {
                currHeadY = Mathf.Lerp(currHeadY, _standardHeadY, Time.deltaTime * 8f);
                if (Mathf.Abs(currHeadY - _standardHeadY) <= 0.1f)
                {
                    currHeadY = _standardHeadY;
                }
                else
                {
                    allGood = false;
                }
            }
            if (camLocalPos.y < _standardCamHeight)
            {
                camLocalPos.y = Mathf.Lerp(camLocalPos.y, _standardCamHeight, Time.deltaTime * 8f);
                if (Mathf.Abs(camLocalPos.y - _standardCamHeight) <= 0.1f)
                {
                    camLocalPos.y = _standardCamHeight;
                }
                else
                {
                    allGood = false;
                }
            }
            //Special case: when we are standing, we need to mark the camera as being moved since other scripts try to adjust the camera's position while standing
            if (allGood)
            {
                isCameraResetting = false;
            }
        }
        //Apply the local position updates
        playerCamera.localPosition = camLocalPos;
        _characterController.height = ccHeight;
        _characterController.radius = ccRadius;
        _characterController.center = ccCenter;
        _shotCollider.height = shotColHeight;
        _shotCollider.center = shotColCenter;
        _headCollider.center = new Vector3(_headCollider.center.x, currHeadY, currHeadZ);
        _thirdPersonBody.localPosition = new Vector3(_thirdPersonBody.localPosition.x, _thirdPersonBody.localPosition.y, currBodyZ);
    }
}
