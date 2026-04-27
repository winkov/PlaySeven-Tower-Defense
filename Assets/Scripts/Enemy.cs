using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyTypeEnum enemyType = EnemyTypeEnum.Archer;
    public float speed = 3f;
    public int maxHealth = 100;
    public int goldValue = 10;
    public int castleDamage = 1;

    private int currentHealth;
    private int currentWaypointIndex;
    private WaypointPath waypointPath;
    private WaveManager waveManager;
    private bool isDead;
    private Renderer meshRenderer;
    private GameObject healthBarCanvasObj;
    private Image healthBarFill;
    private Text healthTextLabel;

    void Start()
    {
        currentHealth = maxHealth;
        currentWaypointIndex = 0;

        waypointPath = FindAnyObjectByType<WaypointPath>();
        waveManager = FindAnyObjectByType<WaveManager>();

        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.color = EnemyTypeHelper.GetColor(enemyType);
        }

        transform.localScale = Vector3.one * EnemyTypeHelper.GetScale(enemyType);
        CreateHealthBar();
        UpdateHealthBar();
    }

    public void SetEnemyType(EnemyTypeEnum type)
    {
        enemyType = type;
        speed = EnemyTypeHelper.GetSpeed(type);
        maxHealth = EnemyTypeHelper.GetMaxHealth(type);
        currentHealth = maxHealth;
        goldValue = EnemyTypeHelper.GetGoldValue(type);

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<Renderer>();
        }

        if (meshRenderer != null)
        {
            meshRenderer.material.color = EnemyTypeHelper.GetColor(type);
        }

        transform.localScale = Vector3.one * EnemyTypeHelper.GetScale(type);
        CreateHealthBar();
        UpdateHealthBar();
    }

    public void ApplyWaveModifiers(float healthMultiplier, float speedBonus)
    {
        maxHealth = Mathf.CeilToInt(maxHealth * healthMultiplier);
        speed += speedBonus;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        if (waypointPath == null || waypointPath.Count == 0)
        {
            return;
        }

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        Transform targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);

        if (targetWaypoint == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWaypoint.position,
            speed * Time.deltaTime
        );

        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint <= 0.05f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypointPath.Count)
            {
                ReachEndOfPath();
            }
        }
    }

    void CreateHealthBar()
    {
        if (healthBarCanvasObj != null)
        {
            CreateHealthBar();
        }

        healthBarCanvasObj = new GameObject("HealthBarCanvas");
        healthBarCanvasObj.transform.SetParent(transform, false);

        Canvas canvas = healthBarCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5;

        CanvasScaler scaler = healthBarCanvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        healthBarCanvasObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = healthBarCanvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1.4f, 0.35f);
        canvasRect.localPosition = new Vector3(0f, 1.4f, 0f);
        canvasRect.localRotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one * 0.35f;

        GameObject bg = new GameObject("HealthBarBg");
        bg.transform.SetParent(healthBarCanvasObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("HealthBarFill");
        fill.transform.SetParent(bg.transform, false);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(0.20f, 0.90f, 0.35f, 0.95f);
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBarFill.fillAmount = 1f;
        healthBarFill.raycastTarget = false;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 16f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        GameObject label = new GameObject("HealthText");
        label.transform.SetParent(healthBarCanvasObj.transform, false);
        healthTextLabel = label.AddComponent<Text>();
        healthTextLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthTextLabel.text = currentHealth + "/" + maxHealth;
        healthTextLabel.fontSize = 24;
        healthTextLabel.alignment = TextAnchor.UpperCenter;
        healthTextLabel.color = Color.white;
        healthTextLabel.raycastTarget = false;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(0f, 0f);
        labelRect.offsetMax = new Vector2(0f, 0f);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;
        float fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        healthBarFill.fillAmount = Mathf.Clamp01(fillAmount);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die(true);
        }
    }

    void ReachEndOfPath()
    {
        if (isDead) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.DamageCastle(castleDamage);
        }

        Die(false);
    }

    void Die(bool giveGold)
    {
        if (isDead) return;

        isDead = true;

        if (giveGold && GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldValue);
        }

        if (waveManager != null)
        {
            waveManager.OnEnemyDied();
        }

        Destroy(gameObject);
    }
}
