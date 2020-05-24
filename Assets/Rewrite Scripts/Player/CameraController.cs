using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private Transform _cameraTransform;
    
    private float _cameraDefaultFov;
    private const float CameraSprintFov = 65f;

    public bool PlayerIsSprinting { get; set; } = false;
    
    private bool _playerIsGrounded = false;
    private bool _playerIsWallRunning = false;
    private bool _playerJumped = false;
    private bool _playerIsAiming = false;

    private void Awake()
    {
        _cameraTransform = _camera.transform;
        _cameraDefaultFov = _camera.fieldOfView;
    }

    private void Update()
    {
        HandleSprinting();
    }

    private void HandleSprinting()
    {
        if (PlayerIsSprinting || _playerIsWallRunning)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, CameraSprintFov, Time.deltaTime * 5f);
        }
        else if (_camera.fieldOfView != _cameraDefaultFov)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _cameraDefaultFov, Time.deltaTime * 5f); ;
        }
    }
    
}