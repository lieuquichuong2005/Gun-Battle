using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletController : MonoBehaviour
{
    public WeaponController weaponController;
    public int pistolDamage = 10;

    Rigidbody rb;
    private void Start()
    {
        weaponController = GameObject.FindWithTag("WeaponController").GetComponent<WeaponController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = transform.forward * weaponController.bulletSpeed * Time.deltaTime;
        }
        Destroy(this.gameObject, 15f);
    }
    // Ki?m tra va ch?m v?i k? ??ch
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<PlayerStats>().TakeDamage(pistolDamage);
            Destroy(this.gameObject);
        }
    }
}
