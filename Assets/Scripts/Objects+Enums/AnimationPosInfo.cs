using System;
using UnityEngine;

[Serializable]
public class AnimationPosInfo
{
    public Vector3 localPos = Vector3.zero;
    public Vector3 localRot = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
}