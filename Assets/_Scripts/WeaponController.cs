using StarterAssets;

using System.Collections;
using UnityEditor.PackageManager.Requests;

using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public StarterAssetsInputs inputs;


    private Camera mainCamera;
    Animator pistolAnim;
    public GameObject bulletPrefab;
    public Transform firePosition;
    
    public float bulletSpeed;
    public float pistolMaxAmmo;
    public float pistolCurrentAmmo;
    public float pistolRange;
    public float pistolShootSpeed;
    
    float timeToShoot = 0f;
    int shootHash;



    void Start()
    {
        inputs = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssetsInputs>();

        pistolAnim = GameObject.FindWithTag("Gun").GetComponent<Animator>();
        shootHash = Animator.StringToHash("Shoot");
        mainCamera = Camera.main;
        pistolCurrentAmmo = pistolMaxAmmo;

    }
    private void Update()
    {
        timeToShoot += Time.deltaTime;


        var shoot = inputs.shoot;
        if (shoot && timeToShoot > pistolShootSpeed)
        {
            if(pistolCurrentAmmo > 0f)
            {
                Shoot();
                pistolCurrentAmmo--;
                timeToShoot = 0;
                inputs.ShootInput(false);
            }
            else
                StartCoroutine(ReloadAmmo());
        }    
        if(Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadAmmo());
        }    
    }
    public void Shoot()
    {
        pistolAnim.SetTrigger(shootHash);

        RaycastHit hit; //Lấy thông tin về vật thể mà raycast chạm vào
        var hitPosition = Camera.main.transform.position + Camera.main.transform.forward * pistolRange;
        Debug.Log(hitPosition.x + " " + hitPosition.z);
        var shootDirection = hitPosition - firePosition.position;

        if(Physics.Raycast(firePosition.position, shootDirection, out hit, pistolRange))
        {
            Debug.Log(hit.transform.name);
        }
        else
        {
            Debug.Log("Không chạm vào vật thể nào");
        }

        GameObject bullet = Instantiate(bulletPrefab, firePosition.position, Quaternion.identity);

        bullet.transform.forward = shootDirection;




    }
    void OnDrawGizmos()
    {
        if (firePosition != null && mainCamera != null)
        {
            Gizmos.color = Color.red;


            // Lấy hướng bắn từ camera

            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 100f))
            {

                targetPoint = hit.point; 
            }
            else
            {
                targetPoint = ray.GetPoint(100f); 
            }

            Gizmos.DrawLine(firePosition.position, targetPoint);
        }
    }
    IEnumerator ReloadAmmo()
    {
        yield return new WaitForSeconds(1f);
        pistolCurrentAmmo = pistolMaxAmmo;
    }    

}
