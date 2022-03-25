using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
	[SerializeField] Image healthbarImage, shieldbarImage;
	[SerializeField] GameObject ui, scoreText, weaponText, livesTest, fuelbarImage, throttlebarImage;
	[SerializeField] GameObject mainEngine, mainEngineInput, reverseEngine, reverseEngineInput, warpEngine;
	[SerializeField] GameObject cameraHolder;

	AudioSource warpSound;
	float warpMaxVolume = 0.4f;

	[SerializeField] Item[] items;

	int itemIndex;
	int previousItemIndex = -1;

	float verticalLookRotation;
	Vector3 smoothMoveVelocity;
	Vector3 moveAmount;

	Rigidbody rb;

	PhotonView PV;

	const float maxHealth = 100f;
	float currentHealth = maxHealth;
	const float maxShield = 100f;
	float currentShield = maxShield;
	public bool isImmune;

	PlayerManager playerManager;

	public float speed = 40;
	public float sideSpeed = 10;
	public float verticalSpeed = 6;
	public float brakeSpeed = 4;

	private float activeSpeed;
	private float vertAxis;
	private float thrustIncrement;
	float throttle;
	private float activeSideSpeed;
	private float activeVertSpeed;
	static public float totalSpeed;
	static public float warpMulti = 1;
	static public bool warping;
	const float maxWarpFuel = 100f;
	public float warpFuel = maxWarpFuel;


	private float forwardAccerleration = 2;
	private float otherAccerleration = 1.25f;

	static public float mouseLookSpeed = 300;
	private Vector2 mouseDistance;
	private Vector2 mouseLocation;
	private Vector2 screenCenter;

	public float rollSpeed = 100;
	public float rollAccerleration = 4;
	private float rollInput;


	Transform player;
	public Camera cam;
	public float staticCursorDistance = 500f;
	public Image activeCursor, staticCursor;
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		PV = GetComponent<PhotonView>();

		playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			EquipItem(0);
		}
		else
		{
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(rb);
			Destroy(ui);
		}
		screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
		isImmune = false;
		StartCoroutine(AutoRefuel());
		Debug.Log("Refueling active");
		player = this.transform;
		mainEngine.SetActive(false);
		reverseEngine.SetActive(false);
		warpEngine.SetActive(false);
		warpSound = GetComponent<AudioSource>();
		warpSound.loop = true;
		warpSound.volume = warpMaxVolume;
		warpSound.mute = true;
		warpSound.Play();
	}

	void Update()
	{
		if(!PV.IsMine)
			return;

		if (throttle > 0.001)
		{
			mainEngine.SetActive(true);
			reverseEngine.SetActive(false);
		}
		else if (throttle < -0.001)
		{
			mainEngine.SetActive(false);
			reverseEngine.SetActive(true);
		}
		else
		{
			mainEngine.SetActive(false);
			reverseEngine.SetActive(false);
		}

		if (Input.GetKey(KeyCode.W))
		{
			mainEngineInput.SetActive(true);
		}
		else if (Input.GetKey(KeyCode.S))
		{
			reverseEngineInput.SetActive(true);
		}
		else
		{
			mainEngineInput.SetActive(false);
			reverseEngineInput.SetActive(false);
		}

	}
