using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] Slider healthBar;
    [SerializeField] Slider staminaBar;
    
    private int currentHealth;
    private int maxHealth = 100;

    private int currentStamina;
    private int maxStamina = 50;

    void Start()
    {
        healthBar = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
        staminaBar = GameObject.FindWithTag("StaminaBar").GetComponent<Slider>();

        healthBar.maxValue = maxHealth;
        staminaBar.maxValue = maxStamina;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        healthBar.value = currentHealth;
        staminaBar.value = currentStamina;
    }

    void Update()
    {
        
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateUI();
        if (currentHealth <= 0)
        {
            DieManager();
        }
    }
    public void RestoreHealth(int health)
    {
        currentHealth += health;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateUI();
    }
    public void RestoreStamina(int stamina)
    {
        currentStamina += stamina;
        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
        UpdateUI();
    }
    void DieManager()
    {
        Debug.Log("Player died");
    }
    void UpdateUI()
    {
        healthBar.value = currentHealth;
        staminaBar.value = currentStamina;
    }
}
