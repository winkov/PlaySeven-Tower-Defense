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
    private Vector3 baseVisualScale = Vector3.one;

    void Awake()
    {
        baseDamage = damage;
        baseRange = range;
        baseFireRate = fireRate;
        baseSplashRadius = splashRadius;
        baseArmor = armor;
        baseVisualScale = transform.localScale;
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
        baseVisualScale = transform.localScale;

        ApplyVisualIdentity(); // 🔥 NOVO
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

        GameObject soldier = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        soldier.transform.position = transform.position + new Vector3(Random.Range(-0.6f, 0.6f), 0.6f, Random.Range(-0.6f, 0.6f));
        soldier.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);

        SoldierUnit unit = soldier.AddComponent<SoldierUnit>();
        unit.damage = Mathf.Max(6, damage / 2);
        unit.attackRate = 1.1f + (upgradeFireRateLevel * 0.08f);
        unit.attackRange = 1.1f + (upgradeRangeLevel * 0.04f);
        unit.lifetime = 9f + upgradeDamageLevel;

        soldierSpawnTimer = Mathf.Max(1.5f, 4.5f - (upgradeFireRateLevel * 0.4f));
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>();
        Enemy nearest = null;
        float nearestDistance = range;

        for (int i = 0; i < enemies.Length; i++)
        {
            float d = Vector3.Distance(transform.position, enemies[i].transform.position);
            if (d <= nearestDistance)
            {
                nearestDistance = d;
                nearest = enemies[i];
            }
        }
        return nearest;
    }

    void Shoot(Enemy target)
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.up;

        GameObject proj = projectilePrefab != null
            ? Instantiate(projectilePrefab, spawnPosition, Quaternion.identity)
            : CreateRuntimeProjectile(spawnPosition);

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile == null) projectile = proj.AddComponent<Projectile>();

        ConfigureProjectileVisual(projectile);
        projectile.SetTarget(target, damage);
    }

    GameObject CreateRuntimeProjectile(Vector3 pos)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        p.transform.position = pos;
        Destroy(p.GetComponent<Collider>());
        return p;
    }

    void ConfigureProjectileVisual(Projectile projectile)
    {
        if (towerType == TowerType.Mage)
        {
            projectile.speed = 11f;
            projectile.visibleScale = 0.3f;
            projectile.visualColor = new Color(0.2f, 0.7f, 1f);
            projectile.splashRadius = splashRadius;
        }
        else if (towerType == TowerType.Archer)
        {
            projectile.speed = 20f;
            projectile.visibleScale = 0.12f;
            projectile.visualColor = new Color(0.6f, 0.4f, 0.2f);
        }
        else if (towerType == TowerType.Catapult)
        {
            projectile.speed = 7f;
            projectile.visibleScale = 0.5f;
            projectile.visualColor = new Color(0.3f, 0.3f, 0.3f);
            projectile.splashRadius = splashRadius;
        }
        else
        {
            projectile.speed = 10f;
            projectile.visibleScale = 0.25f;
            projectile.visualColor = Color.gray;
        }
    }

    // 🔥 IDENTIDADE VISUAL
    void ApplyVisualIdentity()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>();

        Color color = Color.white;
        float scale = 1f;

        switch (towerType)
        {
            case TowerType.Mage:
                color = new Color(0.3f, 0.7f, 1f);
                scale = 0.8f;
                break;

            case TowerType.Archer:
                color = new Color(0.6f, 0.4f, 0.2f);
                scale = 0.9f;
                break;

            case TowerType.Catapult:
                color = new Color(0.4f, 0.4f, 0.4f);
                scale = 1.1f;
                break;

            case TowerType.Barracks:
                color = new Color(0.8f, 0.2f, 0.2f);
                scale = 1.0f;
                break;
        }

        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material = new Material(Shader.Find("Standard"));
            rends[i].material.color = color;
        }

        transform.localScale = baseVisualScale * scale;
    }

    public bool UpgradeDamage()
    {
        if (upgradeDamageLevel >= maxUpgradeLevel) return false;
        if (GameManager.Instance == null || GameManager.Instance.Gold < upgradeDamageCost) return false;

        GameManager.Instance.AddGold(-upgradeDamageCost);
        upgradeDamageLevel++;

        damage = baseDamage + (upgradeDamageLevel * (towerType == TowerType.Archer ? 2 : towerType == TowerType.Catapult ? 8 : 5));

        ApplyUpgradeScaleVisual();
        return true;
    }

    public bool UpgradeRange()
    {
        if (upgradeRangeLevel >= maxUpgradeLevel) return false;
        if (GameManager.Instance == null || GameManager.Instance.Gold < upgradeRangeCost) return false;

        GameManager.Instance.AddGold(-upgradeRangeCost);
        upgradeRangeLevel++;

        range = baseRange + (upgradeRangeLevel * 0.8f);

        ApplyUpgradeScaleVisual();
        return true;
    }

    public bool UpgradeFireRate()
    {
        if (upgradeFireRateLevel >= maxUpgradeLevel) return false;
        if (GameManager.Instance == null || GameManager.Instance.Gold < upgradeFireRateCost) return false;

        GameManager.Instance.AddGold(-upgradeFireRateCost);
        upgradeFireRateLevel++;

        fireRate = baseFireRate + (upgradeFireRateLevel * 0.2f);

        ApplyUpgradeScaleVisual();
        return true;
    }

    void ApplyUpgradeScaleVisual()
    {
        float level = upgradeDamageLevel + upgradeRangeLevel + upgradeFireRateLevel;
        float scaleBoost = 1f + (level * 0.03f);

        transform.localScale = baseVisualScale * scaleBoost;
    }
}