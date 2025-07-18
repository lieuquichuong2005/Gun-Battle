using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedPlayerStats : NetworkBehaviour
{
    [Header("UI References")]
    //public GameObject playerStatCanvas;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;

    [Header("Stats Settings")]
    [Networked] public int maxHealth { get; private set; } = 100;
    [Networked] public int maxStamina { get; private set; } = 50;
    public float staminaRegenRate = 5f;
    public float healthRegenRate = 1f;
    public bool canRegenerateHealth = false;

    // Network Properties
    [Networked] public int CurrentHealth { get; set; }
    [Networked] public int CurrentStamina { get; set; }
    [Networked] public bool IsDead { get; set; }
    [Networked] public TickTimer StaminaRegenTimer { get; set; }
    [Networked] public TickTimer HealthRegenTimer { get; set; }

    public override void Spawned()
    {
        // Initialize stats
        CurrentHealth = maxHealth;
        CurrentStamina = maxStamina;
        IsDead = false;

        // Find UI elements for local player only
        if (Object.HasInputAuthority)
        {
            FindUIElements();
            UpdateUI();
            //playerStatCanvas.SetActive(true);
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        HandleStaminaRegeneration();

        if (canRegenerateHealth)
        {
            HandleHealthRegeneration();
        }
    }

    public override void Render()
    {
        // Update UI for local player
        if (Object.HasInputAuthority)
        {
            UpdateUI();
        }
    }

    void FindUIElements()
    {
        if (healthBar == null)
        {
            GameObject healthBarObj = GameObject.FindWithTag("HealthBar");
            if (healthBarObj != null)
                healthBar = healthBarObj.GetComponent<Slider>();
        }

        if (staminaBar == null)
        {
            GameObject staminaBarObj = GameObject.FindWithTag("StaminaBar");
            if (staminaBarObj != null)
                staminaBar = staminaBarObj.GetComponent<Slider>();
        }

        // Setup UI max values
        if (healthBar != null)
            healthBar.maxValue = maxHealth;
        if (staminaBar != null)
            staminaBar.maxValue = maxStamina;
    }

    void HandleStaminaRegeneration()
    {
        if (CurrentStamina < maxStamina && StaminaRegenTimer.ExpiredOrNotRunning(Runner))
        {
            CurrentStamina = Mathf.Min(CurrentStamina + 1, maxStamina);
            StaminaRegenTimer = TickTimer.CreateFromSeconds(Runner, 1f / staminaRegenRate);
        }
    }

    void HandleHealthRegeneration()
    {
        if (CurrentHealth < maxHealth && CurrentHealth > 0 && HealthRegenTimer.ExpiredOrNotRunning(Runner))
        {
            CurrentHealth = Mathf.Min(CurrentHealth + 1, maxHealth);
            HealthRegenTimer = TickTimer.CreateFromSeconds(Runner, 1f / healthRegenRate);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int damage, PlayerRef attacker)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            RPC_OnPlayerDied(attacker);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnPlayerDied(PlayerRef killer)
    {
        Debug.Log($"Player {Object.InputAuthority} was killed by {killer}");

        // Handle death effects
        if (Object.HasInputAuthority)
        {
            // Show death screen, respawn UI, etc.
            HandleLocalPlayerDeath();
        }

        // Play death animation, effects, etc.
        HandleDeathEffects();
    }

    public void RestoreHealth(int health)
    {
        if (Object.HasStateAuthority)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + health, maxHealth);
            if (CurrentHealth > 0)
                IsDead = false;
        }
    }

    public void RestoreStamina(int stamina)
    {
        if (Object.HasStateAuthority)
        {
            CurrentStamina = Mathf.Min(CurrentStamina + stamina, maxStamina);
        }
    }

    public bool UseStamina(int amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            return true;
        }
        return false;
    }

    void UpdateUI()
    {
        if (healthBar != null)
            healthBar.value = CurrentHealth;
        if (staminaBar != null)
            staminaBar.value = CurrentStamina;
    }

    void HandleLocalPlayerDeath()
    {
        // Disable player controls
        GetComponent<PlayerController>().enabled = false;

        // Show respawn UI
        // Implement your death/respawn logic here
    }

    void HandleDeathEffects()
    {
        // Play death animation
        // Spawn death particles
        // Play death sound
        // Implement your death effects here
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Respawn(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        CurrentHealth = maxHealth;
        CurrentStamina = maxStamina;
        IsDead = false;

        if (Object.HasInputAuthority)
        {
            GetComponent<PlayerController>().enabled = true;
        }
    }

    // Public methods for external access
    public bool IsAlive => !IsDead;
    public float HealthPercentage => (float)CurrentHealth / maxHealth;
    public float StaminaPercentage => (float)CurrentStamina / maxStamina;
}