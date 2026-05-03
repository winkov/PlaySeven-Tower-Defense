using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class Enemy : MonoBehaviour
{
    public EnemyTypeEnum enemyType = EnemyTypeEnum.Archer;
    public float speed = 3f;
    public int maxHealth = 100;
    public int goldValue = 10;
    public int castleDamage = 1;
    public GameObject deathEffectPrefab;
    private int currentHealth;
    private int currentWaypointIndex;
    private WaypointPath waypointPath;
    private WaveManager waveManager;
    private bool isDead;
    private bool isBlocked = false;
    private Transform blocker;

    private Renderer[] meshRenderers;
    private Color originalColor;
    private GameObject healthBarCanvasObj;
    private Image healthBarFill;

    private Camera mainCamera;

    private Transform leftLeg;
    private Transform rightLeg;

    private float walkAnimTimer;
    private Vector3 baseScale;

    private float flightHeight;

    private float stuckTimer;
    private Vector3 lastPosition;
    public float GetPathProgress()
    {
        if (waypointPath == null || waypointPath.Count == 0)
            return 0f;

        return currentWaypointIndex;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // =========================
    // INIT
    // =========================
    public void SetBlocked(Transform source)
    {
        isBlocked = true;
        blocker = source;
    }

    public void ReleaseBlock(Transform source)
    {
        if (blocker == source)
        {
            isBlocked = false;
            blocker = null;
        }
    }
    public static List<Enemy> ActiveEnemies = new List<Enemy>();

    void OnEnable()
    {
        ActiveEnemies.Add(this);
    }

    void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }
    void Start()
    {
        currentHealth = maxHealth;

        waypointPath = FindAnyObjectByType<WaypointPath>();
        waveManager = FindAnyObjectByType<WaveManager>();
        mainCamera = Camera.main;

        DisablePhysics();

        meshRenderers = GetComponentsInChildren<Renderer>();

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            originalColor = meshRenderers[0].material.color;
        }

        ApplyVisualByType(enemyType);

        InitializePathProgress();

        lastPosition = transform.position;

        CreateHealthBar();
        UpdateHealthBar();

        baseScale = transform.localScale;
        displayedHealthPct = 1f;
        targetHealthPct = 1f;
        healthBarFill.fillAmount = 1f;
    }
    System.Collections.IEnumerator HitFlash()
    {
        foreach (var r in meshRenderers)
            r.material.color = Color.white;

        yield return new WaitForSeconds(0.08f);

        foreach (var r in meshRenderers)
            r.material.color = originalColor;
    }
    void DisablePhysics()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.applyRootMotion = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        Behaviour navAgent = GetComponent("NavMeshAgent") as Behaviour;
        if (navAgent != null) navAgent.enabled = false;
    }

    // =========================
    // UPDATE
    // =========================
    void Update()
    {
        if (waypointPath == null || waypointPath.Count == 0) return;

        MoveAlongPath();
        HandleStuckFallback();
        AnimateWalk();
        UpdateHealthBarSmooth();
    }
    void ShowDamagePopup(int dmg)
    {
        GameObject popup = new GameObject("DamagePopup");

        popup.transform.position =
            transform.position + Vector3.up * (GetVisualHeight() + 0.5f);

        Canvas canvas = popup.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;

        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2f, 1f);
        rect.localScale = Vector3.one * 0.02f;

        Text text = popup.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = dmg.ToString();
        text.fontSize = 80;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.yellow;

        StartCoroutine(AnimatePopup(popup, text));
    }
    System.Collections.IEnumerator AnimatePopup(GameObject popup, Text text)
    {
        float t = 0f;
        Vector3 start = popup.transform.position;
        Vector3 end = start + Vector3.up * 1.5f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            popup.transform.position = Vector3.Lerp(start, end, t);

            // 🔥 sempre olhando pra câmera
            if (mainCamera != null)
                popup.transform.forward = mainCamera.transform.forward;

            Color c = text.color;
            c.a = 1f - t;
            text.color = c;

            yield return null;
        }

        Destroy(popup);
    }
    void UpdateHealthBarSmooth()
    {
        if (healthBarFill == null) return;

        displayedHealthPct = Mathf.MoveTowards(
            displayedHealthPct,
            targetHealthPct,
            Time.deltaTime * 1.5f
        );

        RectTransform fillRect = healthBarFill.GetComponent<RectTransform>();

        if (fillRect != null)
        {
            // 🔥 aqui está o segredo: muda o tamanho REAL
            fillRect.anchorMax = new Vector2(displayedHealthPct, 1f);
        }

        // 🎨 cor dinâmica
        if (displayedHealthPct > 0.6f)
            healthBarFill.color = Color.green;
        else if (displayedHealthPct > 0.3f)
            healthBarFill.color = Color.yellow;
        else
            healthBarFill.color = Color.red;
    }
    void LateUpdate()
    {
        if (healthBarCanvasObj == null || mainCamera == null) return;

        float height = GetVisualHeight();

        // 🔥 posiciona sempre acima da cabeça
        healthBarCanvasObj.transform.position =
            transform.position + Vector3.up * (height + 0.4f);

        // 🔥 sempre vira pra câmera
        healthBarCanvasObj.transform.rotation =
            Quaternion.LookRotation(mainCamera.transform.forward);
    }

    // =========================
    // TYPE CONFIG
    // =========================
    public void SetEnemyType(EnemyTypeEnum type)
    {
        enemyType = type;

        speed = EnemyTypeHelper.GetSpeed(type);
        maxHealth = EnemyTypeHelper.GetMaxHealth(type);
        currentHealth = maxHealth;
        goldValue = EnemyTypeHelper.GetGoldValue(type);

        flightHeight = type == EnemyTypeEnum.Flying ? 1.6f : 0f;

        ApplyVisualByType(type);

        if (healthBarCanvasObj == null)
            CreateHealthBar();

        UpdateHealthBar();

        baseScale = transform.localScale;
    }

    void ApplyVisualByType(EnemyTypeEnum type)
    {
        Color color = EnemyTypeHelper.GetColor(type);
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<Renderer>(true);
        }
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material != null)
            {
                meshRenderers[i].material.color = color;
            }
        }

        transform.localScale = Vector3.one * EnemyTypeHelper.GetScale(type) * 1.2f;

        if (transform.Find("Body") == null)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(0.55f, 0.8f, 0.45f);

            Destroy(body.GetComponent<Collider>());

            leftLeg = transform.Find("LeftLeg") ?? transform.GetComponentInChildren<Transform>().Find("LeftLeg");
            rightLeg = transform.Find("RightLeg") ?? transform.GetComponentInChildren<Transform>().Find("RightLeg");
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

        Destroy(leg.GetComponent<Collider>());

        return leg.transform;
    }

    // =========================
    // ANIMATION
    // =========================
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

    // =========================
    // MOVEMENT
    // =========================
    void MoveAlongPath()
    {
        if (isBlocked) return;

        if (currentWaypointIndex >= waypointPath.Count)
        {
            ReachEndOfPath();
            return;
        }

        Transform targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
        if (targetWaypoint == null) return;

        Vector3 targetPos = targetWaypoint.position + Vector3.up * flightHeight;

        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.magnitude > 0.01f)
        {
            transform.position += dir.normalized * speed * Time.deltaTime;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 6f
            );
        }

        if (Vector3.Distance(transform.position, targetPos) <= 0.3f)
        {
            currentWaypointIndex++;
        }
    }

    void InitializePathProgress()
    {
        if (waypointPath == null || waypointPath.Count == 0) return;

        float bestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < waypointPath.Count; i++)
        {
            Transform wp = waypointPath.GetWaypoint(i);
            if (wp == null) continue;

            float dist = Vector3.Distance(transform.position, wp.position);

            if (dist < bestDistance)
            {
                bestDistance = dist;
                closestIndex = i;
            }
        }

        currentWaypointIndex = Mathf.Clamp(closestIndex + 1, 0, waypointPath.Count - 1);
    }

    public void InitializePath(WaypointPath path, int startWaypointIndex)
    {
        waypointPath = path;
        currentWaypointIndex = Mathf.Clamp(startWaypointIndex, 0, path.Count - 1);
        lastPosition = transform.position;
        stuckTimer = 0f;
    }

    void HandleStuckFallback()
    {
        float moved = Vector3.Distance(transform.position, lastPosition);

        if (moved < 0.02f) stuckTimer += Time.deltaTime;
        else stuckTimer = 0f;

        lastPosition = transform.position;

        if (stuckTimer > 0.75f)
        {
            Transform target = waypointPath.GetWaypoint(currentWaypointIndex);

            if (target != null)
            {
                Vector3 dir = target.position - transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.001f)
                    transform.position += dir.normalized * 0.25f;
            }

            stuckTimer = 0f;
        }
    }

    // =========================
    // HEALTH BAR
    // =========================
    void CreateHealthBar()
    {
        healthBarCanvasObj = new GameObject("HealthBar");
        healthBarCanvasObj.transform.SetParent(null);

        Canvas canvas = healthBarCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 20;

        RectTransform rect = healthBarCanvasObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 16);
        healthBarCanvasObj.transform.localScale = Vector3.one * 0.01f;

        // BG
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(healthBarCanvasObj.transform, false);

        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // FILL
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(bg.transform, false);

        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = Color.green;

        RectTransform fillRect = fill.GetComponent<RectTransform>();

        // 🔥 IMPORTANTE: ancora pela esquerda
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.pivot = new Vector2(0, 0.5f);

        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
    }

    float displayedHealthPct = 1f;
    float targetHealthPct = 1f;

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;

        targetHealthPct = Mathf.Clamp01((float)currentHealth / maxHealth);

        // 🔥 PRIMEIRO UPDATE NÃO SUAVIZA (evita travar cheio)
        if (displayedHealthPct <= 0f || displayedHealthPct > 1f)
        {
            displayedHealthPct = targetHealthPct;
            healthBarFill.fillAmount = displayedHealthPct;
        }
    }

    float GetVisualHeight()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>();

        if (rends == null || rends.Length == 0)
            return 2f;

        Bounds bounds = rends[0].bounds;

        for (int i = 1; i < rends.Length; i++)
            bounds.Encapsulate(rends[i].bounds);

        return bounds.size.y;
    }

    // =========================
    // DAMAGE / DEATH
    // =========================
    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Max(0, currentHealth); // 🔥 evita negativo

        ShowDamagePopup(dmg);
        StartCoroutine(HitFlash());

        UpdateHealthBar();

        if (currentHealth <= 0)
            Die(true);
    }

    void ReachEndOfPath()
    {
        if (isDead) return;

        if (GameManager.Instance != null)
            GameManager.Instance.DamageCastle(castleDamage);

        Die(false);
    }

    void Die(bool giveGold)
    {
        Debug.Log("INIMIGO MORREU", this);
        if (isDead) return;

        isDead = true;

        if (giveGold && GameManager.Instance != null)
            GameManager.Instance.AddGold(goldValue);

        if (waveManager != null)
            waveManager.OnEnemyDied();

        if (healthBarCanvasObj != null)
            Destroy(healthBarCanvasObj);

        if (deathEffectPrefab != null)
        {
            GameObject fx = Instantiate(deathEffectPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            Destroy(fx, 1f);
        }
        Destroy(gameObject);
    }
    void OnDestroy()
    {
        if (healthBarCanvasObj != null)
            Destroy(healthBarCanvasObj);
    }
}