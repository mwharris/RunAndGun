using UnityEngine;
using System.Collections;

public class FXManager : MonoBehaviour {

	public GameObject bulletFxPrefab;
	public GameObject defaultHitEffect;
	public GameObject redHitEffect;
	public GameObject enemyHitEffect;
	public GameObject deathEffect;
	public AudioSource aSource;
	public AudioClip[] footstepSounds;   
	public AudioClip gunShot;
	public AudioClip landingSound; 
	public AudioClip doubleJumpSound;   
	public AudioClip hitSound;
	public AudioClip[] deathSounds; 

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
			//Show some red debris
			Instantiate(defaultHitEffect, endPos, Quaternion.identity);
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

	/*
	[PunRPC]
	void FootstepFX(Vector3 pos, bool isWallRunning)
	{
		AudioClip clipToPlay;

		//Pick & play a random footstep sound from the array,
		AudioClip[] soundsToUse;
		if(isWallRunning)
		{
			soundsToUse = wallRunSounds;
		}
		else
		{
			soundsToUse = footstepSounds;
		}
		int n = Random.Range(1, soundsToUse.Length);
		clipToPlay = soundsToUse[n];
		AudioSource.PlayClipAtPoint(clipToPlay, pos);

		//Move picked sound to index 0 so it's not picked next time
		if(isWallRunning)
		{
			wallRunSounds[n] = wallRunSounds[0];
			wallRunSounds[0] = clipToPlay;
		}
		else
		{
			footstepSounds[n] = footstepSounds[0];
			footstepSounds[0] = clipToPlay;
		}
	}
	*/

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
