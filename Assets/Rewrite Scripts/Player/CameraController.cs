using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private Transform _cameraTransform;
    
    private float _cameraDefaultFov;
    private const float CameraSprintFov = 65f;

    public bool PlayerIsSprinting { get; set; } = false;
    public bool PlayerIsWallRunning { get; set; } = false;
    public bool PlayerIsAirborneFast { get; set; } = false;
    public bool PlayerIsSliding { get; set; } = false;
    
    private bool _playerIsGrounded = false;
    private bool _playerJumped = false;
    private bool _playerIsAiming = false;

    private void Awake()
    {
        _cameraTransform = _camera.transform;
        _cameraDefaultFov = _camera.fieldOfView;
    }

    private void Update()
    {
        if (PlayerIsSprinting || PlayerIsWallRunning || PlayerIsAirborneFast || PlayerIsSliding)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, CameraSprintFov, Time.deltaTime * 5f);
        }
        else if (_camera.fieldOfView != _cameraDefaultFov)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _cameraDefaultFov, Time.deltaTime * 5f);
        }
    }
    
}