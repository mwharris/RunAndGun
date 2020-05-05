using UnityEngine;

public interface IStateParams
{
    Vector3 Velocity { get; set; }
    float GravityOverride { get; set; }
    RaycastHit WallRunHitInfo { get; set; }
    float WallRunZRotation { get; set; }
    bool WallJumped { get; set; }
    AbstractBehavior inputBehavior { get; set;  }
}