using UnityEngine;

public interface IStateParams
{
    Vector3 Velocity { get; set; }
    float GravityOverride { get; set; }
    bool PreserveSprint { get; set; }
    RaycastHit WallRunHitInfo { get; set; }
    float WallRunZRotation { get; set; }
    bool WallJumped { get; set; }
    bool WallRunningLeft { get; set; }
    bool WallRunningRight { get; set; }
    AbstractBehavior inputBehavior { get; set;  }
}