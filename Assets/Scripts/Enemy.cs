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
    private Transform leftLeg;
    private Transform rightLeg;
    private float walkAnimTimer;
    private Vector3 baseScale;

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
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (waypointPath == null || waypointPath.Count == 0) return;
        MoveAlongPath();
        AnimateWalk();
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
        baseScale = transform.localScale;
    }

    void ApplyVisualByType(EnemyTypeEnum type)
    {
        if (meshRenderer == null) meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null) meshRenderer.material.color = EnemyTypeHelper.GetColor(type);
        transform.localScale = Vector3.one * EnemyTypeHelper.GetScale(type) * 1.2f;

        if (transform.Find("Body") == null)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(0.55f, 0.8f, 0.45f);
            Collider bc = body.GetComponent<Collider>();
            if (bc != null) Destroy(bc);
            Renderer br = body.GetComponent<Renderer>();
            if (br != null) br.material.color = new Color(0.15f, 0.15f, 0.18f);

            leftLeg = CreateLeg("LeftLeg", new Vector3(-0.18f, 0.25f, 0f));
            rightLeg = CreateLeg("RightLeg", new Vector3(0.18f, 0.25f, 0f));
        }
        else
        {
            leftLeg = transform.Find("LeftLeg");
            rightLeg = transform.Find("RightLeg");
        }
    }

    Transform CreateLeg(string name, Vector3 localPos)
    {
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leg.name = name;
        leg.transform.SetParent(transform, false);
        leg.transform.localPosition = localPos;
        leg.transform.localScale = new Vector3(0.14f, 0.5f, 0.14f);
        Collider c = leg.GetComponent<Collider>();
        if (c != null) Destroy(c);
        Renderer r = leg.GetComponent<Renderer>();
        if (r != null) r.material.color = new Color(0.2f, 0.2f, 0.24f);
        return leg.transform;
    }

    void AnimateWalk()
    {
        walkAnimTimer += Time.deltaTime * (4f + speed * 0.8f);
        float swing = Mathf.Sin(walkAnimTimer) * 16f;
        float bob = Mathf.Abs(Mathf.Sin(walkAnimTimer * 0.9f)) * 0.06f;
        transform.localScale = baseScale + new Vector3(0f, bob, 0f);
        if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
        if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
    }

    public void ApplyWaveModifiers(float healthMultiplier, float speedBonus)
    {
        maxHealth = Mathf.CeilToInt(maxHealth * healthMultiplier);
        speed += speedBonus;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void MoveAlongPath()
    {
        Transform targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
        if (targetWaypoint == null) return;

        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        Vector3 dir = targetWaypoint.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * 8f);
        }

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
        canvasRect.sizeDelta = new Vector2(190f, 52f);
        canvasRect.localPosition = new Vector3(0f, 2.2f, 0f);
        canvasRect.localScale = Vector3.one * 0.014f;

        GameObject bg = new GameObject("HealthBarBg");
        bg.transform.SetParent(healthBarCanvasObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.86f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("HealthBarFill");
        fill.transform.SetParent(bg.transform, false);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(0.95f, 0.16f, 0.16f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = new Vector2(5f, 5f);
        fillRect.offsetMax = new Vector2(184f, -5f);

        GameObject label = new GameObject("HealthText");
        label.transform.SetParent(healthBarCanvasObj.transform, false);
        healthTextLabel = label.AddComponent<Text>();
        healthTextLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthTextLabel.fontSize = 20;
        healthTextLabel.alignment = TextAnchor.MiddleCenter;
        healthTextLabel.color = Color.white;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;
        float pct = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;
        RectTransform fillRect = healthBarFill.GetComponent<RectTransform>();
        if (fillRect != null)
        {
            float width = Mathf.Lerp(0f, 179f, pct);
            fillRect.offsetMax = new Vector2(width, -5f);
        }
        healthBarFill.color = Color.Lerp(new Color(0.55f, 0.04f, 0.04f, 1f), new Color(1f, 0.2f, 0.2f, 1f), pct);
        if (healthTextLabel != null) healthTextLabel.text = currentHealth + " / " + maxHealth;
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
