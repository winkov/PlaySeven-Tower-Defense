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
    private Tower builtTower;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        spotRenderer = GetComponent<Renderer>();
        
        // Garantir que o BuildSpot tem Collider para receber cliques
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1f, 0.1f, 1f);
        }
        
        UpdateVisual();
    }

    void OnMouseDown()
    {
        Debug.Log("BuildSpot clicked! BuildMode: " + (BuildManager.Instance != null ? BuildManager.Instance.IsBuildModeActive : false), this);
        
        if (hasTower)
        {
            if (builtTower != null)
            {
                UIManager uiManager = FindAnyObjectByType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.SelectTower(builtTower);
                }
            }
            return;
        }

        if (BuildManager.Instance != null && !BuildManager.Instance.IsBuildModeActive)
        {
            Debug.Log("Build mode is not active, cannot build", this);
            return;
        }
        
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found", this);
            return;
        }

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
        else
        {
            Debug.Log("Not enough gold to build tower", this);
        }
    }

    void CreateTower()
    {
        Vector3 towerPosition = transform.position;

        GameObject towerObject = null;

        if (towerPrefab != null)
        {
            towerObject = Instantiate(towerPrefab, towerPosition + Vector3.up * 0.6f, Quaternion.identity);
        }
        else
        {
            towerObject = new GameObject("Runtime Tower");
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
        }

        GameObject crystalObject = CreateTowerPart("Magic Crystal", PrimitiveType.Sphere, towerObject.transform);
        crystalObject.transform.localPosition = new Vector3(0f, 2.0f, 0f);
        crystalObject.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
        SetColor(crystalObject, new Color(0.2f, 0.7f, 1f));

        Transform firePoint = crystalObject.transform;

        builtTower = towerObject.AddComponent<Tower>();
        builtTower.range = 8f;
        builtTower.fireRate = 1f;
        builtTower.damage = 20;
        builtTower.firePoint = firePoint;
        builtTower.debugLogs = false;
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
