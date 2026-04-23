using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged; // Fires whenever health changes, passes current health

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Call this to deal damage — e.g. TakeDamage(25f)
    public void TakeDamage(float amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        onHealthChanged.Invoke(currentHealth);

        Debug.Log($"Took {amount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Call this to heal — e.g. Heal(10f)
    public void Heal(float amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged.Invoke(currentHealth);

        Debug.Log($"Healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    // Fully restore health
    public void HealFull()
    {
        Heal(maxHealth);
    }

    // Returns health as 0-1 value, useful for health bars
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    void Die()
    {
        Debug.Log("Player died!");
        onDeath.Invoke();

        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}