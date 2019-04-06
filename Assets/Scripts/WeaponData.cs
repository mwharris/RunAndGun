using UnityEngine;
using System.Collections;

public class WeaponData : MonoBehaviour
{
    [SerializeField] private float weaponDamage = 25f;
    public float WeaponDamage { get { return weaponDamage; } }

    [SerializeField] private int magazineCapacity = 12;
    public int MagazineCapacity { get { return magazineCapacity; } }

    [SerializeField] private float reloadTime = 2.5f;
    public float ReloadTime { get { return reloadTime; } }

    [SerializeField] private float shotDelayTime = 0.315f;
    public float ShotDelayTime { get { return shotDelayTime; } }

    [SerializeField] private Transform firePoint;
    public Transform FirePoint { get { return firePoint; } }

    [SerializeField] private AudioClip reloadClip;
    public AudioClip ReloadClip { get { return reloadClip; } }

    [SerializeField] private string reticleParentTag;
    [SerializeField] private GameObject reticleParent;
    public GameObject ReticleParent { get { return reticleParent; } }

    [SerializeField] private WeaponStyles weaponStyle;
    public WeaponStyles WeaponStyle { get { return weaponStyle; } }

    private void Awake()
    {
        if (reticleParent == null)
        {
            reticleParent = GameObject.FindGameObjectWithTag(reticleParentTag);
        }
    }
}