void FixedUpdate()
	{
		if(!PV.IsMine)
			return;

		vertAxis = Input.GetAxis("Vertical");
		if (vertAxis > 0)
		{
			thrustIncrement = 0.02f;
		}
		else if (vertAxis < 0)
		{
			thrustIncrement = -0.02f;
		}

		if (Input.GetButton("Vertical"))
		{
			throttle += thrustIncrement;
		}

		throttle = Mathf.Clamp(throttle, -0.5f, 1f);

		mouseLocation = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		mouseDistance = new Vector2((mouseLocation.x - screenCenter.x) / screenCenter.x, (mouseLocation.y - screenCenter.y) / screenCenter.y);
		mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1);

		rollInput = Mathf.Lerp(rollInput, Input.GetAxis("Roll"), rollAccerleration * Time.deltaTime);

		transform.Rotate(-mouseDistance.y * Time.deltaTime * mouseLookSpeed / 2, mouseDistance.x * Time.deltaTime * mouseLookSpeed / 2, rollSpeed * Time.deltaTime * -rollInput, Space.Self);

		activeSpeed = Mathf.Lerp(activeSpeed, throttle * speed, forwardAccerleration * Time.deltaTime);
		activeSideSpeed = Mathf.Lerp(activeSideSpeed, Input.GetAxisRaw("Horizontal") * sideSpeed, otherAccerleration * Time.deltaTime);
		activeVertSpeed = Mathf.Lerp(activeVertSpeed, Input.GetAxisRaw("Height") * verticalSpeed, otherAccerleration * Time.deltaTime);

		Vector3 movementForce = new Vector3(activeSpeed, activeSideSpeed, activeVertSpeed);
		rb.AddRelativeForce(activeSideSpeed * 300f * Time.deltaTime, activeVertSpeed * 300f * Time.deltaTime, activeSpeed * 300f * Time.deltaTime * warpMulti);

		totalSpeed = Mathf.Sqrt((activeSpeed * activeSpeed) + (activeSideSpeed * activeSideSpeed) + (activeVertSpeed * activeVertSpeed));
		totalSpeed = Mathf.Round(totalSpeed * 10);

		PlayerWarp();
		updateHUD();

		if (Input.GetKey(KeyCode.B))
		{
			Brake();
		}
	}



	void SetZero()
	{
		throttle = 0;
		if (rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}
	void Brake()
	{
		throttle = Mathf.Lerp(throttle, 0f, brakeSpeed * Time.deltaTime);
		activeSideSpeed = Mathf.Lerp(activeSideSpeed, 0f, brakeSpeed * Time.deltaTime);
		activeVertSpeed = Mathf.Lerp(activeVertSpeed, 0f, brakeSpeed * Time.deltaTime);
		transform.Rotate(Vector3.zero);

	}


	void EquipItem(int _index)
	{
		if(_index == previousItemIndex)
			return;

		itemIndex = _index;

		items[itemIndex].itemGameObject.SetActive(true);

		if(previousItemIndex != -1)
		{
			items[previousItemIndex].itemGameObject.SetActive(false);
		}

		previousItemIndex = itemIndex;

		if(PV.IsMine)
		{
			Hashtable hash = new Hashtable();
			hash.Add("itemIndex", itemIndex);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if(!PV.IsMine && targetPlayer == PV.Owner)
		{
			EquipItem((int)changedProps["itemIndex"]);
		}
	}

	void PlayerWarp()
	{
		if (Input.GetKey(KeyCode.G) && warpFuel > 0 && throttle > 0)
		{
			//gameObject.GetComponent<SU_TravelWarp>().Warp = true;
			//mainCamera.SetActive(false);
			mouseLookSpeed = 20f;
			warpMulti += .1f;
			warpMulti = Mathf.Clamp(warpMulti, 1, 3);
			warping = true;
			warpFuel -= .3f;
			totalSpeed += 100;
			warpEngine.SetActive(true);
            if (warpSound.mute)
            {
            	warpSound.mute = false;
            }
            if (warpSound.volume < warpMaxVolume)
            {
            	warpSound.volume += 2f * Time.deltaTime;
            }

        }
        else  //(Input.GetKeyUp(KeyCode.G) || warpFuel <= 0)
		{
			//gameObject.GetComponent<SU_TravelWarp>().Warp = false;
			//mainCamera.SetActive(true);
			mouseLookSpeed = 300f;
			warping = false;
			warpEngine.SetActive(false);
			if (warpMulti != 1f)
			{
				warpMulti = 1f;
			}
            if (warpSound.volume > 0.001f)
            {
            	warpSound.volume -= 5f * Time.deltaTime;
            }
            else
            {
            	warpSound.mute = true;
            }

        }


	}

	void OnCollisionEnter(Collision collision)
	{
		throttle = -.5f;
		Invoke("SetZero", 1.4f);
		Debug.Log("hit object");

		if (warping)
		{
			playerManager.Die();
			return;
		}
		if (collision.gameObject.tag == "Enemy")
		{
			playerManager.Die();
			//collision.gameObject.GetComponent<EnemyStats>().DestroySelf();
		}
		else if (collision.gameObject.tag == "Player" && collision.gameObject != rb.gameObject && rb != null)
        {
			TakeDamage(30);
			return;

		}

		if (isImmune == false)
		{
			if (currentShield >= 30)
			{
				currentShield -= 30;
			}
			else
			{
				currentShield -= 30;
				currentHealth -= Mathf.Abs(currentShield);
				currentShield = 0;
			}
		}
		updateHUD();
		Debug.Log("health: " + currentHealth + "shield: " + currentShield);
		isImmune = true;
		Invoke("NoMoreImmunity", 2f);
		if (currentHealth <= 0 )
		{
			Die();
		}
	}
	void NoMoreImmunity()
	{
		isImmune = false;
	}
	public void HealHealth(float hp)
    {
		if(currentHealth != 100)
        {
			currentHealth += hp;
			currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        }else if(currentShield != 100)
        {
			currentShield += hp;
			currentShield = Mathf.Clamp(currentShield, 0, 100);
        }


	}
	public void TakeDamage(float damage)
	{
		PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
	}


	[PunRPC]
	void RPC_TakeDamage(float damage)
	{
		if(!PV.IsMine)
			return;

		if (isImmune == false)
		{
			if (currentShield >= 30)
			{
				currentShield -= 30;
			}
			else
			{
				currentShield -= 30;
				currentHealth -= Mathf.Abs(currentShield);
				currentShield = 0;
			}

		}

		if (currentHealth <= 0)
		{
			Die();
		}
	}
	void updateHUD()
    {
		scoreText.GetComponent<Text>().text = totalSpeed.ToString("F0") + " mph" +
			//   "\n" + avgFrameRate + " fps" +
			"\n" + warpFuel.ToString("F0") + " : fuel left" +
			"\n" + throttle.ToString("F3") //+
			//"\n" + EnemyStats.totalDeaths.ToString("F0") + " : enemies killed";
			//weaponText.GetComponent<TextMeshProUGUI>().text = WeaponName(LaserFire.currentWeapon) +
			//   "\n" + LaserFire.currentWeaponAmmo.ToString("F0");
		;
		healthbarImage.fillAmount = currentHealth / 100;
		shieldbarImage.fillAmount = currentShield / 100;
		//Debug.Log(throttlebarImage.GetComponent<Renderer>().material.GetFloat("Fill Amount"));
		throttlebarImage.GetComponent<Renderer>().material.SetFloat("_FillAmount", throttle);
		fuelbarImage.GetComponent<Renderer>().material.SetFloat("_FillAmount", warpFuel / 200);
	}
	IEnumerator AutoRefuel()
	{
		yield return new WaitForSeconds(1f);
		while (true)
		{
			if (warpFuel < 10 && PlayerController.warping == false)
			{
				yield return new WaitForSeconds(.8f);
				warpFuel++;
			}
			else if (warpFuel < 30 && PlayerController.warping == false)
			{
				yield return new WaitForSeconds(.45f);
				warpFuel++;
			}
			else
			{
				yield return null;
			}
		}
	}
	void Die()
	{

		playerManager.Die();
	}

    public void LateUpdate()
    {
		if (player != null && cam != null)
		{
			Vector3 staticPos = (player.forward * staticCursorDistance) + player.position;
			Vector3 screenPos = cam.WorldToScreenPoint(staticPos);
			screenPos.z -= 10f;

			staticCursor.transform.position = Vector3.Lerp(staticCursor.transform.position, screenPos, Time.deltaTime * 6f);
		}
		if (activeCursor != null && player != null)
		{
			activeCursor.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y - 24f, Input.mousePosition.z);
		}

		if (PlayerController.warping == true && staticCursor != null)
		{

			staticCursor.gameObject.SetActive(false);
		}
		else if (staticCursor != null)
		{
			staticCursor.gameObject.SetActive(true);
		}
	}
}