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
    public int segmentSpacing = 2;
    public int buildCost = 50;
    public bool generateOnStart = true;

    public int spotsPerSide = 5;

    private WaypointPath waypointPath;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateBuildSpots();
        }
    }

    // =========================
    // GENERATION
    // =========================

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

            if (a == null || b == null) continue;

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
            Transform a = waypointPath.GetWaypoint(i);
            Transform b = waypointPath.GetWaypoint(i + 1);

            if (a == null || b == null) continue;

            length += Vector3.Distance(a.position, b.position);
        }

        return length;
    }

    // =========================
    // POSITIONING
    // =========================

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

    // =========================
    // BUILD SPOT CREATION
    // =========================

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