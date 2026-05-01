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
    private Camera mainCamera;

    void Start()
    {
        currentHealth = maxHealth;
        waypointPath = FindAnyObjectByType<WaypointPath>();
        waveManager = FindAnyObjectByType<WaveManager>();
        mainCamera = Camera.main;
        meshRenderer = GetComponent<Renderer>();
        ApplyVisualByType(enemyType);
        CreateHealthBar();
        UpdateHealthBar();
    }

    void LateUpdate()
    {
        if (healthBarCanvasObj != null && mainCamera != null)
        {
            healthBarCanvasObj.transform.forward = mainCamera.transform.forward;
        }
    }

    public void SetEnemyType(EnemyTypeEnum type)
    {
        enemyType = type;
        speed = EnemyTypeHelper.GetSpeed(type);
        maxHealth = EnemyTypeHelper.GetMaxHealth(type);
        currentHealth = maxHealth;
        goldValue = EnemyTypeHelper.GetGoldValue(type);
        ApplyVisualByType(type);
        CreateHealthBar();
        UpdateHealthBar();
    }

    void ApplyVisualByType(EnemyTypeEnum type)
    {
        if (meshRenderer == null) meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null) meshRenderer.material.color = EnemyTypeHelper.GetColor(type);
        transform.localScale = Vector3.one * EnemyTypeHelper.GetScale(type) * 1.2f;

        if (transform.childCount == 0)
        {
            GameObject helm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helm.transform.SetParent(transform);
            helm.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            helm.transform.localScale = new Vector3(0.45f, 0.35f, 0.45f);
            Collider hc = helm.GetComponent<Collider>();
            if (hc != null) Destroy(hc);
            Renderer hr = helm.GetComponent<Renderer>();
            if (hr != null) hr.material.color = new Color(0.15f, 0.15f, 0.18f);
        }
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
        if (waypointPath == null || waypointPath.Count == 0) return;
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        Transform targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
        if (targetWaypoint == null) return;
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetWaypoint.position) <= 0.05f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypointPath.Count) ReachEndOfPath();
        }
    }

    void CreateHealthBar()
    {
        if (healthBarCanvasObj != null) Destroy(healthBarCanvasObj);
        healthBarCanvasObj = new GameObject("HealthBarCanvas");
        healthBarCanvasObj.transform.SetParent(transform, false);
        Canvas canvas = healthBarCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 20;
        RectTransform canvasRect = healthBarCanvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2.2f, 0.6f);
        canvasRect.localPosition = new Vector3(0f, 1.7f, 0f);
        canvasRect.localScale = Vector3.one * 0.45f;

        GameObject bg = new GameObject("HealthBarBg");
        bg.transform.SetParent(healthBarCanvasObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.8f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("HealthBarFill");
        fill.transform.SetParent(bg.transform, false);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(0.24f, 0.95f, 0.35f, 0.95f);
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 20f); fillRect.offsetMax = new Vector2(-2f, -2f);

        GameObject label = new GameObject("HealthText");
        label.transform.SetParent(healthBarCanvasObj.transform, false);
        healthTextLabel = label.AddComponent<Text>();
        healthTextLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthTextLabel.fontSize = 30;
        healthTextLabel.alignment = TextAnchor.UpperCenter;
        healthTextLabel.color = Color.white;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;
        healthBarFill.fillAmount = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;
        if (healthTextLabel != null) healthTextLabel.text = currentHealth + "/" + maxHealth;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        currentHealth -= dmg;
        UpdateHealthBar();
        if (currentHealth <= 0) Die(true);
    }

    void ReachEndOfPath()
    {
        if (isDead) return;
        if (GameManager.Instance != null) GameManager.Instance.DamageCastle(castleDamage);
        Die(false);
    }

    void Die(bool giveGold)
    {
        if (isDead) return;
        isDead = true;

        if (giveGold && GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldValue);
            ShowGoldPopup("+" + goldValue);
        }
        if (waveManager != null) waveManager.OnEnemyDied();
        Destroy(gameObject);
    }

    void ShowGoldPopup(string text)
    {
        GameObject popup = new GameObject("GoldPopup");
        popup.transform.position = transform.position + Vector3.up * 2f;
        Canvas canvas = popup.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2.4f, 0.8f);
        rect.localScale = Vector3.one * 0.02f;
        Text label = popup.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text;
        label.fontSize = 64;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(1f, 0.9f, 0.2f);
        Destroy(popup, 0.75f);
    }
}
