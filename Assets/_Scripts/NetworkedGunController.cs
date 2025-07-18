using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkedGunController : NetworkBehaviour
{
    [Header("Weapon Settings")]
    public WeaponData[] weaponDataArray;
    public GameObject[] weaponsModel;
    public Transform[] firePoint;

    [Header("References")]
    private AudioSource audioSource;
    public Camera playerCamera;
    public TMP_Text ammoMount;

    private float idleTimer = 0f;
    private const float idleThreshold = 8f;
    private Animator currentGunAnimator;

    public GameObject[] gunSlash;
    public GameObject[] ammoInMagzine;

    // Network Properties
    [Networked] public int CurrentWeaponIndex { get; set; } = 0;
    [Networked] public float CurrentAmmo { get; set; }
    [Networked] public TickTimer ShootTimer { get; set; }
    [Networked] public TickTimer ReloadTimer { get; set; }
    [Networked] public bool IsReloading { get; set; }
    [Networked] private bool IsShootingAnim { get; set; }
    [Networked] private TickTimer ShootAnimTimer { get; set; }

    [Networked] private bool IsCheckingGun { get; set; }
    [Networked] private TickTimer CheckGunAnimTimer { get; set; }


    // ✅ THÊM: Track weapon index để detect changes
    private int previousWeaponIndex = -1;

    // Bullet prefab for networking
    public NetworkPrefabRef bulletPrefab;

    public override void Spawned()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (Object.HasInputAuthority)
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        EquipWeapon(0);
        UpdateAmmoUI();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<NetworkInputData>(out var input))
        {
            HandleWeaponSwitching(input);
            HandleShooting(input);
            UpdateGunAnimation(input);
            HandleReloading(input);
        }

        // Handle reload timer
        if (IsReloading && ReloadTimer.ExpiredOrNotRunning(Runner))
        {
            CompleteReload();
        }
        if (ShootAnimTimer.ExpiredOrNotRunning(Runner)) IsShootingAnim = false;
        if (CheckGunAnimTimer.ExpiredOrNotRunning(Runner)) IsCheckingGun = false;

    }

    public override void Render()
    {
        UpdateAmmoUI();

        if (previousWeaponIndex != CurrentWeaponIndex)
        {
            ChangeWeaponModel(CurrentWeaponIndex);
            previousWeaponIndex = CurrentWeaponIndex;
        }

        // Trigger animation dựa trên biến đã sync
        if (currentGunAnimator == null && weaponsModel[CurrentWeaponIndex] != null)
        {
            currentGunAnimator = weaponsModel[CurrentWeaponIndex].GetComponent<Animator>();
        }

        if (currentGunAnimator != null)
        {
            if (IsShootingAnim)
            {
                currentGunAnimator.SetTrigger("Shoot");
            }
            if (IsCheckingGun)
            {
                currentGunAnimator.SetTrigger("Check");
            }
        }
    }


    void HandleWeaponSwitching(NetworkInputData input)
    {
        if (input.weaponSwitchIndex >= 0 && input.weaponSwitchIndex < weaponDataArray.Length)
        {
            if (input.weaponSwitchIndex != CurrentWeaponIndex && !IsReloading)
            {
                EquipWeapon(input.weaponSwitchIndex);
            }
        }

        if (input.switchToNextWeapon && !IsReloading)
        {
            int nextWeaponIndex = (CurrentWeaponIndex + 1) % weaponDataArray.Length;
            EquipWeapon(nextWeaponIndex);
        }
    }

    void HandleShooting(NetworkInputData input)
    {
        if (input.isShooting && CanShoot())
        {
            Shoot();
            IsShootingAnim = true;
            ShootAnimTimer = TickTimer.CreateFromSeconds(Runner, 0.2f);
        }
    }

    void HandleReloading(NetworkInputData input)
    {
        if (input.isReloading && CanReload())
        {
            Debug.Log("Reloading weapon...");
            StartReload();
        }
    }

    bool CanShoot()
    {
        return CurrentAmmo > 0 &&
               !IsReloading &&
               ShootTimer.ExpiredOrNotRunning(Runner);
    }

    bool CanReload()
    {
        return !IsReloading &&
               CurrentAmmo < weaponDataArray[CurrentWeaponIndex].maxAmmo;
    }

    void Shoot()
    {
        CurrentAmmo--;
        ShootTimer = TickTimer.CreateFromSeconds(Runner, 1f / weaponDataArray[CurrentWeaponIndex].fireRate);

        // Play effects cho tất cả clients
        RPC_PlayShootEffects();

        if (Object.HasInputAuthority)
        {
            RPC_ShowGunSlash();
        }

        // CHỈ SERVER SPAWN BULLET
        if (Object.HasStateAuthority)
        {
            var shootDir = GetShootDirection();
            SpawnBullet(shootDir);
        }

        if (CurrentAmmo <= 0)
        {
            ammoInMagzine[CurrentWeaponIndex]?.SetActive(false);
        }
    }

    void SpawnBullet(Vector3 direction)
    {
        var bulletPrefab = weaponDataArray[CurrentWeaponIndex].ammoType;

        // Spawn với NULL authority để server quản lý
        var bulletObj = Runner.Spawn(
            bulletPrefab,
            firePoint[CurrentWeaponIndex].position,
            Quaternion.LookRotation(direction),
            null  // NULL authority = server authority
        );

        if (bulletObj.TryGetComponent<NetworkedBullet>(out var bullet))
        {
            bullet.Initialize(direction, weaponDataArray[CurrentWeaponIndex], Object.InputAuthority);
        }
    }

    Vector3 GetShootDirection()
    {
        if (playerCamera != null)
        {
            var hitPosition = playerCamera.transform.position +
                             playerCamera.transform.forward * weaponDataArray[CurrentWeaponIndex].range;
            return (hitPosition - firePoint[CurrentWeaponIndex].position).normalized;
        }
        return firePoint[CurrentWeaponIndex].forward;
    }

    void StartReload()
    {
        IsReloading = true;
        ReloadTimer = TickTimer.CreateFromSeconds(Runner, weaponDataArray[CurrentWeaponIndex].reloadTime);
        RPC_PlayReloadEffects();
    }

    void CompleteReload()
    {
        IsReloading = false;
        CurrentAmmo = weaponDataArray[CurrentWeaponIndex].maxAmmo;
        ammoInMagzine[CurrentWeaponIndex]?.SetActive(true);
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponDataArray.Length) return;

        CurrentWeaponIndex = index;
        CurrentAmmo = weaponDataArray[CurrentWeaponIndex].maxAmmo;

        // ✅ FIX: Không gọi ChangeWeaponModel ở đây nữa
        // Để Render() handle việc này cho tất cả clients
    }

    // ✅ FIX: Method này sẽ được gọi từ Render() cho tất cả clients
    void ChangeWeaponModel(int index)
    {
        // Tắt tất cả weapons
        for (int i = 0; i < weaponsModel.Length; i++)
            if (weaponsModel[i] != null) weaponsModel[i].SetActive(false);

        // Bật weapon hiện tại
        if (weaponsModel[index] != null)
        {
            weaponsModel[index].SetActive(true);
            currentGunAnimator = weaponsModel[index].GetComponent<Animator>(); // ✅ Cập nhật animator
        }

        Debug.Log($"[{Object.InputAuthority}] Changed weapon model to index: {index}");
    }

    void UpdateAmmoUI()
    {
        if (Object.HasInputAuthority && ammoMount != null)
        {
            ammoMount.text = $"{CurrentAmmo}/{weaponDataArray[CurrentWeaponIndex].maxAmmo}";
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_PlayShootEffects()
    {
        if (weaponDataArray[CurrentWeaponIndex].gunShot != null)
        {
            audioSource.PlayOneShot(weaponDataArray[CurrentWeaponIndex].gunShot);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_PlayReloadEffects()
    {
        if (weaponDataArray[CurrentWeaponIndex].gunReload != null)
        {
            audioSource.PlayOneShot(weaponDataArray[CurrentWeaponIndex].gunReload);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_ShowGunSlash()
    {
        // Hiển thị effect
        if (gunSlash[CurrentWeaponIndex] != null)
        {
            gunSlash[CurrentWeaponIndex].SetActive(true);

            // Tắt sau 0.1s
            StartCoroutine(HideGunSlash(CurrentWeaponIndex));
        }
    }

    IEnumerator HideGunSlash(int index)
    {
        yield return new WaitForSeconds(0.1f);
        if (index < gunSlash.Length && gunSlash[index] != null)
        {
            gunSlash[index].SetActive(false);
        }
    }

    void UpdateGunAnimation(NetworkInputData input)
    {
        // Lấy Animator từ vũ khí đang dùng
        GameObject currentGun = weaponsModel[CurrentWeaponIndex];
        if (currentGun == null) return;

        if (currentGunAnimator == null)
            currentGunAnimator = currentGun.GetComponent<Animator>();

        if (input.moveDirection.magnitude > 0.1f || input.isShooting)
        {
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Runner.DeltaTime;
        }

        // Trigger animation bằng Networked biến (chỉ trên máy chủ)
        if (Object.HasStateAuthority)
        {
            if (idleTimer >= idleThreshold)
            {
                IsCheckingGun = true;
                CheckGunAnimTimer = TickTimer.CreateFromSeconds(Runner, 1f);
                idleTimer = 0f;
            }
        }

    }

}