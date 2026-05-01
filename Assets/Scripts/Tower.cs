using UnityEngine;

public enum TowerType
{
    Mage,
    Archer,
    Catapult,
    Barracks
}

public class Tower : MonoBehaviour
{
    public TowerType towerType = TowerType.Mage;
    public float range = 8f;
    public float fireRate = 1f;
    public int damage = 20;
    public float splashRadius = 0f;
    public int armor = 0;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public bool debugLogs = true;

    public int upgradeDamageLevel = 0;
    public int upgradeRangeLevel = 0;
    public int upgradeFireRateLevel = 0;
    public int maxUpgradeLevel = 5;
    public int upgradeDamageCost = 50;
    public int upgradeRangeCost = 50;
    public int upgradeFireRateCost = 50;

    private float fireTimer;
    private float soldierSpawnTimer;
    private int baseDamage;
    private float baseRange;
    private float baseFireRate;
    private float baseSplashRadius;
    private int baseArmor;

    void Awake()
    {
        baseDamage = damage;
        baseRange = range;
        baseFireRate = fireRate;
        baseSplashRadius = splashRadius;
        baseArmor = armor;
    }

    public void ConfigureByType(TowerType type)
    {
        towerType = type;
        maxUpgradeLevel = 5;

        if (type == TowerType.Mage)
        {
            damage = 22; range = 7.8f; fireRate = 1.2f; splashRadius = 1.5f; armor = 0;
            upgradeDamageCost = 70; upgradeRangeCost = 65; upgradeFireRateCost = 70;
        }
        else if (type == TowerType.Archer)
        {
            damage = 10; range = 10.8f; fireRate = 2.05f; splashRadius = 0f; armor = 0;
            upgradeDamageCost = 50; upgradeRangeCost = 60; upgradeFireRateCost = 55;
        }
        else if (type == TowerType.Catapult)
        {
            damage = 52; range = 13.2f; fireRate = 0.52f; splashRadius = 2.5f; armor = 0;
            upgradeDamageCost = 95; upgradeRangeCost = 85; upgradeFireRateCost = 90;
        }
        else
        {
            damage = 16; range = 3.4f; fireRate = 1.3f; splashRadius = 0f; armor = 5;
            upgradeDamageCost = 65; upgradeRangeCost = 55; upgradeFireRateCost = 60;
        }

        upgradeDamageLevel = 0;
        upgradeRangeLevel = 0;
        upgradeFireRateLevel = 0;
        baseDamage = damage;
        baseRange = range;
        baseFireRate = fireRate;
        baseSplashRadius = splashRadius;
        baseArmor = armor;
    }

    void Update()
    {
        if (towerType == TowerType.Barracks)
        {
            HandleBarracksSpawn();
            return;
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f) return;

        Enemy target = FindNearestEnemy();
        if (target == null) return;

        Shoot(target);
        fireTimer = 1f / Mathf.Max(0.1f, fireRate);
    }

