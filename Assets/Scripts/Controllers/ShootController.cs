using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class ShootController : AbstractBehavior 
{
    //General global variables - Set by Unity
    [SerializeField] private float kickDampening = 0.25f;
    [SerializeField] private AudioSource aSource;

    //General global variables - private
    private Camera _playerCamera;
	private float _baseFov;
	private PhotonView _pView;
	private float aimRecoilAmount = 0.05f;
	private float hipRecoilAmount = 0.1f;

    //Weapon specific stuff
	private float _cooldownTimer;
	private float _reloadTimer;

	//HUD variables
	private Text _bulletCountText;
	private Text _clipSizeText;
	private GameObject _hitIndicator;
	private float _hitIndicatorTimer;
	private float _hitIndicatorTimerMax;

	//Imported Scripts
	private FXManager _fxManager;
    private RPCManager _rpcManager;
	private GameManager _gameManager;
	private RecoilController _recoilController;
	private AccuracyController _accuracyController;
    private BodyController _bodyController;
    private PlayerBodyData _playerBodyData;
    private WeaponData _currWeaponData;

    void Start()
	{
        //Get components from the player body data
        _bodyController = GetComponent<BodyController>();
        SetBodyControlVars();
        _playerCamera = _playerBodyData.playerCamera.GetComponent<Camera>();
        //Initialize timers
        _cooldownTimer = 0.0f;
		_hitIndicatorTimerMax = 0.3f;
		_hitIndicatorTimer = _hitIndicatorTimerMax;
		//Initialize a reference to the FX and RPC Managers
		_fxManager = FindObjectOfType<FXManager>();
        _rpcManager = FindObjectOfType<RPCManager>();
        //Initialize a reference to the GameManager
        _gameManager = FindObjectOfType<GameManager>();
		//Initialize a reference to the Recoil and Accuracy controllers
		_recoilController = GetComponent<RecoilController>();
		_accuracyController = GetComponent<AccuracyController>();
        //Initialize a reference to the Hit Indicators
        _hitIndicator = GameObject.FindGameObjectWithTag("HitIndicator");
		//Initialize a reference to the bullet count
		GameObject txt = GameObject.FindGameObjectWithTag("BulletCount");
		_bulletCountText = txt.GetComponent<Text>();
        _bulletCountText.text = _currWeaponData.MagazineCapacity.ToString();
        //...and max bullet capacity
        txt = GameObject.FindGameObjectWithTag("ClipSize");
		_clipSizeText = txt.GetComponent<Text>();
		_clipSizeText.text = _currWeaponData.MagazineCapacity.ToString();
		//Get the attached PhotonView
		_pView = GetComponent<PhotonView>();
		//Get the camera's original FOV range
		_baseFov = _playerCamera.fieldOfView;
	}

    void Update()
    {
        //Make sure we keep our variables from the body controller up to date
        SetBodyControlVars();

        //Gather inputs needed below
        bool isShootDown = inputState.GetButtonPressed(inputs[0]);
		float shootHoldTime = inputState.GetButtonHoldTime(inputs[0]);
		bool isAimDown = inputState.GetButtonPressed(inputs[1]);
		bool isReloadDown = inputState.GetButtonPressed(inputs[2]);

		//Hide the hit indicators from last frame
		HideHitIndicator();			

		//Make sure we aren't reloading
		if(_gameManager.GetGameState() == GameManager.GameState.playing && _reloadTimer <= 0) {
			//Update the displayed bullet count
			_bulletCountText.text = _currWeaponData.BulletCount.ToString();
            _clipSizeText.text = _currWeaponData.MagazineCapacity.ToString();

			//Determine if we are attempting to aim our weapon
			if(isAimDown)
			{
				Aim();
			}
			else if (inputState.playerIsAiming || inputState.playerIsReloading)
			{
				StopAiming();
			}

			//Determine if we try to shoot the weapon
			if(isShootDown && _cooldownTimer <= 0)
			{
				//Only fire if there are bullets in the clip
				if(_currWeaponData.BulletCount > 0)
				{
					Shoot(isAimDown);
				}
				//If the clip is empty, reload instead
				else if (shootHoldTime == 0)
                {
                    Reload();
                }
			}
            //If we're not shooting than make sure we don't flag ourselves as such
            else
            {
                inputState.playerIsShooting = false;
            }

			//Reload if 'R' is pressed and the clip is not full
			if(isReloadDown && _currWeaponData.BulletCount < _currWeaponData.MagazineCapacity)
			{
				Reload();
			}
		}
		else if (_gameManager.GetGameState() == GameManager.GameState.playing) {
			//Update the displayed bullet count
			_bulletCountText.text = "--";
			//Decrement the reload timer if we are reloading
			_reloadTimer -= Time.deltaTime;
		}

		//Decrement the cooldown timer
		_cooldownTimer -= Time.deltaTime;
	}

    private void SetBodyControlVars()
    {
        if (_bodyController != null)
        {
            _playerBodyData = _bodyController.PlayerBodyData;
            _currWeaponData = _playerBodyData.weapon.GetComponent<WeaponData>();
        }
    }

    //Handle aiming our weapon
    void Aim()
	{
		//Tell the animator to pull the gun to our face
		inputState.playerIsAiming = true;
		//Disable the crosshairs
        if (_currWeaponData.HideReticleOnAim)
        {
            foreach (Transform child in _currWeaponData.ReticleParent.transform)
            {
                child.GetComponent<Image>().enabled = false;
            }
        }
        //Make sure we turn off sprinting
        inputState.playerIsSprinting = false;
		//Zoom the camera in
		_playerCamera.fieldOfView = Mathf.Lerp(_playerCamera.fieldOfView, 50, 10 * Time.deltaTime);
	}

	//Helper method used by other classes to tell ShootController to stop aiming.
	//Usually due to jumping, running, or wall-running.
	public void StopAiming()
	{
		//Tell the animator to pull the gun to the hip
        inputState.playerIsAiming = false;
		//Enable the crosshairs
		foreach(Transform child in _currWeaponData.ReticleParent.transform)
		{
			child.GetComponent<Image>().enabled = true;
		}
		//Zoom the camera out to normal
		_playerCamera.fieldOfView = Mathf.Lerp(_playerCamera.fieldOfView, _baseFov, 10 * Time.deltaTime);
	}

	//Handle everything that needs to be done to fire a weapon
	void Shoot(bool isAimDown)
	{
		//Reset the shot timer
		_cooldownTimer = _currWeaponData.ShotDelayTime;

		//Decrement the bullet counter
		_currWeaponData.BulletCount--;

		//Shoot a Ray and find the closest thing we hit that isn't ourselves
		Vector3 shootVector = ApplyAccuracy(_playerCamera.transform.forward);
		Ray ray = new Ray(_playerCamera.transform.position, shootVector);
		HitPlayerInfo info = FindClosestHitInfo(ray);

        //Notify other controllers and players that we are shooting
        inputState.playerIsShooting = true;
        _rpcManager.GetComponent<PhotonView>().RPC("PlayerShot", RpcTarget.AllBuffered, _pView.Owner.ActorNumber);

		//Handle Recoil and Accuracy updates based on if we're aiming
		if(isAimDown)
		{
			_recoilController.StartRecoil(aimRecoilAmount);
			_accuracyController.AddShootingOffset(inputState.playerIsAiming);
            _playerBodyData.body.transform.localPosition += kickDampening * _currWeaponData.KickAmount;
            if (_currWeaponData.WeaponStyle == WeaponStyles.SingleHanded)
            {
	            _playerBodyData.body.transform.localEulerAngles += kickDampening * _currWeaponData.RotKickAmount;
            }
        }
		else
		{
			_recoilController.StartRecoil(hipRecoilAmount);
			_accuracyController.AddShootingOffset(inputState.playerIsAiming);
            _playerBodyData.body.transform.localPosition += _currWeaponData.KickAmount;
            _playerBodyData.body.transform.localEulerAngles += _currWeaponData.RotKickAmount;
        }

		//Check if we hit a red object
		bool hitRed = info.hitTransform != null && info.hitTransform.CompareTag("Red");

		//Flag to show if we hit an enemy or not
		bool hitEnemy = false;

		//Make sure we actually hit something
		if(info.hitTransform != null)
		{
			//Determine if the object we hit has hit points
			Health h = info.hitTransform.GetComponent<Health>();
			//If we did not find an object with health
			if(h == null)
			{
				//Loop through it's parents and try to find one that has health
				while(info.hitTransform.parent && h == null)
				{
                    info.hitTransform = info.hitTransform.parent;
					h = info.hitTransform.GetComponent<Health>();
				}
			}
			//Check if we eventually found an object with health
			if(h != null)
			{
				//Show the hit indicator
				hitEnemy = true;
				ShowHitIndicator();
				//Use an RPC to send damage over the network
				PhotonView pv = h.GetComponent<PhotonView>();
				if(pv != null)
				{
                    pv.RPC(
                        "TakeDamage", 
                        RpcTarget.AllBuffered,
                        _currWeaponData.WeaponDamage, 
                        _pView.Owner.NickName, 
                        _pView.Owner.ActorNumber, 
                        transform.position,
                        info.headshot
                    );
				}
			} 

			//Show some bullet FX
			if(_fxManager != null)
			{
				GunFX(info.hitPoint, hitEnemy, hitRed);
			}
		}
		//Hit nothing, show bullet FX anyway
		else if(_fxManager != null)
		{
            //Make the FX reach a certain distance from the camera
            info.hitPoint = _playerCamera.transform.position + (_playerCamera.transform.forward * 100f);
			GunFX(info.hitPoint, hitEnemy, hitRed);
		}
	}

	//Take a shooting vector and apply offsets based on recoil, weapon type
	private Vector3 ApplyAccuracy(Vector3 shootVector)
	{
		shootVector.x += WeightedRandomAccuracy(_accuracyController.totalOffset);
		shootVector.y += WeightedRandomAccuracy(_accuracyController.totalOffset);
		shootVector.z += WeightedRandomAccuracy(_accuracyController.totalOffset);
		return shootVector;
	}

	private float WeightedRandomAccuracy(float accuracyRange)
	{
		//This is 1/3 of the range
		float quarter = _accuracyController.totalOffset / 4;
		float twoThirds = (_accuracyController.totalOffset / 3) * 2;
		//Get a random value between 0 and 1
		float rand = Random.value;
		//40% change to shoot in the middle 25% of the accuracy range
		if(rand <= 0.4F)
		{
			return Random.Range(-quarter, quarter);
		}
		//35% chance to shoot in the middle 60% of the accuracy range
		if(rand <= 0.75F)
		{
			return Random.Range(-twoThirds, twoThirds);
		} 
		//20% to shoot anywhere within the accuracy range
		return Random.Range(-accuracyRange, accuracyRange);
	}

	//Helper function to reload our weapon
	void Reload()
	{
		//Free us from aiming if we are
		if (inputState.playerIsAiming) {
			StopAiming();
		}
        //Notify other players that we are reloading (for animations)
        _rpcManager.GetComponent<PhotonView>().RPC("PlayerReloaded", RpcTarget.AllBuffered, _pView.Owner.ActorNumber);
        //Start the reload timer
        _reloadTimer = _currWeaponData.ReloadTime;
		//Reset the bullet count
		_currWeaponData.BulletCount = _currWeaponData.MagazineCapacity;
		//Play a sound
		aSource.PlayOneShot(_currWeaponData.ReloadClip);
		//Play the reload animation
        inputState.playerIsReloading = true;
        inputState.playerIsShooting = false;
	}

	//Helper function to show the gun FX
	void GunFX(Vector3 hitPoint, bool hitEnemy, bool hitRed)
	{
		//Grab the location of the gun and spawn the FX there
		_fxManager.GetComponent<PhotonView>().RPC("BulletFX", RpcTarget.All, _pView.Owner.ActorNumber, hitPoint, hitEnemy, hitRed);
	}

    //Raycast in a line and find the closest object hit
    private HitPlayerInfo FindClosestHitInfo(Ray ray)
    {
        HitPlayerInfo info = new HitPlayerInfo();
        //Get all objects that our raycast hit
        RaycastHit[] hits = Physics.RaycastAll(ray);
		//Loop through all the things we hit
		foreach(RaycastHit hit in hits)
		{
			//Find the closest object we hit that is not ourselves
			if(hit.transform != this.transform 
				&& !hit.transform.IsChildOf(this.transform) 
				&& (info.hitTransform == null || hit.distance < info.distance))
			{
                //Update the closest hit and distance
                info.hitTransform = hit.transform;
                info.distance = hit.distance;
                info.hitPoint = hit.point;
                info.headshot = hit.collider.GetType() == typeof(BoxCollider);
            }
		}
		return info;
	}

	//Display the hit indicators
	void ShowHitIndicator()
	{
		_hitIndicatorTimer = _hitIndicatorTimerMax;
		//Show them all
		foreach (Transform child in _hitIndicator.transform)
		{
			child.gameObject.SetActive(true);
		}
	}

	//Hide the hit indicators after some time
	void HideHitIndicator()
	{
		_hitIndicatorTimer -= Time.deltaTime;
		//Show them all
		if(_hitIndicatorTimer <= 0)
		{
			foreach (Transform child in _hitIndicator.transform)
			{
				child.gameObject.SetActive(false);
			}
		}
	}

    //Wait until the animation is done to allow shooting
    public IEnumerator WaitForRecoilDone(float time)
	{
		yield return new WaitForSeconds(time);
        inputState.playerIsShooting = false;
	}
}
