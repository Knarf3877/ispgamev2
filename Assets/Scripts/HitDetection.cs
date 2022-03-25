using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetection : MonoBehaviour
{
    public ParticleSystem explodeFX;
    public float lifeTime = 4f;
    private bool useFixedUpdate = true;
    [SerializeField] private LayerMask hitMask = -1;

    private Vector3 velocity = Vector3.forward;
    private float destructionTime = 0f;
    private bool isFired = false;
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
            return;

        if (useFixedUpdate == true)
        {
            MoveBullet(damage);
        }
    }

    public void Fire(Vector3 position, Quaternion rotation, float muzzleVelocity, float spread)
    {
        transform.position = position;
        Vector3 spreadAngle = Vector3.zero;
        spreadAngle.x = Random.Range(-spread, spread);
        spreadAngle.y = Random.Range(-spread, spread);
        Quaternion deviationRotation = Quaternion.Euler(spreadAngle);

        transform.rotation = rotation * deviationRotation;

        velocity = (transform.forward * muzzleVelocity);
        destructionTime = Time.time + lifeTime;
        isFired = true;

    }

    public void MoveBullet(float damage)
    {
        // Perform the raycast. Shoots a ray forwards of the bullet that covers all the distance
        // that it will cover in this frame. This guarantees a hit in all but the most extenuating
        // circumstances (against other extremely fast and small moving targets it may miss) and
        // works at practically any bullet speed.
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
                target.ApplyDamage(damage);
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