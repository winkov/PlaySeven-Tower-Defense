using UnityEngine;

public class BuildSpot : MonoBehaviour
{
    public GameObject towerPrefab;
    public int buildCost = 50;
    public Color availableColor = new Color(0.45f, 0.62f, 0.45f);
    public Color occupiedColor = new Color(0.35f, 0.35f, 0.35f);

    private GameManager gameManager;
    private bool hasTower = false;
    private Renderer spotRenderer;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        spotRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    void OnMouseDown()
    {
        if (hasTower) return;
        if (BuildManager.Instance != null && !BuildManager.Instance.IsBuildModeActive) return;
        if (gameManager == null) return;

        if (gameManager.SpendGold(buildCost))
        {
            CreateTower();
            hasTower = true;
            UpdateVisual();

            if (BuildManager.Instance != null)
            {
                BuildManager.Instance.SetBuildMode(false);
            }
        }
    }

    void CreateTower()
    {
        Vector3 towerPosition = transform.position;

        if (towerPrefab != null)
        {
            Instantiate(towerPrefab, towerPosition + Vector3.up * 0.6f, Quaternion.identity);
            return;
        }

        GameObject towerObject = new GameObject("Runtime Tower");
        towerObject.name = "Runtime Tower";
        towerObject.transform.position = towerPosition + Vector3.up * 0.05f;

        GameObject baseObject = CreateTowerPart("Stone Base", PrimitiveType.Cylinder, towerObject.transform);
        baseObject.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        baseObject.transform.localScale = new Vector3(1.1f, 0.25f, 1.1f);
        SetColor(baseObject, new Color(0.35f, 0.35f, 0.38f));

        GameObject bodyObject = CreateTowerPart("Stone Body", PrimitiveType.Cylinder, towerObject.transform);
        bodyObject.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        bodyObject.transform.localScale = new Vector3(0.75f, 1.2f, 0.75f);
        SetColor(bodyObject, new Color(0.46f, 0.46f, 0.5f));

        GameObject roofObject = CreateTowerPart("Mage Roof", PrimitiveType.Cylinder, towerObject.transform);
        roofObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        roofObject.transform.localScale = new Vector3(0.95f, 0.35f, 0.95f);
        SetColor(roofObject, new Color(0.25f, 0.18f, 0.55f));

        GameObject crystalObject = CreateTowerPart("Magic Crystal", PrimitiveType.Sphere, towerObject.transform);
        crystalObject.transform.localPosition = new Vector3(0f, 2.0f, 0f);
        crystalObject.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
        SetColor(crystalObject, new Color(0.2f, 0.7f, 1f));

        Transform firePoint = crystalObject.transform;

        Tower tower = towerObject.AddComponent<Tower>();
        tower.range = 8f;
        tower.fireRate = 1f;
        tower.damage = 20;
        tower.firePoint = firePoint;
        tower.debugLogs = false;
    }

    void UpdateVisual()
    {
        if (spotRenderer == null) return;

        spotRenderer.material.color = hasTower ? occupiedColor : availableColor;
    }

    GameObject CreateTowerPart(string partName, PrimitiveType primitiveType, Transform parent)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = partName;
        part.transform.SetParent(parent);
        part.transform.localRotation = Quaternion.identity;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return part;
    }

    void SetColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}
