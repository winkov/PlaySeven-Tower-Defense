using UnityEngine;

public class BuildSpotGenerator : MonoBehaviour
{
    public GameObject buildSpotPrefab;
    public GameObject towerPrefab;
    public float sideOffset = 2.5f;
    public float spotScale = 1f;
    public int buildCost = 50;
    public bool generateOnStart = true;

    private WaypointPath waypointPath;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateBuildSpots();
        }
    }

    public void GenerateBuildSpots()
    {
        waypointPath = FindAnyObjectByType<WaypointPath>();

        if (waypointPath == null || waypointPath.Count < 2)
        {
            Debug.LogWarning("BuildSpotGenerator needs a WaypointPath with at least 2 waypoints.", this);
            return;
        }

        for (int i = 0; i < waypointPath.Count - 1; i++)
        {
            Transform start = waypointPath.GetWaypoint(i);
            Transform end = waypointPath.GetWaypoint(i + 1);

            if (start == null || end == null) continue;

            Vector3 middle = (start.position + end.position) * 0.5f;
            Vector3 direction = (end.position - start.position).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;

            CreateBuildSpot(middle + side * sideOffset, "BuildSpot_Left_" + i);
            CreateBuildSpot(middle - side * sideOffset, "BuildSpot_Right_" + i);
        }
    }

    void CreateBuildSpot(Vector3 position, string spotName)
    {
        position.y = 0.05f;

        GameObject spotObject;

        if (buildSpotPrefab != null)
        {
            spotObject = Instantiate(buildSpotPrefab, position, Quaternion.identity);
        }
        else
        {
            spotObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spotObject.transform.position = position;
            spotObject.transform.localScale = new Vector3(spotScale, 0.12f, spotScale);

            Renderer spotRenderer = spotObject.GetComponent<Renderer>();
            if (spotRenderer != null)
            {
                spotRenderer.material.color = Color.green;
            }
        }

        spotObject.name = spotName;

        BuildSpot buildSpot = spotObject.GetComponent<BuildSpot>();
        if (buildSpot == null)
        {
            buildSpot = spotObject.AddComponent<BuildSpot>();
        }

        buildSpot.towerPrefab = towerPrefab;
        buildSpot.buildCost = buildCost;
    }
}
