using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float fireRate;
    public int range;
    public int maxAmmo;
    public int bulletSpeed;
    public int damage;
    public float reloadTime;
    public NetworkPrefabRef ammoType;
    public AudioClip gunShot;
    public AudioClip gunReload;
    public GameObject firePosition;
}
/*using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    
    [Header("Fire Settings")]
    [Tooltip("Số viên bắn mỗi giây (rounds per second)")]
    public float fireRate = 1f;
    
    [Tooltip("Chế độ bắn: 0=Bán tự động, 1=Tự động, 2=Burst")]
    public FireMode fireMode = FireMode.SemiAuto;
    
    [Tooltip("Số viên trong 1 burst (chỉ áp dụng cho chế độ burst)")]
    public int burstCount = 3;
    
    [Tooltip("Độ trễ giữa các viên trong burst (giây)")]
    public float burstDelay = 0.1f;
    
    [Header("Range & Damage")]
    [Tooltip("Tầm bắn tối đa (mét)")]
    public int range = 100;
    
    [Tooltip("Sát thương mỗi viên")]
    public int damage = 25;
    
    [Header("Ammo Settings")]
    [Tooltip("Số đạn tối đa trong băng")]
    public int maxAmmo = 30;
    
    [Tooltip("Tốc độ đạn (m/s)")]
    public int bulletSpeed = 500;
    
    [Tooltip("Thời gian nạp đạn (giây)")]
    public float reloadTime = 2f;
    
    [Header("Prefab & Audio")]
    public NetworkPrefabRef ammoType;
    public AudioClip gunShot;
    public AudioClip gunReload;
    
    [Header("Accuracy")]
    [Tooltip("Độ lệch tối đa khi bắn (độ)")]
    public float spread = 0f;
    
    [Tooltip("Độ giật súng theo trục Y")]
    public float recoilY = 1f;
    
    [Tooltip("Độ giật súng theo trục X")]
    public float recoilX = 0.5f;
}

public enum FireMode
{
    SemiAuto,   // Bán tự động - phải nhấn mỗi lần bắn
    FullAuto,   // Tự động - giữ chuột để bắn liên tục
    Burst       // Burst - mỗi lần nhấn bắn nhiều viên
}*/
