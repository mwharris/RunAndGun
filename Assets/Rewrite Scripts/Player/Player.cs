using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform _playerBody;
    [SerializeField] private Camera _playerCamera;
    
    public Transform PlayerBody => _playerBody;
    public Camera PlayerCamera => _playerCamera;
}