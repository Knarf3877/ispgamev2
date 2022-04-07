using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class HitDetection : MonoBehaviourPunCallbacks
{
    [SerializeField] ParticleSystem explodeFX, flashFX;
    public float lifeTime = 4f;
    [SerializeField] LayerMask hitMask = -1;

    [SerializeField] float laserForce, laserSpread;
    private Vector3 velocity = Vector3.forward;
    private float destructionTime = 0f;
    bool isFired = false;
    private const float kVelocityMult = 1f;
    public float damage = 10f;


    [SerializeField] private bool adjustLaser;

    void Start()
    {
        if (adjustLaser)
        {
            transform.Rotate(Vector3.left * 90);
        }
    }
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // e.g. store this gameobject as this player's charater in Player.TagObject

        Debug.Log(info);
    }

    void Update()
    {
        if (isFired == false)
            return;
        if (Time.time > destructionTime)
            DestroyBullet(transform.position, false);
    }
    void FixedUpdate()
    {
        if (isFired == false)
            Fire(transform.position, transform.rotation, laserForce, laserSpread);


        MoveBullet(damage, name);

    }

    void Fire(Vector3 position, Quaternion rotation, float muzzleVelocity, float spread)
    {
        ParticleSystem tempFX = Instantiate(flashFX, transform.position, transform.rotation);
        Destroy(tempFX.gameObject, 1f);
        transform.position = position + transform.forward * 6f;
        Vector3 spreadAngle = Vector3.zero;
        spreadAngle.x = Random.Range(-spread, spread);
        spreadAngle.y = Random.Range(-spread, spread);
        Quaternion deviationRotation = Quaternion.Euler(spreadAngle);

        transform.rotation = rotation * deviationRotation;

        velocity = (transform.forward * muzzleVelocity);
        destructionTime = Time.time + lifeTime;
        isFired = true;


    }

    public void MoveBullet(float damage, string name)
    {
        RaycastHit rayHit;
        Ray velocityRay = new Ray(transform.position, velocity.normalized);
        bool rayHasHit = Physics.Raycast(velocityRay, out rayHit, velocity.magnitude * kVelocityMult * Time.deltaTime, hitMask);

        if (rayHasHit == true)
        {

            GameObject rayHitGameObject = rayHit.transform.gameObject;
            PlayerController target = rayHitGameObject.GetComponent<PlayerController>();
            //  bool hitSelf = false;
            if (target)
            {
                print("Hit " + target.name + " with " + target.currentHealth + " HP left.");
                target.TakeDamage(damage);
            }

            DestroyBullet(rayHit.point, true);
        }
        else
        {
            // Bullet didn't hit anything, continue moving.
            transform.Translate(velocity * Time.deltaTime, Space.World);


        }

    }
    public void DestroyBullet(Vector3 position, bool fromImpact)
    {
        if (fromImpact == true && explodeFX != null)
        {
            ParticleSystem tempExplodeFX = Instantiate(explodeFX, transform.position, transform.rotation);
            Destroy(tempExplodeFX.gameObject, 1f);

        }

        Destroy(gameObject);
    }
    /*    private void OnCollisionEnter(Collision collision)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 contactPoint = contact.point;
            Destroy(gameObject);
            Debug.Log("Laser Hit");
            GameObject smallExplosion = Instantiate(explodeFX, contactPoint, this.transform.rotation * Quaternion.Euler(0,-90,0));
            Destroy(smallExplosion, 5f);
        }*/

}