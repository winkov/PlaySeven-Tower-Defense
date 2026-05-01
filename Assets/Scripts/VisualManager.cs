using UnityEngine;

public class VisualManager : MonoBehaviour
{
    public bool setupCamera = true;
    public bool colorExistingObjects = true;
    public bool createPathVisuals = true;
    public bool createCastleVisual = true;
    public bool createPathBorders = true;
    public bool createEnvironmentProps = true;

    public float pathWidth = 3.1f;
    public float pathHeight = 0.06f;
    public int treesPerPathSegment = 4;
    public int rocksPerPathSegment = 2;

    public Color groundColor = new Color(0.08f, 0.28f, 0.12f);
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
        CreateMaterials();
        CreateVisualRoot();

        if (colorExistingObjects)
        {
            ColorGround();
            ColorWaypointSpheres();
        }

        WaypointPath waypointPath = FindAnyObjectByType<WaypointPath>();
        if (createPathVisuals) CreatePathSegments(waypointPath);
        if (createCastleVisual) CreateCastle(waypointPath);
        if (createEnvironmentProps) CreateEnvironmentProps(waypointPath);
        if (setupCamera) SetupCamera();
    }

    void CreateMaterials()
    {
        groundMaterial = CreateMaterial("Runtime Ground Material", groundColor);
        pathMaterial = CreateMaterial("Runtime Path Material", new Color(0.36f, 0.25f, 0.13f));
        waypointMaterial = CreateMaterial("Runtime Waypoint Material", waypointColor);
        rockMaterial = rockMaterialOverride != null ? rockMaterialOverride : CreateMaterial("Runtime Rock Material", new Color(0.32f, 0.32f, 0.34f));
    }

    Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = materialName;
        material.color = color;
        material.SetFloat("_Glossiness", 0.06f);
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
        CreateGroundTiles(ground.transform.position);
    }

    void CreateGroundTiles(Vector3 center)
    {
        if (groundTilePrefab == null) return;
        for (int x = -4; x <= 4; x++)
        {
            for (int z = -4; z <= 4; z++)
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
            Renderer renderer = waypoint.GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.material = waypointMaterial;
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

            Vector3 a = start.position; a.y = 0.03f;
            Vector3 b = end.position; b.y = 0.03f;
            Vector3 dir = b - a;
            float len = dir.magnitude;
            if (len < 0.01f) continue;

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "Path Segment " + (i + 1);
            segment.transform.SetParent(visualRoot);
            segment.transform.position = (a + b) * 0.5f;
            segment.transform.rotation = Quaternion.LookRotation(dir.normalized);
            segment.transform.localScale = new Vector3(pathWidth, pathHeight, len + 0.15f);
            Renderer r = segment.GetComponent<Renderer>();
            if (r != null) r.material = pathMaterial;
            Collider c = segment.GetComponent<Collider>();
            if (c != null) c.enabled = false;

            if (createPathBorders)
            {
                Vector3 side = Vector3.Cross(Vector3.up, dir.normalized).normalized;
                CreatePathBorder(segment.transform.position + side * (pathWidth * 0.53f), dir.normalized, len, i, "L");
                CreatePathBorder(segment.transform.position - side * (pathWidth * 0.53f), dir.normalized, len, i, "R");
            }
        }
    }

    void CreatePathBorder(Vector3 pos, Vector3 dir, float len, int index, string side)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = "Path Border " + side + " " + (index + 1);
        border.transform.SetParent(visualRoot);
        border.transform.position = pos + Vector3.up * 0.05f;
        border.transform.rotation = Quaternion.LookRotation(dir);
        border.transform.localScale = new Vector3(0.11f, 0.09f, Mathf.Max(0.1f, len - 0.2f));
        Renderer r = border.GetComponent<Renderer>();
        if (r != null) r.material = rockMaterial;
        Collider c = border.GetComponent<Collider>();
        if (c != null) c.enabled = false;
    }

    void CreateCastle(WaypointPath waypointPath)
    {
        if (waypointPath == null || waypointPath.Count == 0) return;
        Transform lastWaypoint = waypointPath.GetWaypoint(waypointPath.Count - 1);
        if (lastWaypoint == null) return;

        GameObject existingCastle = GameObject.Find("Castle");
        if (existingCastle != null)
        {
            Renderer existingRenderer = existingCastle.GetComponent<Renderer>();
            if (existingRenderer != null) existingRenderer.enabled = false;
        }

        if (castlePrefab == null) return;
        GameObject castleAsset = Instantiate(castlePrefab, lastWaypoint.position + new Vector3(0f, 0.02f, 0f), Quaternion.Euler(0f, 180f, 0f), visualRoot);
        castleAsset.name = "Castle";
        castleAsset.transform.localScale = Vector3.one * 0.65f;

        if (castleMaterialOverride != null)
        {
            Renderer[] rends = castleAsset.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                rends[i].material = castleMaterialOverride;
            }
        }
    }

    void CreateEnvironmentProps(WaypointPath waypointPath)
    {
        if (waypointPath == null || waypointPath.Count < 2) return;

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
                float dist = Random.Range(5.2f, 7.8f);
                CreateTree(basePos + side * sign * dist);
            }

            for (int r = 0; r < rocksPerPathSegment; r++)
            {
                float pct = Random.Range(0.2f, 0.8f);
                Vector3 basePos = Vector3.Lerp(start.position, end.position, pct);
                float sign = r % 2 == 0 ? -1f : 1f;
                float dist = Random.Range(3.7f, 5.4f);
                CreateRock(basePos + side * sign * dist);
            }
        }
    }

    void CreateTree(Vector3 position)
    {
        position.y = 0f;
        if (treePrefabOverride == null) return;
        GameObject treeAsset = Instantiate(treePrefabOverride, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), visualRoot);
        float s = Random.Range(1.35f, 1.95f);
        treeAsset.transform.localScale = Vector3.one * s;
    }

    void CreateRock(Vector3 position)
    {
        position.y = 0.04f;
        if (rockPrefabOverride == null) return;
        GameObject rockAsset = Instantiate(rockPrefabOverride, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), visualRoot);
        float s = Random.Range(1.2f, 1.8f);
        rockAsset.transform.localScale = Vector3.one * s;

        if (rockMaterialOverride != null)
        {
            Renderer[] rends = rockAsset.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++) rends[i].material = rockMaterialOverride;
        }
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        mainCamera.transform.position = new Vector3(0f, 8.1f, -5.2f);
        mainCamera.transform.rotation = Quaternion.Euler(66f, 0f, 0f);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 4.9f;
    }

#if UNITY_EDITOR
    void AutoAssignEnvironmentPrefabsInEditor()
    {
        if (treePrefabOverride == null) treePrefabOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Meshtint Free Toon Assets/Santa Claus/Prefabs/Tree 01.prefab");
        if (rockPrefabOverride == null) rockPrefabOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Meshtint Free Toon Assets/Santa Claus/Prefabs/Rock 01.prefab");
        if (groundTilePrefab == null) groundTilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Meshtint Free Toon Assets/Cars and City Pack/Prefabs/Grass 01.prefab");
        if (castlePrefab == null) castlePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low-Poly medival defense/Models/Gate.fbx");
        if (castleMaterialOverride == null) castleMaterialOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Low-Poly medival defense/Materials/Gate_Diffuse.mat");
        if (rockMaterialOverride == null) rockMaterialOverride = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Low-Poly medival defense/Materials/TNT_FULL.mat");
    }
#endif
}
