using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 5f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            EnemyHealth health = collision.collider.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            Destroy(gameObject, 0.25f);
        }

        
    }
    void Start()
    {
        Destroy(gameObject, 2);
    }
}
