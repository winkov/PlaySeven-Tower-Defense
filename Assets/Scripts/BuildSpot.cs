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
            box.size = new Vector3(1.8f, 0.25f, 1.8f);
        }
        CreateSpotVisuals();
        UpdateVisual();
    }

    void OnMouseDown()
    {
        if (hasTower)
        {
            UIManager ui = FindAnyObjectByType<UIManager>();
            if (ui != null && builtTower != null) ui.SelectTower(builtTower);
            return;
        }

        UIManager manager = FindAnyObjectByType<UIManager>();
        if (manager != null)
        {
            manager.ShowBuildChoice(this);
        }
    }

    public void BuildTower(TowerType towerType)
    {
        if (hasTower || gameManager == null) return;
        if (!gameManager.SpendGold(buildCost)) return;

        CreateTower(towerType);
        hasTower = true;
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

        Transform firePoint = selectedPrefab == null ? CreateVisualByType(towerObject.transform, towerType) : FindFirePointFromPrefab(towerObject.transform);
        builtTower = towerObject.GetComponent<Tower>();
        if (builtTower == null) builtTower = towerObject.AddComponent<Tower>();
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
            {
                return children[i];
            }
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

    Transform CreateVisualByType(Transform root, TowerType type)
    {
        if (type == TowerType.Mage)
        {
            GameObject baseObj = CreatePart("Base", PrimitiveType.Cylinder, root, new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.2f, 0.9f), new Color(0.28f, 0.28f, 0.35f));
            GameObject body = CreatePart("Body", PrimitiveType.Cylinder, root, new Vector3(0f, 0.85f, 0f), new Vector3(0.6f, 1f, 0.6f), new Color(0.4f, 0.4f, 0.55f));
            GameObject crystal = CreatePart("Crystal", PrimitiveType.Sphere, root, new Vector3(0f, 1.8f, 0f), new Vector3(0.3f, 0.3f, 0.3f), new Color(0.2f, 0.7f, 1f));
            return crystal.transform;
        }
        if (type == TowerType.Archer)
        {
            GameObject baseObj = CreatePart("Base", PrimitiveType.Cylinder, root, new Vector3(0f, 0.18f, 0f), new Vector3(0.95f, 0.18f, 0.95f), new Color(0.35f, 0.25f, 0.18f));
            GameObject post = CreatePart("Post", PrimitiveType.Cylinder, root, new Vector3(0f, 0.9f, 0f), new Vector3(0.35f, 1.1f, 0.35f), new Color(0.45f, 0.3f, 0.2f));
            GameObject bowTop = CreatePart("BowTop", PrimitiveType.Cube, root, new Vector3(0f, 1.8f, 0f), new Vector3(1.1f, 0.1f, 0.1f), new Color(0.55f, 0.35f, 0.2f));
            return bowTop.transform;
        }
        if (type == TowerType.Catapult)
        {
            GameObject baseObj = CreatePart("Base", PrimitiveType.Cube, root, new Vector3(0f, 0.25f, 0f), new Vector3(1.2f, 0.5f, 1.2f), new Color(0.35f, 0.32f, 0.28f));
            GameObject arm = CreatePart("Arm", PrimitiveType.Cube, root, new Vector3(0f, 1.0f, 0.1f), new Vector3(0.18f, 0.18f, 1.4f), new Color(0.45f, 0.38f, 0.24f));
            GameObject rock = CreatePart("Rock", PrimitiveType.Sphere, root, new Vector3(0f, 1.0f, 0.85f), new Vector3(0.35f, 0.35f, 0.35f), new Color(0.32f, 0.32f, 0.34f));
            return rock.transform;
        }

        GameObject barracks = CreatePart("BarracksBase", PrimitiveType.Cube, root, new Vector3(0f, 0.35f, 0f), new Vector3(1.4f, 0.7f, 1.4f), new Color(0.42f, 0.38f, 0.32f));
        GameObject banner = CreatePart("Banner", PrimitiveType.Cube, root, new Vector3(0f, 1.2f, 0f), new Vector3(0.2f, 0.8f, 0.2f), new Color(0.2f, 0.2f, 0.2f));
        return banner.transform;
    }

    GameObject CreatePart(string name, PrimitiveType primitive, Transform parent, Vector3 localPos, Vector3 localScale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.transform.localRotation = Quaternion.identity;
        Collider c = go.GetComponent<Collider>();
        if (c != null) Destroy(c);
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.material.color = color;
        return go;
    }

    void CreateSpotVisuals()
    {
        if (transform.Find("PedestalBase") == null)
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "PedestalBase";
            pedestal.transform.SetParent(transform, false);
            pedestal.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            pedestal.transform.localScale = new Vector3(1.45f, 0.18f, 1.45f);
            Collider collider = pedestal.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
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
            Collider collider = ring.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
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
        if (spotRenderer == null && baseRenderer == null && ringRenderer == null) return;

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
        if (ringVisual != null) ringVisual.localScale = previewSelected && !hasTower ? new Vector3(1.18f, 0.06f, 1.18f) : new Vector3(1.08f, 0.06f, 1.08f);
    }

    public void SetBuildPreviewSelected(bool selected)
    {
        previewSelected = selected;
        UpdateVisual();
    }

    void EnsureTowerSelectionCollider(GameObject towerObject)
    {
        if (towerObject == null) return;
        if (towerObject.GetComponent<Collider>() != null) return;
        SphereCollider selectionCollider = towerObject.AddComponent<SphereCollider>();
        selectionCollider.center = new Vector3(0f, 1f, 0f);
        selectionCollider.radius = 0.8f;
    }
}
