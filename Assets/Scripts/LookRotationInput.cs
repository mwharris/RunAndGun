using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookRotationInput
{
    public Transform player;
    public Transform camera;
    public Transform neck;
    public Vector2 lookInput;
    public float mouseSensitivity;
    public bool invertY;
    public float wallRunZRotation;
    public bool isAiming;
    public bool isWallRunning;
    public bool wrapAround;

    public LookRotationInput(Transform p, Transform c, Vector2 li, float ms, bool iy, 
        bool aiming, bool wallRunningFlag, float wrz, bool wrap)
    {
        player = p;
        camera = c;
        lookInput = li;
        mouseSensitivity = ms;
        invertY = iy;
        isAiming = aiming;
        isWallRunning = wallRunningFlag;
        wallRunZRotation = wrz;
        wrapAround = wrap;
    }

}
