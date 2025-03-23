using StarterAssets;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponData[] weaponDataArray; // Mảng chứa tất cả vũ khí
    public Transform[] firePosition;

    [Header("References")]
    public StarterAssetsInputs inputs;
    private AudioSource audioSource;
    private Camera mainCamera;
    private Animator weaponAnimator;

    private int currentWeaponIndex = 0;
    private float currentAmmo;
    private float timeToShoot = 0f;

    private static readonly int shootHash = Animator.StringToHash("Shoot");

    void Start()
    {
        inputs = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssetsInputs>();
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        EquipWeapon(0);  // Trang bị vũ khí đầu tiên
    }

    void Update()
    {
        timeToShoot += Time.deltaTime;

        // Bắn súng
        if (inputs.shoot && timeToShoot >= 1f / weaponDataArray[currentWeaponIndex].fireRate)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                currentAmmo--;
                timeToShoot = 0;
                inputs.ShootInput(false);
            }
            else
            {
                StartCoroutine(ReloadAmmo());
            }
        }

        // Nạp đạn
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadAmmo());
        }

        // Đổi súng bằng phím số
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponDataArray.Length) return;

        currentWeaponIndex = index;
        currentAmmo = weaponDataArray[currentWeaponIndex].maxAmmo;
        Debug.Log($"Đã trang bị: {weaponDataArray[currentWeaponIndex].weaponName}");
    }

    void Shoot()
    {
        weaponAnimator.SetTrigger(shootHash);

        if (weaponDataArray[currentWeaponIndex].gunShot != null)
        {
            audioSource.PlayOneShot(weaponDataArray[currentWeaponIndex].gunShot);
        }

        RaycastHit hit;
        var hitPosition = Camera.main.transform.position + Camera.main.transform.forward * weaponDataArray[currentWeaponIndex].range;
        var shootDirection = hitPosition - firePosition[currentWeaponIndex].position;

        if (Physics.Raycast(mainCamera.transform.position, shootDirection, out hit, weaponDataArray[currentWeaponIndex].range))
        {
            Debug.Log($"Trúng: {hit.transform.name}");

            var targetHealth = hit.transform.GetComponent<PlayerStats>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(weaponDataArray[currentWeaponIndex].damage);
            }
        }

        GameObject bullet = Instantiate(weaponDataArray[currentWeaponIndex].ammoType, firePosition[currentWeaponIndex].position, Quaternion.identity);
        bullet.transform.forward = shootDirection * weaponDataArray[currentWeaponIndex].bulletSpeed;
    }

    IEnumerator ReloadAmmo()
    {
        Debug.Log("Đang nạp đạn...");

        if (weaponDataArray[currentWeaponIndex].gunReload != null)
        {
            audioSource.PlayOneShot(weaponDataArray[currentWeaponIndex].gunReload);
        }

        yield return new WaitForSeconds(weaponDataArray[currentWeaponIndex].reloadTime);
        currentAmmo = weaponDataArray[currentWeaponIndex].maxAmmo;

        Debug.Log($"{weaponDataArray[currentWeaponIndex].weaponName} đã nạp đầy đạn ({currentAmmo}/{weaponDataArray[currentWeaponIndex].maxAmmo})");
    }
}
