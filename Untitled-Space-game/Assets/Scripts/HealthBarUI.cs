using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider healthSlider;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        healthSlider.value = 1f;
        playerHealth.onHealthChanged.AddListener(OnHealthChanged);
    }

    void OnDestroy()
    {
        playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    void OnHealthChanged(float currentHealth)
    {
        healthSlider.value = playerHealth.GetHealthPercent();
    }
}