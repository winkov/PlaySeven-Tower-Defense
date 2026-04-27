using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public int damage = 20;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public bool debugLogs = true;

    public int upgradeDamageLevel = 0;
    public int upgradeRangeLevel = 0;
    public int upgradeFireRateLevel = 0;

    public int upgradeDamageCost = 50;
    public int upgradeRangeCost = 50;
    public int upgradeFireRateCost = 50;

    public int maxUpgradeLevel = 3;

    private float fireTimer;
    private bool warnedInvalidProjectilePrefab;
    private int baseDamage;
    private float baseRange;
    private float baseFireRate;

    void Awake()
    {
        baseDamage = damage;
        baseRange = range;
        baseFireRate = fireRate;
    }

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
        projectileObject.transform.localScale = Vector3.one * 0.28f;

        Collider projectileCollider = projectileObject.GetComponent<Collider>();
        if (projectileCollider != null)
        {
            Destroy(projectileCollider);
        }

        Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();
        if (projectileRenderer != null)
        {
            projectileRenderer.material.color = new Color(1f, 0.45f, 0.05f);
        }

        Light projectileLight = projectileObject.AddComponent<Light>();
        projectileLight.color = new Color(1f, 0.45f, 0.05f);
        projectileLight.range = 2.5f;
        projectileLight.intensity = 1.2f;

        Projectile projectile = projectileObject.AddComponent<Projectile>();
        projectile.visibleScale = 0.28f;
        return projectileObject;
    }

    public bool UpgradeDamage()
    {
        if (upgradeDamageLevel >= maxUpgradeLevel)
        {
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.Gold >= upgradeDamageCost)
        {
            GameManager.Instance.AddGold(-upgradeDamageCost);
            upgradeDamageLevel++;
            damage = baseDamage + (upgradeDamageLevel * 5);

            if (debugLogs)
            {
                Debug.Log("Tower damage upgraded to level " + upgradeDamageLevel + " (damage: " + damage + ")", this);
            }

            return true;
        }

        return false;
    }

    public bool UpgradeRange()
    {
        if (upgradeRangeLevel >= maxUpgradeLevel)
        {
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.Gold >= upgradeRangeCost)
        {
            GameManager.Instance.AddGold(-upgradeRangeCost);
            upgradeRangeLevel++;
            range = baseRange + (upgradeRangeLevel * 2f);

            if (debugLogs)
            {
                Debug.Log("Tower range upgraded to level " + upgradeRangeLevel + " (range: " + range + ")", this);
            }

            return true;
        }

        return false;
    }

    public bool UpgradeFireRate()
    {
        if (upgradeFireRateLevel >= maxUpgradeLevel)
        {
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.Gold >= upgradeFireRateCost)
        {
            GameManager.Instance.AddGold(-upgradeFireRateCost);
            upgradeFireRateLevel++;
            fireRate = baseFireRate + (upgradeFireRateLevel * 0.5f);

            if (debugLogs)
            {
                Debug.Log("Tower fire rate upgraded to level " + upgradeFireRateLevel + " (fire rate: " + fireRate + ")", this);
            }

            return true;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
