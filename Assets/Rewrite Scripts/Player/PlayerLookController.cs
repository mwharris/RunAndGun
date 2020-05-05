using System;
using UnityEngine;

public class PlayerLookController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    public Camera PlayerCamera => playerCamera;

    private PlayerMovementStateMachine _movementStateMachine;
    
    private float _mouseSensitvity = 100f;
    private float _cameraXRotation = 0f;
    private bool _playerIsWallRunning = false;
    private float _wallRunZRotation = 0f;

    private void Awake()
    {
        _movementStateMachine = GetComponent<PlayerMovementStateMachine>();
    }
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        SetLookVars();

        var cameraLocalRot = playerCamera.transform.localRotation;
        
        float mouseX = PlayerInput.Instance.MouseX * _mouseSensitvity * Time.deltaTime;
        float mouseY = PlayerInput.Instance.MouseY * _mouseSensitvity * Time.deltaTime;

        _cameraXRotation -= mouseY;
        _cameraXRotation = Mathf.Clamp(_cameraXRotation, -90f, 90f);

        // Rotate the camera vertically for up/down look
        cameraLocalRot = Quaternion.Euler(_cameraXRotation, 0f, 0f);
        // Tilt sideways when wall-running
        if (_playerIsWallRunning)
        {
            cameraLocalRot.z = _wallRunZRotation;
        }
        playerCamera.transform.localRotation = cameraLocalRot;
        
        // Rotate our body horizontally for left/right look
        transform.Rotate(Vector3.up * mouseX);
    }

    private void SetLookVars()
    {
        var playerLookVars = _movementStateMachine.GetPlayerLookVars();
        _playerIsWallRunning = playerLookVars.PlayerIsWallRunning;
        _wallRunZRotation = playerLookVars.WallRunZRotation;
    }
}