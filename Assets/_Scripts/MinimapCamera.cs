using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform player; 
    public float height = 20f; 
    public Vector2 mapBoundsMin; 
    public Vector2 mapBoundsMax; 

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.y = height; 

            newPosition.x = Mathf.Clamp(newPosition.x, mapBoundsMin.x, mapBoundsMax.x);
            newPosition.z = Mathf.Clamp(newPosition.z, mapBoundsMin.y, mapBoundsMax.y);

            this.transform.position = newPosition;
        }
    }
}