using StarterAssets;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public StarterAssetsInputs inputs;

    public GameObject bulletPrefab;
    public Transform firePosition;
    public float bulletSpeed;

    private Camera mainCamera;

    void Start()
    {
        inputs = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssetsInputs>();
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
        if (mainCamera == null)
        {
            Debug.LogError("Camera chính chưa được gán hoặc không có Tag 'MainCamera'.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Vector3 targetPoint = ray.GetPoint(100f);
        Vector3 shootDirection = (targetPoint - firePosition.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePosition.position, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false; // Đảm bảo Rigidbody hoạt động đúng với vật lý
            rb.useGravity = false;  // Tắt trọng lực để đạn không bị rơi xuống
            rb.linearVelocity = shootDirection * bulletSpeed * Time.deltaTime;
        }

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
                targetPoint = hit.point; // Nếu chạm, lấy điểm chạm
            }
            else
            {
                targetPoint = ray.GetPoint(100f); // Nếu không chạm, lấy điểm xa trên ray
            }

            // Vẽ đường từ firePosition đến targetPoint
            Gizmos.DrawLine(firePosition.position, targetPoint);
        }
    }
}
