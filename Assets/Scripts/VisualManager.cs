using UnityEngine;

public class VisualManager : MonoBehaviour
{
    public bool setupCamera = true;
    public bool colorExistingObjects = true;
    public bool createPathVisuals = true;
    public bool createCastleVisual = true;
    public bool createPathBorders = true;
    public bool createEnvironmentProps = true;
    public bool hideWaypointMeshes = true;
    public bool useGroundTiles = false;

    public float pathWidth = 5.2f;
    public float pathHeight = 0.1f;
    public float pathShoulderWidth = 0.9f;
    public int treesPerPathSegment = 1;
    public int rocksPerPathSegment = 1;
    public float castleDistanceForward = 3.2f;
    public float castleScale = 0.52f;
    public float borderPadding = 4.8f;

    public Color groundColor = new Color(0.42f, 0.67f, 0.24f);
    public Color waypointColor = new Color(1f, 0.75f, 0.1f);

    private Material groundMaterial;
    private Material pathMaterial;
    private Material waypointMaterial;
    private Material rockMaterial;
    private Transform visualRoot;

    public GameObject treePrefabOverride;
    public GameObject rockPrefabOverride;
    public GameObject groundTilePrefab;
    public GameObject castlePrefab;

    public Material pathMaterialOverride;
    public Material castleMaterialOverride;
    public Material rockMaterialOverride;

    void Start()
    {
#if UNITY_EDITOR
        AutoAssignEnvironmentPrefabsInEditor();
#endif
        RefreshWorldVisuals();
    }

    public void RefreshWorldVisuals()
    {
        CreateMaterials();
        CreateVisualRoot();

        WaypointPath waypointPath = FindAnyObjectByType<WaypointPath>();

        if (colorExistingObjects)
        {
            ColorGround();
            ColorWaypointSpheres();
        }

        if (createPathVisuals) CreatePathSegments(waypointPath);
        if (createCastleVisual) CreateCastle(waypointPath);
        if (createEnvironmentProps) CreateEnvironmentProps(waypointPath);
        if (setupCamera) SetupCamera(waypointPath);
    }

