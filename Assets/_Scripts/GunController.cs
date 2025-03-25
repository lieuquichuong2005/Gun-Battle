using StarterAssets;
using System.Collections;
using TMPro;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponData[] weaponDataArray;
    public GameObject[] weaponsModel;
    public Transform[] firePoint;

    [Header("References")]
    public StarterAssetsInputs inputs;
    private AudioSource audioSource;
    private Camera mainCamera;
    private Animator weaponAnimator;

    public int currentWeaponIndex = 0;
    public TMP_Text ammoMount;
    private float currentAmmo;
    private float timeToShoot = 0f;

    private static readonly int shootHash = Animator.StringToHash("Shoot");

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        EquipWeapon(0);
        UpdateAmmoUI();
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
        ChangeWeapon(index);
    }

    void Shoot()
    {
        weaponAnimator?.SetTrigger(shootHash);

        if (weaponDataArray[currentWeaponIndex].gunShot != null)
        {
            audioSource.PlayOneShot(weaponDataArray[currentWeaponIndex].gunShot);
        }

        RaycastHit hit; //Lấy thông tin về vật thể mà raycast chạm vào
        var hitPosition = Camera.main.transform.position + Camera.main.transform.forward * weaponDataArray[currentWeaponIndex].range;
        var shootDirection = hitPosition - firePoint[currentWeaponIndex].position;

        if (Physics.Raycast(firePoint[currentWeaponIndex].position, shootDirection, out hit, weaponDataArray[currentWeaponIndex].range))
        {
            Debug.Log($"Trúng: {hit.transform.name}");

            var targetHealth = hit.transform.GetComponent<PlayerStats>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(weaponDataArray[currentWeaponIndex].damage);
            }
        }

        GameObject bullet = Instantiate(weaponDataArray[currentWeaponIndex].ammoType, firePoint[currentWeaponIndex].position, firePoint[currentWeaponIndex].rotation);
        bullet.transform.forward = shootDirection;
        UpdateAmmoUI();
    }

    IEnumerator ReloadAmmo()
    {

        if (weaponDataArray[currentWeaponIndex].gunReload != null)
        {
            audioSource.PlayOneShot(weaponDataArray[currentWeaponIndex].gunReload);
        }

        yield return new WaitForSeconds(weaponDataArray[currentWeaponIndex].reloadTime);
        currentAmmo = weaponDataArray[currentWeaponIndex].maxAmmo;

        UpdateAmmoUI();
    }
    void UpdateAmmoUI()
    {
        ammoMount.text = currentAmmo + "/" + weaponDataArray[currentWeaponIndex].maxAmmo;
    }
    void ChangeWeapon(int index)
    {
        foreach(GameObject weapon in weaponsModel)
        {
            weapon.gameObject.SetActive(false);
        }
        weaponsModel[index].gameObject.SetActive(true);
    }
}