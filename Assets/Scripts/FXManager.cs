using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FXManager : MonoBehaviour {

	public GameObject bulletFxPrefab;
	public GameObject defaultHitEffect;
	public GameObject redHitEffect;
	public GameObject enemyHitEffect;
	public GameObject deathEffect;
	public GameObject bullethole;
	public AudioSource aSource;
	public AudioClip[] footstepSounds;   
	public AudioClip gunShot;
	public AudioClip landingSound; 
	public AudioClip doubleJumpSound;   
	public AudioClip hitSound;
	public AudioClip[] deathSounds;

    [SerializeField] private GameObject killOverlay;
    [SerializeField] private GameObject headshotOverlay;
    private GameObject displayedOverlay;
    private float overlayTimer = 0.0f;
    
    private GameManager gameManager;
    private PlayerFinder playerFinder;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        playerFinder = new PlayerFinder();
    }

    void Update()
	{
		//Decrement the timer if we are showing the killer overlay
		if(overlayTimer > 0)
		{
            overlayTimer -= Time.deltaTime;
		}
		else if (displayedOverlay != null)
		{
            //Hide the contents of the overlay
            displayedOverlay.transform.GetChild(0).GetComponent<Text>().enabled = false;
            displayedOverlay.transform.GetChild(1).GetComponent<Text>().enabled = false;
		}
	}

	[PunRPC]
	void KillNotification(string deadPlayerName, string killerName, bool headshot)
	{
        //Find our player
        GameObject ourPlayer = gameManager.MyPlayer;
        PhotonView pView = ourPlayer == null ? null : ourPlayer.GetComponent<PhotonView>();
        //Only show notification for ourselves
        if (pView != null && pView.owner != null && pView.owner.NickName == killerName) 
		{
            //Determine which overlay to display based on headshot or not
            displayedOverlay = headshot ? headshotOverlay : killOverlay;
            //Set the name of the person we killed
            displayedOverlay.transform.GetChild(1).GetComponent<Text>().text = deadPlayerName;
            //Show the contents of the overlay
            displayedOverlay.transform.GetChild(0).GetComponent<Text>().enabled = true;
            displayedOverlay.transform.GetChild(1).GetComponent<Text>().enabled = true;
            //Put the overlay on a timer
            overlayTimer = 3.0f;
		}
	}

	[PunRPC]
	void BulletFX(int photonID, Vector3 endPos, bool hitEnemy, bool hitRed)
	{
        //Find the player who shot
        GameObject player = playerFinder.FindPlayerByPUNId(photonID);
        if (player != null)
        {
            BodyController bc = player.GetComponent<BodyController>();
            PlayerBodyData pbd = bc.PlayerBodyData;
            //Find the location of that player's weapon's fire point
            Transform playerWeapon = player.GetComponent<BodyController>().PlayerBodyData.weapon;
            Transform weaponFirePoint = playerWeapon.GetComponent<WeaponData>().FirePoint;
            Vector3 startPos = weaponFirePoint.position;
            //Show the bullet FX
            GameObject bulletFX = (GameObject)Instantiate(bulletFxPrefab, startPos, Quaternion.LookRotation(endPos - startPos));
            //Play the shooting animation
            Animator parentAnim = weaponFirePoint.parent.GetComponent<Animator>();
            parentAnim.SetTrigger("ShootTrig");
            Animator grandParentAnim = weaponFirePoint.parent.parent.GetComponent<Animator>();
            grandParentAnim.SetTrigger("ShootTrig");
            //Show our line rendered bullet trail
            LineRenderer lr = bulletFX.transform.Find("LineFX").GetComponent<LineRenderer>();
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);
            //Play our gun shot
            AudioSource.PlayClipAtPoint(gunShot, startPos);
            //Show FX
            if (hitEnemy)
            {
                //Show some blood
                Instantiate(enemyHitEffect, endPos, Quaternion.identity);
                //Play a sound
                aSource.PlayOneShot(hitSound);
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
		//Show some death effects
		Instantiate(deathEffect, pos, Quaternion.identity);
		//Play a sound
		PlayDeathSound(pos);
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
		AudioClip clipToPlay;

		//Pick & play a random footstep sound from the array,
		int n = Random.Range(1, footstepSounds.Length);
		clipToPlay = footstepSounds[n];
		AudioSource.PlayClipAtPoint(clipToPlay, pos);

		//Move picked sound to index 0 so it's not picked next time
		footstepSounds[n] = footstepSounds[0];
		footstepSounds[0] = clipToPlay;
	}

	[PunRPC]
	void LandingFX(Vector3 pos)
	{
		AudioSource.PlayClipAtPoint(landingSound, pos);
	}

	[PunRPC]
	void DoubleJumpFX(Vector3 pos)
	{
		AudioSource.PlayClipAtPoint(doubleJumpSound, pos);
	}
}
