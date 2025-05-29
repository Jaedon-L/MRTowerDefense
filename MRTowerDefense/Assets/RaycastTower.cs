using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RaycastTower : MonoBehaviour
{
    public float range = .45f;
    public float damagePerShot = 5f;
    public float rotationSpeed = 5f;
    public Transform turret; // the rotating part
    public Transform firePoint; // where the projectile spawns from
    public float fireRate = 2f;
    public float launchForce = 20f; // adjust as needed
    public GameObject projectilePrefab;

    private float fireCooldown = 0f;
    private Transform target;
    public AudioClip cannonSound;
    private AudioSource audioSource;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        fireCooldown -= Time.deltaTime;

        FindTarget();

        if (target)
        {
            RotateTowardsTarget();

            if (fireCooldown <= 0f)
            {
                FireProjectile();
                fireCooldown = 1f / fireRate;
            }
        }

        // UpdateLaserVisibility(); // (if you reactivate ray later)
    }

    void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        target = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                target = hit.transform;
                break;
            }
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = target.position - turret.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(turret.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
        turret.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void FireProjectile()
    {
        if (target == null) return;
        // Play cannon sound
        if (audioSource && cannonSound)
        {
            audioSource.PlayOneShot(cannonSound);
        }
        // Instantiate the projectile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Calculate direction to the *center* of the enemy
        Vector3 direction = (target.position - firePoint.position).normalized;

        // Rotate projectile to face the target
        proj.transform.forward = direction;

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * launchForce;
            // If gravity is off and you want a laser-like projectile, this works perfectly.
        }


    }


    /*
    void ShootBulletRay()
    {
        RaycastHit hit;
        Vector3 dir = (target.position - firePoint.position).normalized;

        if (Physics.Raycast(firePoint.position, dir, out hit, range))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerShot);
                }
            }

            GameObject line = Instantiate(bulletLinePrefab);
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, hit.point);
            Destroy(line, 0.1f);
        }
    }
    */

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
