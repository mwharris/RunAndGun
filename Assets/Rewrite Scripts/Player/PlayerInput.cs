using UnityEngine;

public class PlayerInput : MonoBehaviour, IPlayerInput
{
    public static IPlayerInput Instance { get; set; }
    
    public float Vertical => Input.GetAxis("Vertical");
    public bool VerticalHeld => Input.GetButton("Vertical");
    public float VerticalRaw => Input.GetAxisRaw("Vertical");

    public float Horizontal => Input.GetAxis("Horizontal");
    public bool HorizontalHeld => Input.GetButton("Horizontal");
    public float HorizontalRaw => Input.GetAxisRaw("Horizontal");
    
    public float MouseX => Input.GetAxis("Mouse X");
    public float MouseY => Input.GetAxis("Mouse Y");
    
    public bool ShiftDown => Input.GetKeyDown(KeyCode.LeftShift); 
    public bool ShiftHeld => Input.GetKey(KeyCode.LeftShift);

    public bool SpaceDown => Input.GetKeyDown(KeyCode.Space);
    public bool SpaceHeld => Input.GetKey(KeyCode.Space);

    public bool CrouchDown => Input.GetKeyDown(KeyCode.LeftControl);
    public bool CrouchHeld => Input.GetKey(KeyCode.LeftControl);
    
    private void Awake()
    {
        Instance = this;
    }

    public void Tick()
    {
        // TODO: ...
    }
}
