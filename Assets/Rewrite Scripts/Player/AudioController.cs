using Photon.Pun;

public class AudioController
{
    private readonly Player _player;
    private readonly PhotonView _fxPhotonView;
    
    public AudioController(Player player, FXManager fxManager)
    {
        _player = player;
        _fxPhotonView = fxManager.GetComponent<PhotonView>();
    }

    public float PlayFootstep(float currTimer, float resetTimerValue, float forwardSpeed, float sideSpeed)
    {
        // Play a sound when the associated timer is up and we're moving
        if (currTimer <= 0 && (forwardSpeed != 0 || sideSpeed != 0))
        {
            // Reset the timer
            currTimer = resetTimerValue;
            // Play a networked sound via the FXManager
            _fxPhotonView.RPC("FootstepFX", RpcTarget.All, _player.transform.position);
        }
        return currTimer;
    }

    public float PlayWallRunFootstep(float currTimer, float resetTimerValue)
    {
        // Play a sound when the associated timer is up, regardless of speed
        if (currTimer <= 0)
        {
            // Reset the timer
            currTimer = resetTimerValue;
            // Play a networked sound via the FXManager
            _fxPhotonView.RPC("FootstepFX", RpcTarget.All, _player.transform.position);
        }
        return currTimer;
    }

    public void PlayJump(bool doubleJump)
    {
        string rpcName = doubleJump ? "DoubleJumpFX" : "JumpFX";
        _fxPhotonView.RPC(rpcName, RpcTarget.All, _player.transform.position);
    }

    public void PlayLand()
    {
        _fxPhotonView.RPC("LandingFX", RpcTarget.All, _player.transform.position);
    }

}
