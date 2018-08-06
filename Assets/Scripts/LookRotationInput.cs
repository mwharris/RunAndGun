using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookRotationInput {

    public Transform player;
    public Transform camera;
    public Transform neck;
    public Vector2 lookInput;
    public float mouseSensitivity;
    public bool invertY;
    public float wallRunZRotation;
    public bool isWallRunning;
    public float wallRunAngle1;
    public float wallRunAngle2;
    public bool wrapAround;

    public LookRotationInput(Transform p, Transform c, Vector2 li, float ms, bool iy, 
        bool wallRunningFlag, float wrz, float wrAngle1, float wrAngle2, bool wrap)
    {
        player = p;
        camera = c;
        lookInput = li;
        mouseSensitivity = ms;
        invertY = iy;
        isWallRunning = wallRunningFlag;
        wallRunZRotation = wrz;
        wallRunAngle1 = wrAngle1;
        wallRunAngle2 = wrAngle2;
        wrapAround = wrap;
    }

}
