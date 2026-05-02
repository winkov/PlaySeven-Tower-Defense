using UnityEngine;

public class BuildSpot : MonoBehaviour
{
    public GameObject towerPrefab;
    public GameObject mageTowerPrefab;
    public GameObject archerTowerPrefab;
    public GameObject catapultTowerPrefab;
    public GameObject barracksTowerPrefab;
    public int buildCost = 50;

    public Color availableColor = new Color(0.72f, 0.78f, 0.58f);
    public Color occupiedColor = new Color(0.36f, 0.34f, 0.3f);

    private GameManager gameManager;
    private bool hasTower;
    private bool previewSelected;
    private int lastClickFrame = -1;
    private Renderer spotRenderer;
    private Tower builtTower;

    private Transform ringVisual;
    private Transform baseVisual;
    private Renderer ringRenderer;
    private Renderer baseRenderer;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

#if UNITY_EDITOR
        AutoAssignTowerPrefabsInEditor();
#endif

        spotRenderer = GetComponent<Renderer>();

        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.1f, 0f);
            box.size = new Vector3(1.8f, 0.3f, 1.8f);
        }

        CreateSpotVisuals();
        UpdateVisual();
    }

    void OnMouseDown()
    {
        // 🔥 BLOQUEIA se clicou através da UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        // 🔥 NÃO FAZ NADA se já tem torre
        if (hasTower)
            return;

        UIManager manager = FindAnyObjectByType<UIManager>();
        if (manager != null)
        {
            manager.ShowBuildChoice(this);
        }
    }

    public void ShowPreview()
    {
        if (hasTower) return;
        previewSelected = true;
        UpdateVisual();
    }

    public void ClearPreview()
    {
        previewSelected = false;
        UpdateVisual();
    }

    public void SetBuildPreviewSelected(bool selected)
    {
        previewSelected = selected;
        UpdateVisual();
    }

    // =========================
    // BUILD
    // =========================

    public void BuildTower(TowerType towerType)
    {
        if (hasTower || gameManager == null) return;
        if (!gameManager.SpendGold(buildCost)) return;

        CreateTower(towerType);
        hasTower = true;

        ClearPreview(); // 🔥 garante limpar highlight
        UpdateVisual();
    }

    void CreateTower(TowerType towerType)
    {
        Vector3 towerPosition = transform.position;

        GameObject towerObject;
        GameObject selectedPrefab = GetTowerPrefabByType(towerType);

        if (selectedPrefab != null)
        {
            towerObject = Instantiate(selectedPrefab, towerPosition + Vector3.up * 0.8f, Quaternion.identity);
            towerObject.transform.localScale = Vector3.one * 0.56f;
        }
        else
        {
            towerObject = new GameObject("Runtime Tower");
            towerObject.transform.position = towerPosition + Vector3.up * 0.5f;
        }

        Transform firePoint = selectedPrefab == null
            ? CreateVisualByType(towerObject.transform, towerType)
            : FindFirePointFromPrefab(towerObject.transform);

        builtTower = towerObject.GetComponent<Tower>();
        if (builtTower == null)
            builtTower = towerObject.AddComponent<Tower>();

        builtTower.firePoint = firePoint;
        builtTower.debugLogs = false;
        builtTower.ConfigureByType(towerType);

        EnsureTowerSelectionCollider(towerObject);
    }

    GameObject GetTowerPrefabByType(TowerType towerType)
    {
        if (towerType == TowerType.Mage) return mageTowerPrefab != null ? mageTowerPrefab : towerPrefab;
        if (towerType == TowerType.Archer) return archerTowerPrefab != null ? archerTowerPrefab : towerPrefab;
        if (towerType == TowerType.Catapult) return catapultTowerPrefab != null ? catapultTowerPrefab : towerPrefab;
        return barracksTowerPrefab != null ? barracksTowerPrefab : towerPrefab;
    }

    Transform FindFirePointFromPrefab(Transform root)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            string n = children[i].name.ToLowerInvariant();
            if (n.Contains("muzzle") || n.Contains("fire") || n.Contains("tip") || n.Contains("head"))
                return children[i];
        }

        return root;
    }

