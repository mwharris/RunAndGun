using UnityEngine;
using System.Collections;

public class WeaponData : MonoBehaviour
{
    [SerializeField] private WeaponStyles weaponStyle;
    public WeaponStyles WeaponStyle { get { return weaponStyle; } }

    [SerializeField] private float weaponDamage = 25f;
    public float WeaponDamage { get { return weaponDamage; } }

    [SerializeField] private int magazineCapacity = 12;
    public int MagazineCapacity { get { return magazineCapacity; } }

    [SerializeField] private float reloadTime = 2.5f;
    public float ReloadTime { get { return reloadTime; } }

    [SerializeField] private float shotDelayTime = 0.315f;
    public float ShotDelayTime { get { return shotDelayTime; } }

    [SerializeField] private float recoilSpeed = 4f;
    public float RecoilSpeed { get { return recoilSpeed; } }

    [SerializeField] private Vector3 kickAmount = Vector3.zero;
    public Vector3 KickAmount { get { return kickAmount; } }

    [SerializeField] private Vector3 rotKickAmount = Vector3.zero;
    public Vector3 RotKickAmount { get { return rotKickAmount; } }

    [SerializeField] private float kickReturnSpeed = 0f;
    public float KickReturnSpeed { get { return kickReturnSpeed; } }

    [SerializeField] private float kickReturnAimMultiplier = 0f;
    public float KickReturnAimMultiplier { get { return kickReturnAimMultiplier; } }

    [SerializeField] private Transform firePoint;
    public Transform FirePoint { get { return firePoint; } }

    [SerializeField] private string reticleParentTag;
    [SerializeField] private GameObject reticleParent;
    public GameObject ReticleParent { get { return reticleParent; } }

    [SerializeField] private bool hideReticleOnAim = true;
    public bool HideReticleOnAim { get { return hideReticleOnAim; } }

    [SerializeField] private AnimationPosInfo defaultArmsPosition;
    public AnimationPosInfo DefaultArmsPosition { get { return defaultArmsPosition; } }

    [SerializeField] private AnimationPosInfo crouchArmsPosition;
    public AnimationPosInfo CrouchArmsPosition { get { return crouchArmsPosition; } }

    /*
    [SerializeField] private Animator weaponIKAnimator;
    public Animator WeaponIKAnimator {  get { return weaponIKAnimator; } }
    */

    [SerializeField] private AudioClip shotClip;
    public AudioClip ShotClip { get { return shotClip; } }

    [SerializeField] private AudioClip reloadClip;
    public AudioClip ReloadClip { get { return reloadClip; } }

    private void Awake()
    {
        if (reticleParent == null)
        {
            reticleParent = GameObject.FindGameObjectWithTag(reticleParentTag);
        }
    }
}