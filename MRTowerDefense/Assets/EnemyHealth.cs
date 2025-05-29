using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    private float currentHealth;
    private EnemyPathFollower enemy;

    void Awake()
    {
        currentHealth = maxHealth;
        enemy = GetComponent<EnemyPathFollower>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            // Destroy(gameObject); // or trigger death behavior
            enemy.Die(); 
        }
    }
}
