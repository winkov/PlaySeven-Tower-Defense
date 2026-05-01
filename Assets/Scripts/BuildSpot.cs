using UnityEngine;

public class BuildSpot : MonoBehaviour
{
    public GameObject towerPrefab;
    public int buildCost = 50;
    public Color availableColor = new Color(0.45f, 0.62f, 0.45f);
    public Color occupiedColor = new Color(0.35f, 0.35f, 0.35f);

    private GameManager gameManager;
    private bool hasTower;
    private Renderer spotRenderer;
    private Tower builtTower;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        spotRenderer = GetComponent<Renderer>();
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1f, 0.1f, 1f);
        }
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

        if (towerPrefab != null)
        {
            towerObject = Instantiate(towerPrefab, towerPosition + Vector3.up * 0.6f, Quaternion.identity);
        }
        else
        {
            towerObject = new GameObject("Runtime Tower");
            towerObject.transform.position = towerPosition + Vector3.up * 0.05f;
        }

        Transform firePoint = CreateVisualByType(towerObject.transform, towerType);
        builtTower = towerObject.GetComponent<Tower>();
        if (builtTower == null) builtTower = towerObject.AddComponent<Tower>();
        builtTower.firePoint = firePoint;
        builtTower.debugLogs = false;
        builtTower.ConfigureByType(towerType);
        EnsureTowerSelectionCollider(towerObject);
    }

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

    void UpdateVisual()
    {
        if (spotRenderer == null) return;
        spotRenderer.material.color = hasTower ? occupiedColor : availableColor;
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
