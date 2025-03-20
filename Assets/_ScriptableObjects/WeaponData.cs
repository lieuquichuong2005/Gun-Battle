using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float fireRate;
    public float range;
    public int maxAmmo;
    public float bulletSpeed;
    public float damage;
    public GameObject ammoType;
}
