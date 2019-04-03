using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookRotationInput
{
    //Inputs for the LookRotation() function in PlayerLook.cs
    public Transform player;
    public Transform camera;
    public Transform neck;
    public Vector2 lookInput;
    public float mouseSensitivity;
    public bool invertY;
    public bool aimAssistEnabled;
    public float wallRunZRotation;
    public bool isAiming;
    public bool isWallRunning;
    public bool wrapAround;

    //Outputs for the LookRotation() function in PlayerLook.cs
    public float headAngle;
    public bool lockedOnPlayer;
}
