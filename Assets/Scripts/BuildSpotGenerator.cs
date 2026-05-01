using UnityEngine;

public class BuildSpotGenerator : MonoBehaviour
{
    public GameObject buildSpotPrefab;
    public GameObject towerPrefab;
    public float sideOffset = 7.4f;
    public float spotScale = 1.65f;
    public int buildCost = 50;
    public bool generateOnStart = true;
    public float minDistanceFromPath = 3.6f;

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

        ClearExistingBuildSpots();

        CreateStrategicSpot(0, 1f, 0.55f, "BuildSpot_0");
        CreateStrategicSpot(0, -1f, 0.55f, "BuildSpot_1");
        CreateStrategicSpot(1, 1f, 0.52f, "BuildSpot_2");
        CreateStrategicSpot(1, -1f, 0.52f, "BuildSpot_3");
        CreateStrategicSpot(2, 1f, 0.5f, "BuildSpot_4");
        CreateStrategicSpot(2, -1f, 0.5f, "BuildSpot_5");
        CreateStrategicSpot(3, 1f, 0.5f, "BuildSpot_6");
        CreateStrategicSpot(3, -1f, 0.5f, "BuildSpot_7");
        CreateStrategicSpot(4, 1f, 0.52f, "BuildSpot_8");
        CreateStrategicSpot(4, -1f, 0.52f, "BuildSpot_9");
    }

    void CreateStrategicSpot(int segmentIndex, float sideSign, float along, string spotName)
    {
        if (waypointPath == null || segmentIndex < 0 || segmentIndex >= waypointPath.Count - 1) return;

        Transform start = waypointPath.GetWaypoint(segmentIndex);
        Transform end = waypointPath.GetWaypoint(segmentIndex + 1);
        if (start == null || end == null) return;

        Vector3 middle = Vector3.Lerp(start.position, end.position, along);
        Vector3 direction = (end.position - start.position).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
        Vector3 candidate = middle + side * sideOffset * sideSign;
        candidate = PushAwayFromPath(candidate, side * sideSign);
        CreateBuildSpot(candidate, spotName);
    }

    Vector3 PushAwayFromPath(Vector3 candidate, Vector3 pushDir)
    {
        if (waypointPath == null || waypointPath.Count < 2) return candidate;

        Vector3 adjusted = candidate;
        int guard = 0;
        while (DistanceToPathXZ(adjusted) < minDistanceFromPath && guard < 8)
        {
            adjusted += pushDir.normalized * 0.75f;
            guard++;
        }
        return adjusted;
    }

    float DistanceToPathXZ(Vector3 point)
    {
        float best = float.MaxValue;
        Vector2 p = new Vector2(point.x, point.z);
        for (int i = 0; i < waypointPath.Count - 1; i++)
        {
            Transform a3 = waypointPath.GetWaypoint(i);
            Transform b3 = waypointPath.GetWaypoint(i + 1);
            if (a3 == null || b3 == null) continue;
            Vector2 a = new Vector2(a3.position.x, a3.position.z);
            Vector2 b = new Vector2(b3.position.x, b3.position.z);
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            Vector2 closest = a + ab * t;
            float d = Vector2.Distance(p, closest);
            if (d < best) best = d;
        }
        return best;
    }

    void CreateBuildSpot(Vector3 position, string spotName)
    {
        position.y = 0.08f;

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
                spotRenderer.material.color = new Color(0.63f, 0.72f, 0.53f);
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

    void ClearExistingBuildSpots()
    {
        BuildSpot[] existingSpots = FindObjectsByType<BuildSpot>();
        for (int i = 0; i < existingSpots.Length; i++)
        {
            if (existingSpots[i] != null)
            {
                Destroy(existingSpots[i].gameObject);
            }
        }
    }
}
