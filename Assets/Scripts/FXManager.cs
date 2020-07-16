using System;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Random = UnityEngine.Random;

public class FXManager : MonoBehaviour {

	public GameObject bulletFxPrefab;
	public GameObject defaultHitEffect;
	public GameObject redHitEffect;
	public GameObject enemyHitEffect;
	public GameObject deathEffect;
	public GameObject bullethole;
	public AudioClip[] footstepSounds;   
	public AudioClip landingSound; 
	public AudioClip doubleJumpSound;   
	public AudioClip[] deathSounds;

	[SerializeField] private GameObject killOverlay;
	[SerializeField] private GameObject headshotOverlay;
	
    private GameManager _gameManager;
    private PlayerFinder _playerFinder;
	private GameObject _displayedOverlay;
    private float _overlayTimer = 0.0f;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _playerFinder = new PlayerFinder();
    }

    void Update()
	{
		//Decrement the timer if we are showing the killer overlay
		if(_overlayTimer > 0)
		{
            _overlayTimer -= Time.deltaTime;
		}
		else if (_displayedOverlay != null)
        {
            //Hide the contents of the overlay components
            Transform label = _displayedOverlay.transform.GetChild(0);
            label.gameObject.SetActive(false);
            Transform playerName = _displayedOverlay.transform.GetChild(1);
            playerName.gameObject.SetActive(false);
		}
	}

	[PunRPC]
	void KillNotification(string deadPlayerName, string killerName, bool headshot)
	{
        //Find our player
        GameObject ourPlayer = _gameManager.MyPlayer;
        PhotonView pView = ourPlayer == null ? null : ourPlayer.GetComponent<PhotonView>();
        //Only show notification for ourselves
        if (pView != null && pView.Owner != null && pView.Owner.NickName == killerName) 
		{
            //Determine which overlay to display based on headshot or not
            _displayedOverlay = headshot ? headshotOverlay : killOverlay;
            //Show the label of the desired overlay
            Transform labelTransform = _displayedOverlay.transform.GetChild(0);
            if (labelTransform != null)
            {
                labelTransform.gameObject.SetActive(true);
            }
            //Set the name of the person we killed and show the text
            Transform playerNameTransform = _displayedOverlay.transform.GetChild(1);
            if (playerNameTransform != null)
            {
                playerNameTransform.gameObject.SetActive(true);
                TextMeshProUGUI tmp = playerNameTransform.GetComponent<TextMeshProUGUI>();
                tmp.text = deadPlayerName;
            }
            //Put the overlay on a timer
            _overlayTimer = 3.0f;
		}
	}

	[PunRPC]
	void BulletFX(int photonID, Vector3 endPos, bool hitEnemy, bool hitRed)
	{
        //Find the player who shot
        GameObject player = _playerFinder.FindPlayerByPUNId(photonID);
        if (player != null)
        {
            BodyController bc = player.GetComponent<BodyController>();
            PlayerBodyData pbd = bc.PlayerBodyData;
            Transform playerWeapon = pbd.weapon;
            WeaponData weaponData = pbd.GetWeaponData();
            Animator weaponAnimator = pbd.GetWeaponAnimator();
            //Find the location of that player's weapon fire point
            Transform weaponFirePoint = weaponData.FirePoint;
            Vector3 startPos = weaponFirePoint.position;
            //Show the bullet FX from the fire point to our destination
            GameObject bulletFX = (GameObject)Instantiate(bulletFxPrefab, startPos, Quaternion.LookRotation(endPos - startPos));
            //Play the shooting animation
            weaponAnimator.SetTrigger("ShootTrig");
            //Show our line rendered bullet trail
            LineRenderer lr = bulletFX.transform.Find("LineFX").GetComponent<LineRenderer>();
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);
            //Play our gun shot
            AudioSource.PlayClipAtPoint(weaponData.ShotClip, startPos);
            //Show FX
            if (hitEnemy)
            {
                //Show some hit effects
                Instantiate(enemyHitEffect, endPos, Quaternion.identity);
            }
            else if (hitRed)
            {
                //Show some red debris
                Instantiate(redHitEffect, endPos, Quaternion.identity);
            }
            else
            {
                //Show some debris
                Instantiate(defaultHitEffect, endPos, Quaternion.identity);
                Instantiate(bullethole, endPos, Quaternion.identity);
            }
        }
	}

	[PunRPC]
	void DeathFX(Vector3 pos)
	{
		if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
		{
			//Show some death effects
			Instantiate(deathEffect, pos, Quaternion.identity);
			//Play a sound
			PlayDeathSound(pos);
		}
	}

	void PlayDeathSound(Vector3 pos)
	{
		AudioClip clipToPlay;
			
		//Pick & play a random footstep sound from the array,
		int n = Random.Range(1, deathSounds.Length);
		clipToPlay = deathSounds[n];
		
		//Play our gun shot
		AudioSource.PlayClipAtPoint(clipToPlay, pos);
		
		//Move picked sound to index 0 so it's not picked next time
		deathSounds[n] = deathSounds[0];
		deathSounds[0] = clipToPlay;
	}

	[PunRPC]
	void FootstepFX(Vector3 pos)
	{
		if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
		{
			AudioClip clipToPlay;

			//Pick & play a random footstep sound from the array,
			int n = Random.Range(1, footstepSounds.Length);
			clipToPlay = footstepSounds[n];
			AudioSource.PlayClipAtPoint(clipToPlay, pos);

			//Move picked sound to index 0 so it's not picked next time
			footstepSounds[n] = footstepSounds[0];
			footstepSounds[0] = clipToPlay;
		}
	}

	[PunRPC]
	void LandingFX(Vector3 pos)
	{
		if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
		{
			AudioSource.PlayClipAtPoint(landingSound, pos);
		}
	}

	[PunRPC]
	void DoubleJumpFX(Vector3 pos)
	{
		if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
		{
			AudioSource.PlayClipAtPoint(doubleJumpSound, pos);
		}
	}
}