    void CreateMaterials()
    {
        groundMaterial = CreateMaterial("Runtime Ground Material", groundColor);
        pathMaterial = CreateMaterial("Runtime Path Material", new Color(0.88f, 0.70f, 0.46f));
        waypointMaterial = CreateMaterial("Runtime Waypoint Material", waypointColor);
        rockMaterial = rockMaterialOverride != null ? rockMaterialOverride : CreateMaterial("Runtime Rock Material", new Color(0.32f, 0.32f, 0.34f));
    }

    Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = materialName;
        material.color = color;
        material.SetFloat("_Glossiness", 0.04f);
        return material;
    }

    void CreateVisualRoot()
    {
        GameObject oldRoot = GameObject.Find("Runtime Visuals");
        if (oldRoot != null) Destroy(oldRoot);
        visualRoot = new GameObject("Runtime Visuals").transform;
    }

    void ColorGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return;
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null) renderer.material = groundMaterial;
        if (useGroundTiles) CreateGroundTiles(ground.transform.position);
    }

    void CreateGroundTiles(Vector3 center)
    {
        if (groundTilePrefab == null) return;
        for (int x = -5; x <= 5; x++)
        {
            for (int z = -5; z <= 5; z++)
            {
                Vector3 p = center + new Vector3(x * 6f, 0.02f, z * 6f);
                GameObject tile = Instantiate(groundTilePrefab, p, Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f), visualRoot);
                tile.transform.localScale = new Vector3(2.8f, 1f, 2.8f);
            }
        }
    }

    void ColorWaypointSpheres()
    {
        WaypointPath waypointPath = FindAnyObjectByType<WaypointPath>();
        if (waypointPath == null) return;
        for (int i = 0; i < waypointPath.Count; i++)
        {
            Transform waypoint = waypointPath.GetWaypoint(i);
            if (waypoint == null) continue;
            Renderer[] renderers = waypoint.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                if (hideWaypointMeshes) renderers[r].enabled = false;
                else renderers[r].material = waypointMaterial;
            }
        }
    }

    void CreatePathSegments(WaypointPath waypointPath)
    {
        if (waypointPath == null || waypointPath.Count < 2) return;
        for (int i = 0; i < waypointPath.Count - 1; i++)
        {
            Transform start = waypointPath.GetWaypoint(i);
            Transform end = waypointPath.GetWaypoint(i + 1);
            if (start == null || end == null) continue;

            Vector3 a = start.position; a.y = 0.12f;
            Vector3 b = end.position; b.y = 0.12f;
            Vector3 dir = b - a;
            float len = dir.magnitude;
            if (len < 0.01f) continue;

            CreatePathShoulder(a, dir.normalized, len, i);

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "Path Segment " + (i + 1);
            segment.transform.SetParent(visualRoot);
            segment.transform.position = (a + b) * 0.5f;
            segment.transform.rotation = Quaternion.LookRotation(dir.normalized);
            segment.transform.localScale = new Vector3(pathWidth, pathHeight, len + 0.7f);
            Renderer r = segment.GetComponent<Renderer>();
            if (r != null) r.material = pathMaterial;
            Collider c = segment.GetComponent<Collider>();
            if (c != null) c.enabled = false;

            if (createPathBorders) CreatePathBorder(a, dir.normalized, len, i);
        }

        for (int i = 1; i < waypointPath.Count - 1; i++)
        {
            Transform waypoint = waypointPath.GetWaypoint(i);
            if (waypoint != null) CreatePathJoint(waypoint.position, i);
        }
    }

    void CreatePathShoulder(Vector3 start, Vector3 direction, float length, int index)
    {
        GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shoulder.name = "Path Shoulder " + index;
        shoulder.transform.SetParent(visualRoot);
        shoulder.transform.position = start + (direction * (length * 0.5f)) + Vector3.down * 0.025f;
        shoulder.transform.rotation = Quaternion.LookRotation(direction);
        shoulder.transform.localScale = new Vector3(pathWidth + pathShoulderWidth, pathHeight * 0.8f, length + 1.1f);
        Renderer renderer = shoulder.GetComponent<Renderer>();
        if (renderer != null) renderer.material = CreateMaterial("PathShoulderMat_" + index, new Color(0.58f, 0.43f, 0.22f));
        Collider collider = shoulder.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }

    void CreatePathBorder(Vector3 start, Vector3 direction, float length, int index)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = "Path Border " + index;
        border.transform.SetParent(visualRoot);
        border.transform.position = start + (direction * (length * 0.5f));
        border.transform.rotation = Quaternion.LookRotation(direction);
        border.transform.localScale = new Vector3(pathWidth + pathShoulderWidth + 0.7f, pathHeight * 0.45f, length + 1.35f);
        border.transform.position += Vector3.down * 0.045f;
        Renderer renderer = border.GetComponent<Renderer>();
        if (renderer != null) renderer.material = CreateMaterial("PathBorderMat_" + index, new Color(0.24f, 0.43f, 0.12f));
        Collider collider = border.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }

    void CreatePathJoint(Vector3 position, int index)
    {
        GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        joint.name = "Path Joint " + index;
        joint.transform.SetParent(visualRoot);
        joint.transform.position = new Vector3(position.x, 0.11f, position.z);
        joint.transform.localScale = new Vector3((pathWidth + 0.45f) * 0.5f, 0.05f, (pathWidth + 0.45f) * 0.5f);
        Renderer renderer = joint.GetComponent<Renderer>();
        if (renderer != null) renderer.material = pathMaterial;
        Collider collider = joint.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }

    void CreateCastle(WaypointPath waypointPath)
    {
        if (waypointPath == null || waypointPath.Count == 0) return;
        Transform lastWaypoint = waypointPath.GetWaypoint(waypointPath.Count - 1);
        if (lastWaypoint == null) return;

        GameObject existingCastle = GameObject.Find("Castle");
        if (existingCastle != null)
        {
            Renderer[] oldRenderers = existingCastle.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < oldRenderers.Length; i++) oldRenderers[i].enabled = false;
        }

        if (castlePrefab == null) return;
        Transform previousWaypoint = waypointPath.GetWaypoint(Mathf.Max(0, waypointPath.Count - 2));
        Vector3 travelDir = previousWaypoint != null ? (lastWaypoint.position - previousWaypoint.position).normalized : Vector3.forward;
        Vector3 pos = lastWaypoint.position + travelDir * castleDistanceForward;
        pos.y = 0.08f;
        GameObject castleAsset = Instantiate(castlePrefab, pos, Quaternion.LookRotation(-travelDir, Vector3.up), visualRoot);
        castleAsset.name = "Castle";
        castleAsset.transform.localScale = Vector3.one * castleScale;

        if (castleMaterialOverride != null)
        {
            Renderer[] rends = castleAsset.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++) rends[i].material = castleMaterialOverride;
        }
    }

    void CreateEnvironmentProps(WaypointPath waypointPath)
    {
        if (waypointPath == null || waypointPath.Count < 2) return;

        Bounds bounds = GetPathBounds(waypointPath);
        CreateForestBorder(bounds);
        CreateSparseProps(waypointPath, bounds);
    }

    void CreateForestBorder(Bounds bounds)
    {
        float minX = bounds.min.x - borderPadding;
        float maxX = bounds.max.x + borderPadding;
        float minZ = bounds.min.z - borderPadding;
        float maxZ = bounds.max.z + borderPadding;

        for (float x = minX; x <= maxX; x += 5.4f)
        {
            CreateTree(new Vector3(x, 0f, minZ));
            CreateTree(new Vector3(x, 0f, maxZ));
        }

        for (float z = minZ + 5f; z <= maxZ - 5f; z += 5.4f)
        {
            CreateTree(new Vector3(minX, 0f, z));
            CreateTree(new Vector3(maxX, 0f, z));
        }
    }

    void CreateSparseProps(WaypointPath waypointPath, Bounds bounds)
    {
        for (int i = 0; i < waypointPath.Count - 1; i++)
        {
            Transform start = waypointPath.GetWaypoint(i);
            Transform end = waypointPath.GetWaypoint(i + 1);
            if (start == null || end == null) continue;

            Vector3 dir = (end.position - start.position).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;

            for (int t = 0; t < treesPerPathSegment; t++)
            {
                float pct = (t + 1f) / (treesPerPathSegment + 1f);
                Vector3 basePos = Vector3.Lerp(start.position, end.position, pct);
                float sign = t % 2 == 0 ? 1f : -1f;
                float dist = 10.5f;
                Vector3 pos = basePos + side * sign * dist;
                if (IsNearCenter(bounds, pos)) continue;
                CreateTree(pos);
            }

            for (int r = 0; r < rocksPerPathSegment; r++)
            {
                float pct = 0.5f;
                Vector3 basePos = Vector3.Lerp(start.position, end.position, pct);
                Vector3 pos = basePos + side * (r == 0 ? -6.5f : 6.5f);
                if (IsNearCenter(bounds, pos)) continue;
                CreateRock(pos);
            }
        }
    }

    bool IsNearCenter(Bounds bounds, Vector3 position)
    {
        Vector3 center = bounds.center;
        return Mathf.Abs(position.x - center.x) < 10f && Mathf.Abs(position.z - center.z) < 8f;
    }

    void CreateTree(Vector3 position)
    {
        position.y = 0f;
        if (treePrefabOverride == null) return;
        GameObject treeAsset = Instantiate(treePrefabOverride, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), visualRoot);
        float s = Random.Range(1.8f, 2.35f);
        treeAsset.transform.localScale = Vector3.one * s;
    }

    void CreateRock(Vector3 position)
    {
        position.y = 0.04f;
        if (rockPrefabOverride == null) return;
        GameObject rockAsset = Instantiate(rockPrefabOverride, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), visualRoot);
        float s = Random.Range(1f, 1.35f);
        rockAsset.transform.localScale = Vector3.one * s;
        if (rockMaterialOverride != null)
        {
            Renderer[] rends = rockAsset.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++) rends[i].material = rockMaterialOverride;
        }
    }

    void SetupCamera(WaypointPath waypointPath)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        Bounds bounds = waypointPath != null ? GetPathBounds(waypointPath) : new Bounds(Vector3.zero, new Vector3(40f, 0f, 40f));
        Vector3 center = bounds.center;
        cam.transform.position = new Vector3(center.x - 0.2f, 12.8f, center.z - 8.8f);
        cam.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(7.4f, bounds.extents.z + 0.8f);
    }

    Bounds GetPathBounds(WaypointPath waypointPath)
    {
        if (waypointPath == null || waypointPath.Count == 0) return new Bounds(Vector3.zero, new Vector3(40f, 0f, 40f));

        Bounds bounds = new Bounds(waypointPath.GetWaypoint(0).position, Vector3.zero);
        for (int i = 1; i < waypointPath.Count; i++)
        {
            Transform waypoint = waypointPath.GetWaypoint(i);
            if (waypoint != null) bounds.Encapsulate(waypoint.position);
        }
        return bounds;
    }

