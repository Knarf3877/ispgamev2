﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] Image healthbarImage, shieldbarImage;
    [SerializeField] GameObject ui, pauseMenu, sfx, scoreText, weaponText, deathText, livesTest, fuelbarImage, throttlebarImage;
    [SerializeField] GameObject mainEngine, mainEngineInput, reverseEngine, reverseEngineInput, warpEngine;
    [SerializeField] GameObject cameraHolder;

    AudioSource warpSound;
    float warpMaxVolume = 0.4f;
    bool isPaused = false;

    [SerializeField] GameObject leftBarrel, rightBarrel, mainBarrel, mainBarrel2, mainBarrel3;
    Vector3 shootFromBarrel;
    int shootOrder;
    static public int currentWeapon;
    static public int currentWeaponAmmo;
    private float timeLastFired;
    bool canFire = true;

    static public int laser1Ammo = 100;
    static public int laser2Ammo = 50;
    static public int laser3Ammo = 200;
    static public int laser4Ammo = 50;
    public int defaultLaser1Ammo;
    public int defaultLaser2Ammo;
    public int defaultLaser3Ammo;
    public int defaultLaser4Ammo;

    public bool infiniteAmmo = false;

    ParticleSystem laser1FXLeft;
    ParticleSystem laser1FXRight;
    float laser1FireRate = .2f;

    ParticleSystem laser2FX;
    float laser2FireRate = .4f;

    private ParticleSystem laser3FX;
    float laser3FireRate = .12f;

    ParticleSystem laser4FX;
    float laser4FireRate = .5f;


    static public int deaths = 0;

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
        AudioListener.pause = false;
        isPaused = false;
        if (PV.IsMine)
        {

        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);

            Destroy(rb);
            Destroy(ui);
            Destroy(pauseMenu);
            Destroy(sfx);
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
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        canFire = true;
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
        if (canFire)
            FireWeapon();
        if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false)
        {
            PauseGame();
            Debug.Log("game paused");
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused != false)
        {
            ResumeGame();
        }

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

    public void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseMenu.SetActive(true);
        ui.SetActive(false);
        isPaused = true;
        AudioListener.pause = true;
        rb.freezeRotation = true;
        canFire = false;
    }
    public void ResumeGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        pauseMenu.SetActive(false);
        ui.SetActive(true);
        isPaused = false;
        AudioListener.pause = false;
        Debug.Log("Resue ");
        rb.freezeRotation = false;
        canFire = true;
    }

    public void QuitGame()
    {
        pauseMenu.SetActive(false);
        ui.SetActive(true);
        isPaused = false;
        rb.freezeRotation = false;
        AudioListener.pause = false;
        canFire = true;
        Debug.Log("Quit");
        StartCoroutine(Disconnect());
    }

    IEnumerator Disconnect()
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.InRoom)
            yield return null;
        SceneManager.LoadScene(0);
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
    public void ApplyDamage(float damage, string name)
    {

        if (isImmune == false)
        {
            if (currentShield >= damage)
            {
                currentShield -= damage;
            }
            else
            {
                currentShield -= damage;
                currentHealth -= Mathf.Abs(currentShield);
                currentShield = 0;
            }

        }

        if (currentHealth <= 0)
        {
            playerManager.Die();
            PV.RPC("RPC_TrackKills", RpcTarget.Others, name);
        }
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
            playerManager.Die();
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
            playerManager.Die();

        }
    }
    void updateHUD()
    {
        scoreText.GetComponent<Text>().text = totalSpeed.ToString("F0") + " mph" +
            //   "\n" + avgFrameRate + " fps" +
            "\n" + warpFuel.ToString("F0") + " : fuel left" +
            "\n" + throttle.ToString("F3") 
        ;
        weaponText.GetComponent<TextMeshProUGUI>().text = WeaponName(currentWeapon) +
           "\n" + currentWeaponAmmo.ToString("F0");
        deathText.GetComponent<TextMeshProUGUI>().text = "Deaths: " + deaths.ToString("F0");
        healthbarImage.fillAmount = currentHealth / 100;
        shieldbarImage.fillAmount = currentShield / 100;
        throttlebarImage.GetComponent<Renderer>().material.SetFloat("_FillAmount", Mathf.Abs(throttle));
        fuelbarImage.GetComponent<Renderer>().material.SetFloat("_FillAmount", warpFuel / 200);
    }
    string WeaponName(int weaponNum)
    {
        switch (weaponNum)
        {
            case 1:
                return "Type 1";

            case 2:
                return "Type 2";

            case 3:
                return "Type 3";

            case 4:
                return "Type 4";

            default:
                return "weapon not found";

        }
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

        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Laser1"), shootFromBarrel, leftBarrel.transform.rotation);
        UseAmmo(1);
    }
    void FireLaser2()
    {
        laser2FX.Play();
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Laser2"), mainBarrel.transform.position + transform.forward * 6f, mainBarrel.transform.rotation);
        UseAmmo(2);
    }
    void FireLaser3()
    {
        laser3FX.Play();
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Bullet1"), mainBarrel3.transform.position + transform.forward * 2f, mainBarrel3.transform.rotation);
        UseAmmo(3);
    }
    void FireLaser4()
    {
        laser4FX.Play();
        for (int i = 0; i <= 8; i++)
        {
            Vector3 position = new Vector3(mainBarrel2.transform.position.x + transform.forward.x * 6f + Random.Range(0, .04f), mainBarrel2.transform.position.y + transform.forward.y * 6f + Random.Range(0, .04f), mainBarrel2.transform.position.z + transform.forward.z * 6f + Random.Range(0, .04f));
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Laser3"), position, mainBarrel2.transform.rotation);
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