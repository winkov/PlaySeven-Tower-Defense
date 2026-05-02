using UnityEngine;

public class BuildSpotGenerator : MonoBehaviour
{
    public GameObject buildSpotPrefab;
    public GameObject towerPrefab;

    [Header("Layout")]
    public float sideOffset = 3.5f;
    public float spotScale = 1.4f;
    public float minDistanceFromPath = 2.2f;

    [Header("Generation")]
    public int segmentSpacing = 2; // a cada X segmentos gera spots
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

    public int spotsPerSide = 5;

    public void GenerateBuildSpots()
    {
        waypointPath = FindAnyObjectByType<WaypointPath>();

        if (waypointPath == null || waypointPath.Count < 2)
        {
            Debug.LogWarning("No path found");
            return;
        }

        ClearExistingBuildSpots();

        float totalLength = GetPathLength();
        float spacing = totalLength / (spotsPerSide * 2f);

        float currentDistance = 0f;
        int index = 0;

        for (int i = 0; i < waypointPath.Count - 1; i++)
        {
            Transform a = waypointPath.GetWaypoint(i);
            Transform b = waypointPath.GetWaypoint(i + 1);

            float segmentLength = Vector3.Distance(a.position, b.position);
            Vector3 dir = (b.position - a.position).normalized;

            while (currentDistance < segmentLength)
            {
                Vector3 point = a.position + dir * currentDistance;
                Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;

                Vector3 right = PushAwayFromPath(point + side * sideOffset, side);
                Vector3 left = PushAwayFromPath(point - side * sideOffset, -side);

                CreateBuildSpot(right, "R_" + index);
                CreateBuildSpot(left, "L_" + index);

                currentDistance += spacing;
                index++;
            }

            currentDistance -= segmentLength;
        }
    }

    float GetPathLength()
    {
        float length = 0f;

        for (int i = 0; i < waypointPath.Count - 1; i++)
        {
            length += Vector3.Distance(
                waypointPath.GetWaypoint(i).position,
                waypointPath.GetWaypoint(i + 1).position
            );
        }

        return length;
    }

    void CreateStrategicSpot(int segmentIndex, float sideSign, float along, string spotName)
    {
        Transform start = waypointPath.GetWaypoint(segmentIndex);
        Transform end = waypointPath.GetWaypoint(segmentIndex + 1);

        if (start == null || end == null) return;

        Vector3 middle = Vector3.Lerp(start.position, end.position, along);

        Vector3 direction = (end.position - start.position).normalized;

        // 🔥 lado perpendicular ao caminho
        Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;

        Vector3 candidate = middle + side * sideOffset * sideSign;

        // 🔥 evita colar no path
        candidate = PushAwayFromPath(candidate, side * sideSign);

        CreateBuildSpot(candidate, spotName);
    }

    Vector3 PushAwayFromPath(Vector3 candidate, Vector3 pushDir)
    {
        Vector3 adjusted = candidate;

        int guard = 0;

        while (DistanceToPathXZ(adjusted) < minDistanceFromPath && guard < 10)
        {
            adjusted += pushDir.normalized * 0.6f;
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

            float t = ab.sqrMagnitude > 0.001f
                ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude)
                : 0f;

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
            spotObject.transform.localScale = new Vector3(spotScale, 0.1f, spotScale);

            Renderer r = spotObject.GetComponent<Renderer>();
            if (r != null)
                r.material.color = new Color(0.65f, 0.75f, 0.55f);

            // 🔥 GARANTE collider ativo
            Collider col = spotObject.GetComponent<Collider>();
            if (col == null)
                col = spotObject.AddComponent<CapsuleCollider>();

            col.isTrigger = false;
        }

        spotObject.name = spotName;

        BuildSpot buildSpot = spotObject.GetComponent<BuildSpot>();
        if (buildSpot == null)
            buildSpot = spotObject.AddComponent<BuildSpot>();

        buildSpot.towerPrefab = towerPrefab;
        buildSpot.buildCost = buildCost;
    }

    void ClearExistingBuildSpots()
    {
        BuildSpot[] existing = FindObjectsByType<BuildSpot>();

        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i] != null)
                Destroy(existing[i].gameObject);
        }
    }
}