    void HandleBarracksSpawn()
    {
        soldierSpawnTimer -= Time.deltaTime;
        if (soldierSpawnTimer > 0f) return;
        if (FindNearestEnemy() == null) return;

        GameObject soldierObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        soldierObj.name = "Soldier";
        soldierObj.transform.position = transform.position + new Vector3(Random.Range(-0.6f, 0.6f), 0.6f, Random.Range(-0.6f, 0.6f));
        soldierObj.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
        Renderer renderer = soldierObj.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = new Color(0.5f, 0.5f, 0.55f);
        SoldierUnit soldier = soldierObj.AddComponent<SoldierUnit>();
        soldier.damage = Mathf.Max(6, damage / 2);
        soldier.attackRate = 1.1f + (upgradeFireRateLevel * 0.08f);
        soldier.attackRange = 1.1f + (upgradeRangeLevel * 0.04f);
        soldier.lifetime = 9f + upgradeDamageLevel;
        soldierSpawnTimer = Mathf.Max(1.5f, 4.5f - (upgradeFireRateLevel * 0.4f));
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>();
        Enemy nearestEnemy = null;
        float nearestDistance = range;

        for (int i = 0; i < enemies.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, enemies[i].transform.position);
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemies[i];
            }
        }
        return nearestEnemy;
    }

    void Shoot(Enemy target)
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.up;
        GameObject projectileObject = projectilePrefab != null ? Instantiate(projectilePrefab, spawnPosition, Quaternion.identity) : CreateRuntimeProjectile(spawnPosition);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile == null) projectile = projectileObject.AddComponent<Projectile>();
        ConfigureProjectileVisual(projectile);
        projectile.SetTarget(target, damage);
    }

    GameObject CreateRuntimeProjectile(Vector3 spawnPosition)
    {
        PrimitiveType primitive = towerType == TowerType.Archer ? PrimitiveType.Capsule : towerType == TowerType.Catapult ? PrimitiveType.Sphere : PrimitiveType.Sphere;
        GameObject projectileObject = GameObject.CreatePrimitive(primitive);
        projectileObject.transform.position = spawnPosition;
        projectileObject.transform.localScale = Vector3.one * (towerType == TowerType.Catapult ? 0.38f : 0.22f);
        Collider c = projectileObject.GetComponent<Collider>();
        if (c != null) Destroy(c);
        return projectileObject;
    }

    void ConfigureProjectileVisual(Projectile projectile)
    {
        if (towerType == TowerType.Mage)
        {
            projectile.speed = 11f;
            projectile.visibleScale = 0.26f;
            projectile.visualColor = new Color(0.25f, 0.75f, 1f);
            projectile.splashRadius = splashRadius;
        }
        else if (towerType == TowerType.Archer)
        {
            projectile.speed = 19f;
            projectile.visibleScale = 0.18f;
            projectile.visualColor = new Color(0.65f, 0.45f, 0.2f);
            projectile.splashRadius = 0f;
        }
        else if (towerType == TowerType.Catapult)
        {
            projectile.speed = 8f;
            projectile.visibleScale = 0.4f;
            projectile.visualColor = new Color(0.35f, 0.35f, 0.38f);
            projectile.splashRadius = splashRadius;
        }
        else
        {
            projectile.speed = 10f;
            projectile.visibleScale = 0.2f;
            projectile.visualColor = Color.gray;
            projectile.splashRadius = 0f;
        }
    }

    public bool UpgradeDamage()
    {
        if (upgradeDamageLevel >= maxUpgradeLevel || GameManager.Instance == null || GameManager.Instance.Gold < upgradeDamageCost) return false;
        GameManager.Instance.AddGold(-upgradeDamageCost);
        upgradeDamageLevel++;
        damage = baseDamage + (upgradeDamageLevel * (towerType == TowerType.Archer ? 2 : towerType == TowerType.Catapult ? 8 : 5));
        if (towerType == TowerType.Barracks) armor = baseArmor + upgradeDamageLevel * 2;
        ApplyUpgradeScaleVisual();
        return true;
    }

    public bool UpgradeRange()
    {
        if (upgradeRangeLevel >= maxUpgradeLevel || GameManager.Instance == null || GameManager.Instance.Gold < upgradeRangeCost) return false;
        GameManager.Instance.AddGold(-upgradeRangeCost);
        upgradeRangeLevel++;
        range = baseRange + (upgradeRangeLevel * (towerType == TowerType.Barracks ? 0.35f : towerType == TowerType.Catapult ? 0.75f : 0.9f));
        if (towerType == TowerType.Catapult || towerType == TowerType.Mage) splashRadius = baseSplashRadius + (upgradeRangeLevel * 0.25f);
        ApplyUpgradeScaleVisual();
        return true;
    }

    public bool UpgradeFireRate()
    {
        if (upgradeFireRateLevel >= maxUpgradeLevel || GameManager.Instance == null || GameManager.Instance.Gold < upgradeFireRateCost) return false;
        GameManager.Instance.AddGold(-upgradeFireRateCost);
        upgradeFireRateLevel++;
        fireRate = baseFireRate + (upgradeFireRateLevel * (towerType == TowerType.Catapult ? 0.09f : towerType == TowerType.Archer ? 0.22f : 0.18f));
        ApplyUpgradeScaleVisual();
        return true;
    }

    void ApplyUpgradeScaleVisual()
    {
        float levelFactor = (upgradeDamageLevel + upgradeRangeLevel + upgradeFireRateLevel) * 0.015f;
        transform.localScale = Vector3.one * (1f + Mathf.Clamp(levelFactor, 0f, 0.25f));
    }

    public void PlayUpgradeFeedback(Color glowColor)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = Color.Lerp(renderers[i].material.color, glowColor, 0.35f);
        }
    }
}

public class SoldierUnit : MonoBehaviour
{
    public float speed = 4.2f;
    public float attackRange = 1.1f;
    public float attackRate = 1.1f;
    public int damage = 14;
    public float lifetime = 10f;

    private float attackTimer;
    private float lifeTimer;

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Enemy target = FindNearestEnemy();
        if (target == null) return;
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            return;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            target.TakeDamage(damage);
            attackTimer = 1f / Mathf.Max(0.2f, attackRate);
        }
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>();
        Enemy nearest = null;
        float nearestDist = 999f;
        for (int i = 0; i < enemies.Length; i++)
        {
            float d = Vector3.Distance(transform.position, enemies[i].transform.position);
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = enemies[i];
            }
        }
        return nearest;
    }
}
