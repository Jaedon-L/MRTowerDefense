using UnityEngine;
using UnityEngine.Events;
using TMPro;


public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshPro healthText;

    [Tooltip("Fires when health changes (currentHealth, maxHealth)")]
    public UnityEvent<int,int> onHealthChanged;

    [Tooltip("Fires when health hits zero")]
    public UnityEvent onDeath;

    void Awake()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        healthText.text = "Player health: " + maxHealth;
        onHealthChanged.AddListener(UpdateHealthUI);

    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        healthText.text = "Player health: " + maxHealth;

        // UpdateHealthUI(); 
    }
        private void UpdateHealthUI(int current, int max)
    {
        healthText.text       = "Player health: " + current;
    }

    /// <summary>
    /// Subtract hitPoints from currentHealth, clamp at 0, and fire events.
    /// </summary>
    public void TakeDamage(int hitPoints)
    {
        currentHealth = Mathf.Max(currentHealth - hitPoints, 0);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth == 0)
        {
            onDeath?.Invoke();
            ResetHealth(); 
        }
            

    }
}
