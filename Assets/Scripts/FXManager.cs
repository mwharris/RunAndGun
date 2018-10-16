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

	private float killOverlayTimer = 0.0f;

	void Update()
	{
		//Decrement the timer if we are showing the killer overlay
		if(killOverlayTimer > 0)
		{
			killOverlayTimer -= Time.deltaTime;
		}
		else
		{
			//Get a reference to the overlay
			GameObject killOverlay = GameObject.FindGameObjectWithTag("KillOverlay");
			//Hide the contents of the overlay
			killOverlay.transform.GetChild(0).GetComponent<Text>().enabled = false;
			killOverlay.transform.GetChild(1).GetComponent<Text>().enabled = false;
		}
	}

	[PunRPC]
	void KillNotification(string deadPlayerName, string killerName)
	{
		//Find our player
		GameObject ourPlayer = FindOurPlayer();
        PhotonView pView = ourPlayer.GetComponent<PhotonView>();

        //Only show notification for ourselves
        if (ourPlayer != null && pView.owner.name == killerName) 
		{
			//Get a reference to the overlay
			GameObject killOverlay = GameObject.FindGameObjectWithTag("KillOverlay");
			//Set the name of the person we killed
			killOverlay.transform.GetChild(1).GetComponent<Text>().text = deadPlayerName;
			//Show the contents of the overlay
			killOverlay.transform.GetChild(0).GetComponent<Text>().enabled = true;
			killOverlay.transform.GetChild(1).GetComponent<Text>().enabled = true;
			//Put the overlay on a timer
			killOverlayTimer = 3.0f;
		}
	}

    //Iterate through the players and find ours
    private GameObject FindOurPlayer()
    {
        //Get all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //Loop through and find the player we are controlling
        foreach (GameObject currPlayer in players)
        {
            if (currPlayer.GetComponent<PhotonView>().isMine)
            {
                return currPlayer;
            }
        }
        return null;
    }

	[PunRPC]
	void BulletFX(Vector3 startPos, Vector3 endPos, bool hitEnemy, bool hitRed)
	{
		//Show the bullet FX
		GameObject bulletFX = (GameObject) Instantiate(bulletFxPrefab, startPos, Quaternion.LookRotation(endPos - startPos));
		//Show our line rendered bullet trail
		LineRenderer lr = bulletFX.transform.Find("LineFX").GetComponent<LineRenderer>();
		lr.SetPosition(0, startPos);
		lr.SetPosition(1, endPos);
		//Play our gun shot
		AudioSource.PlayClipAtPoint(gunShot, startPos);
		//Show FX
		if(hitEnemy)
		{
			//Show some blood
			Instantiate(enemyHitEffect, endPos, Quaternion.identity);
			//Play a sound
			aSource.PlayOneShot(hitSound);
		}
		else if(hitRed)
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
