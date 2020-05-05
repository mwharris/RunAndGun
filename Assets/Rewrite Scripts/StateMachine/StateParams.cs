using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateParams : IStateParams
{
    public Vector3 Velocity { get; set; } = Vector3.zero;
    public float GravityOverride { get; set; }
    public RaycastHit WallRunHitInfo { get; set; }
    public float WallRunZRotation { get; set; }
    public bool WallJumped { get; set; } = false;
    public AbstractBehavior inputBehavior { get; set; }
}
