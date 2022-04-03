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

    [SerializeField] GameObject leftBarrel, rightBarrel, mainBarrel, mainBarrel2, mainBarrel3;
    Vector3 shootFromBarrel;
    int shootOrder;
    static public int currentWeapon;
    static public int currentWeaponAmmo;
    private float timeLastFired;


    public int laser1Ammo = 100;
    public int laser2Ammo = 50;
    public int laser3Ammo = 200;
    public int laser4Ammo = 50;
    public int defaultLaser1Ammo;
    public int defaultLaser2Ammo;
    public int defaultLaser3Ammo;
    public int defaultLaser4Ammo;

    public bool infiniteAmmo = false;

    public HitDetection laser1;
    ParticleSystem laser1FXLeft;
    ParticleSystem laser1FXRight;
    float laser1Force = 1000f;
    float laser1FireRate = .2f;
    float l1Spread = 0.15f;

    public HitDetection laser2;
    ParticleSystem laser2FX;
    float laser2Force = 1500f;
    float laser2FireRate = .4f;
    float l2Spread = 0f;

    public HitDetection laser3;
    private ParticleSystem laser3FX;
    float laser3Force = 1000f;
    float laser3FireRate = .12f;
    float l3Spread = 0.5f;

    public HitDetection laser4;
    ParticleSystem laser4FX;
    float laser4Force = 1000f;
    float laser4FireRate = .5f;
    float l4Spread = 4f;


    [SerializeField] Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    Rigidbody rb;

    PhotonView PV;

    const float maxHealth = 100f;
    public float currentHealth = maxHealth;
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
        if (PV.IsMine)
        {

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

        currentWeapon = 1;
        currentWeaponAmmo = laser1Ammo;
        laser1FXLeft = leftBarrel.GetComponent<ParticleSystem>();
        laser1FXRight = rightBarrel.GetComponent<ParticleSystem>();
        laser2FX = mainBarrel.GetComponent<ParticleSystem>();
        laser3FX = mainBarrel3.GetComponent<ParticleSystem>();
        laser4FX = mainBarrel2.GetComponent<ParticleSystem>();
        defaultLaser1Ammo = laser1Ammo;
        defaultLaser2Ammo = laser2Ammo;
        defaultLaser3Ammo = laser3Ammo;
        defaultLaser4Ammo = laser4Ammo;

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
        if (!PV.IsMine)
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
        FireWeapon();

    }
    void FixedUpdate()
    {
        if (!PV.IsMine)
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


    void FireWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon = 1;
            currentWeaponAmmo = laser1Ammo;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon = 2;
            currentWeaponAmmo = laser2Ammo;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentWeapon = 3;
            currentWeaponAmmo = laser3Ammo;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentWeapon = 4;
            currentWeaponAmmo = laser4Ammo;
        }
        if (warping == false)
        {
            switch (currentWeapon)
            {
                case 1:
                    if (Input.GetMouseButton(0) && laser1Ammo > 0)
                    {
                        if (Time.time > laser1FireRate + timeLastFired)
                        {
                            leftBarrel.GetComponent<AudioSource>().Play();
                            FireLaser1();
                            timeLastFired = Time.time;
                        }
                    }
                    break;
                case 2:
                    if (Input.GetMouseButton(0) && laser2Ammo > 0)
                    {
                        if (Time.time > laser2FireRate + timeLastFired)
                        {
                            mainBarrel.GetComponent<AudioSource>().Play();
                            FireLaser2();
                            timeLastFired = Time.time;
                        }
                    }
                    break;
                case 3:
                    if (Input.GetMouseButton(0) && laser3Ammo > 0)
                    {
                        if (Time.time > laser3FireRate + timeLastFired)
                        {
                            mainBarrel3.GetComponent<AudioSource>().Play();
                            FireLaser3();
                            timeLastFired = Time.time;
                        }
                    }
                    break;
                case 4:
                    if (Input.GetMouseButton(0) && laser4Ammo > 0)
                    {
                        if (Time.time > laser4FireRate + timeLastFired)
                        {
                            mainBarrel2.GetComponent<AudioSource>().Play();
                            FireLaser4();
                            timeLastFired = Time.time;
                        }
                    }
                    break;
            }
        }
    }

    public void ApplyDamage(float damage)
    {
        currentHealth -= damage;
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            //EquipItem((int)changedProps["itemIndex"]);
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
        if (currentHealth <= 0)
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
        if (currentHealth != 100)
        {
            currentHealth += hp;
            currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        }
        else if (currentShield != 100)
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
        if (!PV.IsMine)
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

    void FireLaser1()
    {
        switch (shootOrder)
        {
            case 0:
                shootFromBarrel = leftBarrel.transform.position + transform.forward * 5f;
                shootOrder++;
                laser1FXLeft.Play();
                break;

            case 1:
                shootFromBarrel = rightBarrel.transform.position + transform.forward * 5f;
                shootOrder--;
                laser1FXRight.Play();
                break;
        }
        var hitDetection = Instantiate(laser1, shootFromBarrel, leftBarrel.transform.rotation);
        hitDetection.Fire(shootFromBarrel, leftBarrel.transform.rotation, laser1Force, l1Spread);
        UseAmmo(1);
    }

    void FireLaser2()
    {
        laser2FX.Play();
        var hitDetection = Instantiate(laser2, mainBarrel.transform.position + transform.forward * 6f, mainBarrel.transform.rotation);
        hitDetection.Fire(mainBarrel.transform.position + transform.forward * 6f, mainBarrel.transform.rotation, laser2Force, l2Spread);
        UseAmmo(2);
    }
    void FireLaser3()
    {
        laser3FX.Play();
        var hitDetection = Instantiate(laser3, mainBarrel3.transform.position + transform.forward * 2f, mainBarrel3.transform.rotation);
        hitDetection.Fire(mainBarrel3.transform.position + transform.forward * 2f, mainBarrel3.transform.rotation, laser3Force, l3Spread);
        UseAmmo(3);
    }

    void FireLaser4()
    {
        laser4FX.Play();
        for (int i = 0; i <= 10; i++)
        {
            Vector3 position = new Vector3(mainBarrel2.transform.position.x + transform.forward.x * 6f + Random.Range(0, .04f), mainBarrel2.transform.position.y + transform.forward.y * 6f + Random.Range(0, .04f), mainBarrel2.transform.position.z + transform.forward.z * 6f + Random.Range(0, .04f));
            var hitDetection = Instantiate(laser4, position, mainBarrel2.transform.rotation);
            hitDetection.Fire(mainBarrel2.transform.position + transform.forward * 6f, mainBarrel2.transform.rotation, laser4Force, l4Spread);
        }
        UseAmmo(4);
    }

    void UseAmmo(int currentWeapon)
    {
        if (infiniteAmmo == false)
        {
            switch (currentWeapon)
            {
                case 1:
                    laser1Ammo--;
                    break;
                case 2:
                    laser2Ammo--;
                    break;
                case 3:
                    laser3Ammo--;
                    break;
                case 4:
                    laser4Ammo--;
                    break;

            }
            currentWeaponAmmo--;
        }
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