using UnityEngine;

public class CubeDamage : MonoBehaviour
{
    public float damageAmount = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }
    }
}