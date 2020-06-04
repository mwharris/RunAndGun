using UnityEngine;

public class LookRotationInput
{
    //Inputs for the LookRotation() function in PlayerLook.cs
    public Transform Player { get; private set; }
    public Transform Camera { get; private set; }
    public Transform Neck { get; private set; }
    public Vector2 LookInput { get; private set; }
    public float MouseSensitivity { get; private set; }
    public bool InvertY { get; private set; }
    public bool AimAssistEnabled { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsWallRunning { get; private set; }
    
    // These properties need to be accessed by WallRunController
    public float WallRunZRotation { get; set; }
    public bool WrapAround { get; set; }

    //Outputs for the LookRotation() function in PlayerLook.cs
    public float HeadAngle { get; set; }
    public bool LockedOnPlayer { get; set; }

    // Just a helper function to fill out multiple values in this class
    public void SetValues(
        Transform player, 
        Transform playerCamera, 
        Vector2 lookInput, 
        float mouseSensitivity, 
        bool invertY, 
        bool aimAssist, 
        bool playerIsAiming, 
        bool isWallRunning, 
        float wallRunZRotation, 
        bool wrapAround, 
        bool lockedOnPlayer)
    {
        Clear();
        Player = player;
        Camera = playerCamera;
        LookInput = lookInput;
        MouseSensitivity = mouseSensitivity;
        InvertY = invertY;
        AimAssistEnabled = aimAssist;
        IsAiming = playerIsAiming;
        IsWallRunning = isWallRunning;
        WallRunZRotation = wallRunZRotation;
        WrapAround = wrapAround;
        LockedOnPlayer = lockedOnPlayer;
    }
    
    // Null out all the values for this class
    public void Clear()
    {
        Player = null;
        Camera = null;
        Neck = null;
        LookInput = Vector2.zero;
        MouseSensitivity = 0f;
        InvertY = false;
        AimAssistEnabled = false;
        WallRunZRotation = 0f;
        IsAiming = false;
        IsWallRunning = false;
        WrapAround = false;
        HeadAngle = 0;
        LockedOnPlayer = false;
    }
}