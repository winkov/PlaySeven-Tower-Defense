using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public int damage = 20;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public bool debugLogs = true;

    private float fireTimer;
    private bool warnedInvalidProjectilePrefab;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            Enemy target = FindNearestEnemy();

            if (target != null)
            {
                Shoot(target);
                fireTimer = 1f / fireRate;
            }
        }
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>();
        Enemy nearestEnemy = null;
        float nearestDistance = range;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    void Shoot(Enemy target)
    {
        Vector3 spawnPosition = transform.position + Vector3.up;

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        GameObject projectileObject = CreateProjectileObject(spawnPosition);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetTarget(target, damage);

            if (debugLogs)
            {
                Debug.Log("Tower shot a projectile at " + target.name, this);
            }
        }
        else if (debugLogs && !warnedInvalidProjectilePrefab)
        {
            Debug.LogWarning("Projectile Prefab does not have a Projectile script.", projectileObject);
            warnedInvalidProjectilePrefab = true;
        }
    }

    GameObject CreateProjectileObject(Vector3 spawnPosition)
    {
        if (projectilePrefab != null)
        {
            return Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        }

        GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectileObject.name = "Runtime Projectile";
        projectileObject.transform.position = spawnPosition;
        projectileObject.transform.localScale = Vector3.one * 0.35f;

        Collider projectileCollider = projectileObject.GetComponent<Collider>();
        if (projectileCollider != null)
        {
            Destroy(projectileCollider);
        }

        Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();
        if (projectileRenderer != null)
        {
            projectileRenderer.material.color = Color.red;
        }

        projectileObject.AddComponent<Projectile>();
        return projectileObject;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
