using UnityEngine;

public class RaycastTower : MonoBehaviour
{
    public float range = 10f;
    public float damagePerSecond = 5f;
    public float rotationSpeed = 5f;
    public Transform turret; // the rotating part
    public Transform rayOrigin; // where the ray starts
    public LineRenderer laserVisual;
    public float fireRate = 2f; // Shots per second
    private float fireCooldown = 0f; // Internal cooldown
    private float laserShowDuration = 0.05f; // How long the laser line is visible

    public GameObject bulletLinePrefab;


    private Transform target;

    void Update()
    {
        fireCooldown -= Time.deltaTime;


        FindTarget();

        if (target)
        {
            RotateTowardsTarget();

            if (fireCooldown <= 0f)
            {
                ShootBulletRay();
                fireCooldown = 1f / fireRate;

            }
        }

        // UpdateLaserVisibility();
    }

    void FindTarget()
    {
        // Find the first enemy in range (can optimize later)
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

    void ShootBulletRay()
    {
        RaycastHit hit;
        Vector3 dir = (target.position - rayOrigin.position).normalized;

        if (Physics.Raycast(rayOrigin.position, dir, out hit, range))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerSecond); // Apply damage per shot
                }
            }

            // if (laserVisual)
            // {
            //     laserVisual.SetPosition(0, rayOrigin.position);
            //     laserVisual.SetPosition(1, hit.point);
            // }
             // Visual bullet effect
            GameObject line = Instantiate(bulletLinePrefab);
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.SetPosition(0, rayOrigin.position);
            lr.SetPosition(1, hit.point);
            Destroy(line, 0.1f); // short lifetime
        }
    }
    // void UpdateLaserVisibility()
    // {
    //     if (laserVisual)
    //     {
    //         laserVisual.enabled = laserTimer > 0f;
    //     }
    // }


    void DisableLaser()
    {
        if (laserVisual)
        {
            laserVisual.enabled = false;
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

}