#if UNITY_EDITOR
    void AutoAssignEnvironmentPrefabsInEditor()
    {
        if (treePrefabOverride == null) treePrefabOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Meshtint Free Toon Assets/Santa Claus/Prefabs/Tree 01.prefab");
        if (rockPrefabOverride == null) rockPrefabOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Meshtint Free Toon Assets/Santa Claus/Prefabs/Rock 01.prefab");
        if (groundTilePrefab == null) groundTilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Meshtint Free Toon Assets/Cars and City Pack/Prefabs/Grass 01.prefab");
        if (castlePrefab == null) castlePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TowerDefenceAssets/Prefabs/Towers/Tower_15.prefab");
        if (castlePrefab == null) castlePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low-Poly medival defense/Models/Gate.fbx");
        if (castleMaterialOverride == null) castleMaterialOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Low-Poly medival defense/Materials/Gate_Diffuse.mat");
        if (rockMaterialOverride == null) rockMaterialOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Low-Poly medival defense/Materials/TNT_FULL.mat");
        if (pathMaterialOverride == null) pathMaterialOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/TowerDefenceAssets/Materials/Demo Scene/RoadBlock.mat");
        if (pathMaterialOverride == null) pathMaterialOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Meshtint Free Toon Assets/Cars and City Pack/Materials/Cross Roads.mat");
    }
#endif
}