#if UNITY_EDITOR
    void AutoAssignTowerPrefabsInEditor()
    {
        if (mageTowerPrefab == null) mageTowerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TowerDefenceAssets/Prefabs/Towers/Tower_11.prefab");
        if (archerTowerPrefab == null) archerTowerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TowerDefenceAssets/Prefabs/Towers/Tower_2.prefab");
        if (catapultTowerPrefab == null) catapultTowerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TowerDefenceAssets/Prefabs/Towers/Tower_14.prefab");
        if (barracksTowerPrefab == null) barracksTowerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TowerDefenceAssets/Prefabs/Towers/Tower_6.prefab");
    }
#endif

    // =========================
    // VISUAL
    // =========================

    void CreateSpotVisuals()
    {
        if (transform.Find("PedestalBase") == null)
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "PedestalBase";
            pedestal.transform.SetParent(transform, false);
            pedestal.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            pedestal.transform.localScale = new Vector3(1.45f, 0.18f, 1.45f);

            Destroy(pedestal.GetComponent<Collider>());

            baseRenderer = pedestal.GetComponent<Renderer>();
            baseVisual = pedestal.transform;
        }
        else
        {
            baseVisual = transform.Find("PedestalBase");
            baseRenderer = baseVisual.GetComponent<Renderer>();
        }

        if (transform.Find("PedestalRing") == null)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "PedestalRing";
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            ring.transform.localScale = new Vector3(1.08f, 0.06f, 1.08f);

            Destroy(ring.GetComponent<Collider>());

            ringRenderer = ring.GetComponent<Renderer>();
            ringVisual = ring.transform;
        }
        else
        {
            ringVisual = transform.Find("PedestalRing");
            ringRenderer = ringVisual.GetComponent<Renderer>();
        }
    }

    void UpdateVisual()
    {
        Color topColor = hasTower ? occupiedColor : availableColor;
        Color ringColor = hasTower ? new Color(0.28f, 0.26f, 0.24f) : new Color(0.88f, 0.82f, 0.52f);

        Vector3 scale = new Vector3(1.65f, 0.12f, 1.65f);

        if (previewSelected && !hasTower)
        {
            topColor = new Color(1f, 0.87f, 0.24f);
            ringColor = new Color(1f, 0.94f, 0.62f);
            scale = new Vector3(1.82f, 0.12f, 1.82f);
        }

        transform.localScale = scale;

        if (spotRenderer != null) spotRenderer.material.color = topColor;
        if (baseRenderer != null) baseRenderer.material.color = new Color(0.41f, 0.35f, 0.24f);
        if (ringRenderer != null) ringRenderer.material.color = ringColor;

        if (ringVisual != null)
        {
            ringVisual.localScale = previewSelected && !hasTower
                ? new Vector3(1.18f, 0.06f, 1.18f)
                : new Vector3(1.08f, 0.06f, 1.08f);
        }
    }
    public void ShowPreview(TowerType type)
    {
        ShowPreview(); // ignora tipo por enquanto
    }
    void EnsureTowerSelectionCollider(GameObject towerObject)
    {
        if (towerObject == null) return;

        Renderer[] rends = towerObject.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;

        Bounds bounds = rends[0].bounds;

        for (int i = 1; i < rends.Length; i++)
            bounds.Encapsulate(rends[i].bounds);

        // remove collider antigo (se existir)
        Collider existing = towerObject.GetComponent<Collider>();
        if (existing != null)
            Destroy(existing);

        BoxCollider box = towerObject.AddComponent<BoxCollider>();

        // 🔥 converte bounds global → local
        box.center = towerObject.transform.InverseTransformPoint(bounds.center);

        Vector3 size = bounds.size;
        size.y += 0.5f; // aumenta um pouco pra facilitar clique

        box.size *= 1.1f;
    }
    Transform CreateVisualByType(Transform root, TowerType type)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        part.transform.SetParent(root);
        part.transform.localPosition = new Vector3(0f, 1f, 0f);
        part.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

        Destroy(part.GetComponent<Collider>());

        Renderer r = part.GetComponent<Renderer>();

        if (r != null)
        {
            switch (type)
            {
                case TowerType.Mage:
                    r.material.color = Color.blue;
                    break;

                case TowerType.Archer:
                    r.material.color = new Color(0.6f, 0.4f, 0.2f);
                    break;

                case TowerType.Catapult:
                    r.material.color = Color.gray;
                    break;

                case TowerType.Barracks:
                    r.material.color = Color.red;
                    break;
            }
        }

        return part.transform;
    }
}
