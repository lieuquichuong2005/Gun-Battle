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
    public GameObject ammoType;
    public AudioClip gunShot;
    public AudioClip gunReload;
}
