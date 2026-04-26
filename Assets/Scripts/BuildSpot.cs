using UnityEngine;

public class BuildSpot : MonoBehaviour
{
    public GameObject towerPrefab;
    public int buildCost = 50;
    public Color availableColor = Color.green;
    public Color occupiedColor = Color.gray;

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
        Vector3 towerPosition = transform.position + Vector3.up * 0.6f;

        if (towerPrefab != null)
        {
            Instantiate(towerPrefab, towerPosition, Quaternion.identity);
            return;
        }

        GameObject towerObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        towerObject.name = "Runtime Tower";
        towerObject.transform.position = towerPosition;
        towerObject.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);

        Renderer towerRenderer = towerObject.GetComponent<Renderer>();
        if (towerRenderer != null)
        {
            towerRenderer.material.color = Color.blue;
        }

        Tower tower = towerObject.AddComponent<Tower>();
        tower.range = 8f;
        tower.fireRate = 1f;
        tower.damage = 20;
    }

    void UpdateVisual()
    {
        if (spotRenderer == null) return;

        spotRenderer.material.color = hasTower ? occupiedColor : availableColor;
    }
}
