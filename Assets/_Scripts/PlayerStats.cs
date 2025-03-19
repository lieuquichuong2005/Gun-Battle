using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private int currentHealth;
    private int maxHealth = 100;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            DieManager();
        }
    }
    void DieManager()
    {
        Debug.Log("Player died");
    }
}
