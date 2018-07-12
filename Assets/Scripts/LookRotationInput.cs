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
    public bool wallRunRotationOK;
    public Vector3 wallRunCorrection;
    public float wallRunAngle1;
    public float wallRunAngle2;
    public bool wrapAround;

    public LookRotationInput(Transform p, Transform c, Vector2 li, float ms, bool iy, 
        float wrz, Vector3 wrc, float a1, float a2, bool wrap)
    {
        player = p;
        camera = c;
        lookInput = li;
        mouseSensitivity = ms;
        invertY = iy;
        wallRunZRotation = wrz;
        wallRunCorrection = wrc;
        wallRunRotationOK = true;
        wallRunAngle1 = a1;
        wallRunAngle2 = a2;
        wrapAround = wrap;
    }

}
