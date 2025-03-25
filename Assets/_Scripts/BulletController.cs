using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletController : MonoBehaviour
{

    public ParticleSystem bulletExplosion;

    public GunController gunController;

    Rigidbody rb;
    private void Start()
    {
        gunController = GameObject.FindWithTag("WeaponController").GetComponent<GunController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = transform.forward * gunController.weaponDataArray[gunController.currentWeaponIndex].bulletSpeed * Time.deltaTime;
        }
        Destroy(this.gameObject, 15f);
    }
    void OnCollisionEnter(Collision collision)
    {
        bulletExplosion.Play();
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //collision.gameObject.GetComponent<PlayerStats>()?.TakeDamage();

        }
        Destroy(this.gameObject, 0.1f);
    }

}
