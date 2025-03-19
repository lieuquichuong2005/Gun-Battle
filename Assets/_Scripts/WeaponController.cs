using StarterAssets;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public StarterAssetsInputs inputs;

    private Camera mainCamera;
    Animator pistolAnim;
    public GameObject bulletPrefab;
    public Transform firePosition;
    
    public float bulletSpeed;
    public float pistolMaxAmo;
    public float pistolCurrentAmo;
    int shootHash;


    void Start()
    {
        inputs = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssetsInputs>();
        pistolAnim = GameObject.FindWithTag("Pistol").GetComponent<Animator>();
        shootHash = Animator.StringToHash("Shoot");
        mainCamera = Camera.main;

    }
    private void Update()
    {
        var shoot = inputs.shoot;
        if (!shoot) return;

        Debug.Log("Shooting");
        Shoot();
        inputs.ShootInput(false);
    }
    public void Shoot()
    {
        pistolAnim.SetTrigger(shootHash);

        RaycastHit hit; //Lấy thông tin về vật thể mà raycast chạm vào
        if(Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, 100f))
        {
            Debug.Log(hit.transform.name);
        }
        else
        {
            Debug.Log("Không chạm vào vật thể nào");
        }

        GameObject bullet = Instantiate(bulletPrefab, firePosition.position, Quaternion.identity);

        bullet.transform.forward = firePosition.forward;



    }
    void OnDrawGizmos()
    {
        if (firePosition != null && mainCamera != null)
        {
            Gizmos.color = Color.red;

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

}
