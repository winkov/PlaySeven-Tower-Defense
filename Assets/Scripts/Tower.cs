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
    public enum TargetMode
    {
        First,
        Last,
        Strongest,
        Closest
    }

    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.First;
    public bool canHitFlying = true;
    public bool canHitGround = true;
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
    private LineRenderer targetLine;
    private Enemy currentTarget;
    private int baseDamage;
    private float baseRange;
    private float baseFireRate;
    private float baseSplashRadius;
    private int baseArmor;
    private Vector3 baseVisualScale = Vector3.one;
    private float lineTimer;
    void Awake()
    {
        CreateTargetLine();

        baseDamage = damage;
        baseRange = range;
        baseFireRate = fireRate;
        baseSplashRadius = splashRadius;
        baseArmor = armor;
        baseVisualScale = transform.localScale;

        // 🔥 ADICIONA ISSO
        if (projectilePrefab == null)
        {
            Debug.LogError("🚨 Tower sem projectilePrefab configurado!", this);
        }
    }
    void CreateTargetLine()
    {
        GameObject lineObj = new GameObject("TargetLine");
        lineObj.transform.SetParent(transform);

        targetLine = lineObj.AddComponent<LineRenderer>();

        targetLine.positionCount = 2;
        targetLine.startWidth = 0.05f;
        targetLine.endWidth = 0.02f;

        targetLine.material = new Material(Shader.Find("Sprites/Default"));
        targetLine.startColor = new Color(1f, 0.9f, 0.3f, 1f);
        targetLine.endColor = new Color(1f, 0.2f, 0.1f, 0.4f);

        targetLine.enabled = false;
        if (towerType == TowerType.Mage)
        {
            targetLine.startColor = Color.cyan;
        }
        else if (towerType == TowerType.Archer)
        {
            targetLine.startColor = new Color(0.6f, 0.4f, 0.2f);
        }
        else if (towerType == TowerType.Catapult)
        {
            targetLine.startColor = Color.gray;
        }
        float pulse = Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.03f;
        targetLine.startWidth = 0.04f + pulse;
    }
    public void ConfigureByType(TowerType type)
    {
        towerType = type;
        maxUpgradeLevel = 5;
        if (type == TowerType.Mage)
        {
            canHitFlying = true;
            canHitGround = true;
            targetMode = TargetMode.Strongest;
        }
        else if (type == TowerType.Archer)
        {
            canHitFlying = true;
            canHitGround = true;
            targetMode = TargetMode.First;
        }
        else if (type == TowerType.Catapult)
        {
            canHitFlying = false; // 🔥 importante
            canHitGround = true;
            targetMode = TargetMode.Closest;
        }
        else // Barracks
        {
            canHitFlying = false; // 🔥 importante
            canHitGround = true;
            targetMode = TargetMode.First;
        }
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

        Enemy target = SelectTarget();
        currentTarget = target;

        UpdateTargetLine();

        if (target == null) return;

        Shoot(target);
        fireTimer = 1f / Mathf.Max(0.1f, fireRate);
    }
    void UpdateTargetLine()
    {
        if (targetLine == null) return;

        lineTimer -= Time.deltaTime;

        if (currentTarget == null || lineTimer <= 0f)
        {
            targetLine.enabled = false;
            return;
        }

        targetLine.enabled = true;

        Vector3 start = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1f;
        Vector3 end = currentTarget.transform.position + Vector3.up * 0.8f;

        targetLine.SetPosition(0, start);
        targetLine.SetPosition(1, end);
        float pulse = Mathf.Abs(Mathf.Sin(Time.time * 20f)) * 0.02f;

        targetLine.startWidth = 0.06f + pulse;
        targetLine.endWidth = 0.02f;
    }
    void HandleBarracksSpawn()
    {
        soldierSpawnTimer -= Time.deltaTime;
        if (soldierSpawnTimer > 0f) return;

        // 🔥 usa lista otimizada
        if (Enemy.ActiveEnemies.Count == 0) return;

        Vector3 spawnPos = transform.position + new Vector3(
            Random.Range(-0.6f, 0.6f),
            0.6f,
            Random.Range(-0.6f, 0.6f)
        );

        GameObject soldier = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        soldier.transform.position = spawnPos;
        soldier.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);

        SoldierUnit unit = soldier.AddComponent<SoldierUnit>();
        unit.damage = Mathf.Max(6, damage / 2);
        unit.attackRate = 1.1f + (upgradeFireRateLevel * 0.08f);
        unit.attackRange = 1.6f + (upgradeRangeLevel * 0.1f);
        unit.lifetime = 9f + upgradeDamageLevel;

        // 🔥 debug pra confirmar
        Debug.Log("Soldado spawnado");

        soldierSpawnTimer = Mathf.Max(1.5f, 4.5f - (upgradeFireRateLevel * 0.4f));
    }

    Enemy SelectTarget()
    {
        var enemies = Enemy.ActiveEnemies;

        Enemy best = null;
        float bestValue = float.MinValue;

        Vector3 myPos = transform.position;
        float rangeSqr = range * range;

        foreach (var e in enemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;

            Vector3 enemyPos = e.transform.position;
            Vector3 diff = enemyPos - myPos;
            float distSqr = diff.sqrMagnitude;

            if (distSqr > rangeSqr) continue;

            // 🔥 FILTRO DE TIPO
            if (e.enemyType == EnemyTypeEnum.Flying && !canHitFlying)
                continue;

            if (e.enemyType != EnemyTypeEnum.Flying && !canHitGround)
                continue;

            float value = 0f;

            switch (targetMode)
            {
                case TargetMode.Closest:
                    value = -distSqr; // 🔥 evita sqrt
                    break;

                case TargetMode.Strongest:
                    value = e.GetCurrentHealth();
                    break;

                case TargetMode.First:
                    value = e.GetPathProgress();
                    break;

                case TargetMode.Last:
                    value = -e.GetPathProgress();
                    break;
            }

            if (value > bestValue)
            {
                best = e;
                bestValue = value;
            }
        }

        return best;
    }

    void Shoot(Enemy target)
    {
        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up;

        GameObject proj;

        if (projectilePrefab != null)
        {
            proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("⚠ ProjectilePrefab NULL - usando fallback", this);
            proj = CreateRuntimeProjectile(spawnPosition);
        }

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile == null)
            projectile = proj.AddComponent<Projectile>();

        ConfigureProjectileVisual(projectile);
        projectile.SetTarget(target, damage);

        lineTimer = 0.08f;
    }

    GameObject CreateRuntimeProjectile(Vector3 pos)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        p.transform.position = pos;

        Destroy(p.GetComponent<Collider>());

        var proj = p.AddComponent<Projectile>();

        // 🔥 IMPORTANTE: aqui você precisa arrastar manual depois
        // (ou vai continuar null)
        proj.shootSound = null;
        proj.impactSound = null;

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