using UnityEngine;

public class VisualManager : MonoBehaviour
{
    public bool setupCamera = true;
    public bool colorExistingObjects = true;
    public bool createPathVisuals = true;
    public bool createCastleVisual = true;
    public bool createPathBorders = true;
    public bool createEnvironmentProps = true;

    public float pathWidth = 1.6f;
    public float pathHeight = 0.06f;
    public float castleSize = 0.8f;
    public int treesPerPathSegment = 2;
    public int rocksPerPathSegment = 1;

    public Color groundColor = new Color(0.16f, 0.44f, 0.24f);
    public Color pathColor = new Color(0.55f, 0.34f, 0.16f);
    public Color castleColor = new Color(0.45f, 0.42f, 0.48f);
    public Color waypointColor = new Color(1f, 0.75f, 0.1f);
    public Color treeTrunkColor = new Color(0.35f, 0.18f, 0.08f);
    public Color treeLeafColor = new Color(0.08f, 0.36f, 0.13f);
    public Color rockColor = new Color(0.42f, 0.42f, 0.4f);
    public Color roofColor = new Color(0.26f, 0.16f, 0.45f);

    private Material groundMaterial;
    private Material pathMaterial;
    private Material castleMaterial;
    private Material waypointMaterial;
    private Material treeTrunkMaterial;
    private Material treeLeafMaterial;
    private Material rockMaterial;
    private Material roofMaterial;
    private Transform visualRoot;

    void Start()
    {
        CreateMaterials();
        CreateVisualRoot();

        if (colorExistingObjects)
        {
            ColorGround();
            ColorWaypointSpheres();
        }

        WaypointPath waypointPath = FindAnyObjectByType<WaypointPath>();

        if (createPathVisuals)
        {
            CreatePathSegments(waypointPath);
        }

        if (createCastleVisual)
        {
            CreateCastle(waypointPath);
        }

        if (createEnvironmentProps)
        {
            CreateEnvironmentProps(waypointPath);
        }

        if (setupCamera)
        {
            SetupCamera();
        }
    }

    void CreateMaterials()
    {
        groundMaterial = CreateMaterial("Runtime Ground Material", groundColor);
        pathMaterial = CreateMaterial("Runtime Path Material", pathColor);
        castleMaterial = CreateMaterial("Runtime Castle Material", castleColor);
        waypointMaterial = CreateMaterial("Runtime Waypoint Material", waypointColor);
        treeTrunkMaterial = CreateMaterial("Runtime Tree Trunk Material", treeTrunkColor);
        treeLeafMaterial = CreateMaterial("Runtime Tree Leaf Material", treeLeafColor);
        rockMaterial = CreateMaterial("Runtime Rock Material", rockColor);
        roofMaterial = CreateMaterial("Runtime Roof Material", roofColor);
    }

    Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = materialName;
        material.color = color;
        return material;
    }

    void CreateVisualRoot()
    {
        GameObject oldRoot = GameObject.Find("Runtime Visuals");
        if (oldRoot != null)
        {
            Destroy(oldRoot);
        }

        GameObject rootObject = new GameObject("Runtime Visuals");
        visualRoot = rootObject.transform;
    }

    void ColorGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return;

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = groundMaterial;
            renderer.material.SetFloat("_Glossiness", 0.12f);
        }

        CreateGroundDetail(ground.transform.position, 140);
    }

    void CreateGroundDetail(Vector3 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject tuft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tuft.name = "GrassTuft";
            tuft.transform.SetParent(visualRoot);
            tuft.transform.position = new Vector3(
                center.x + Random.Range(-28f, 28f),
                0.03f,
                center.z + Random.Range(-28f, 28f)
            );
            float s = Random.Range(0.08f, 0.18f);
            tuft.transform.localScale = new Vector3(s, Random.Range(0.05f, 0.12f), s);
            Renderer r = tuft.GetComponent<Renderer>();
            if (r != null)
            {
                float tint = Random.Range(-0.06f, 0.06f);
                r.material.color = new Color(groundColor.r + tint, groundColor.g + tint, groundColor.b + tint);
            }
            Collider c = tuft.GetComponent<Collider>();
            if (c != null) Destroy(c);
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
            if (renderer != null)
            {
                renderer.material = waypointMaterial;
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

            Vector3 startPosition = start.position;
            Vector3 endPosition = end.position;
            startPosition.y = 0.03f;
            endPosition.y = 0.03f;

            Vector3 middle = (startPosition + endPosition) * 0.5f;
            Vector3 direction = endPosition - startPosition;
            float length = direction.magnitude;

            if (length <= 0.01f) continue;

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "Path Segment " + (i + 1);
            segment.transform.SetParent(visualRoot);
            segment.transform.position = middle;
            segment.transform.rotation = Quaternion.LookRotation(direction.normalized);
            segment.transform.localScale = new Vector3(pathWidth, pathHeight, length);

            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = pathMaterial;
            }

            Collider collider = segment.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            if (createPathBorders)
            {
                Vector3 side = Vector3.Cross(Vector3.up, direction.normalized).normalized;
                CreatePathBorder(middle + side * (pathWidth * 0.6f), direction.normalized, length, i, "Left");
                CreatePathBorder(middle - side * (pathWidth * 0.6f), direction.normalized, length, i, "Right");
            }
        }
    }

    void CreatePathBorder(Vector3 position, Vector3 direction, float length, int index, string sideName)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = "Path Border " + sideName + " " + (index + 1);
        border.transform.SetParent(visualRoot);
        border.transform.position = position + Vector3.up * 0.04f;
        border.transform.rotation = Quaternion.LookRotation(direction);
        border.transform.localScale = new Vector3(0.16f, 0.12f, length);

        Renderer renderer = border.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = rockMaterial;
        }

        Collider collider = border.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
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
            if (existingRenderer != null)
            {
                existingRenderer.enabled = false;
            }
        }

        GameObject castle = new GameObject("Castle Visual");
        castle.name = "Castle";
        castle.transform.SetParent(visualRoot);
        castle.transform.position = lastWaypoint.position;

        GameObject keep = CreatePrimitiveChild("Castle Keep", PrimitiveType.Cube, castle.transform);
        keep.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        keep.transform.localScale = new Vector3(castleSize * 0.8f, castleSize * 0.9f, castleSize * 0.8f);
        SetMaterial(keep, castleMaterial);

        GameObject roof = CreatePrimitiveChild("Castle Roof", PrimitiveType.Cylinder, castle.transform);
        roof.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        roof.transform.localScale = new Vector3(castleSize * 0.95f, castleSize * 0.4f, castleSize * 0.95f);
        SetMaterial(roof, roofMaterial);

        CreateCastleTower(castle.transform, new Vector3(-1.55f, 0f, -1.55f));
        CreateCastleTower(castle.transform, new Vector3(1.55f, 0f, -1.55f));
        CreateCastleTower(castle.transform, new Vector3(-1.55f, 0f, 1.55f));
        CreateCastleTower(castle.transform, new Vector3(1.55f, 0f, 1.55f));

        GameObject gate = CreatePrimitiveChild("Castle Gate", PrimitiveType.Cube, castle.transform);
        gate.transform.localPosition = new Vector3(0f, 0.45f, -1.28f);
        gate.transform.localScale = new Vector3(0.7f, 0.9f, 0.08f);
        SetColor(gate, new Color(0.18f, 0.1f, 0.04f));
    }

    void CreateCastleTower(Transform parent, Vector3 localPosition)
    {
        GameObject tower = CreatePrimitiveChild("Castle Tower", PrimitiveType.Cylinder, parent);
        tower.transform.localPosition = localPosition + Vector3.up * 0.6f;
        tower.transform.localScale = new Vector3(0.35f, 0.8f, 0.35f);
        SetMaterial(tower, castleMaterial);

        GameObject roof = CreatePrimitiveChild("Tower Roof", PrimitiveType.Cylinder, parent);
        roof.transform.localPosition = localPosition + Vector3.up * 1.25f;
        roof.transform.localScale = new Vector3(0.45f, 0.3f, 0.45f);
        SetMaterial(roof, roofMaterial);
    }

    void CreateEnvironmentProps(WaypointPath waypointPath)
    {
        if (waypointPath == null || waypointPath.Count < 2) return;

        for (int i = 0; i < waypointPath.Count - 1; i++)
        {
            Transform start = waypointPath.GetWaypoint(i);
            Transform end = waypointPath.GetWaypoint(i + 1);

            if (start == null || end == null) continue;

            Vector3 direction = (end.position - start.position).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;

            for (int treeIndex = 0; treeIndex < treesPerPathSegment; treeIndex++)
            {
                float t = (treeIndex + 1f) / (treesPerPathSegment + 1f);
                Vector3 basePosition = Vector3.Lerp(start.position, end.position, t);
                float sideSign = treeIndex % 2 == 0 ? 1f : -1f;
                CreateTree(basePosition + side * sideSign * Random.Range(4f, 6f));
            }

            for (int rockIndex = 0; rockIndex < rocksPerPathSegment; rockIndex++)
            {
                Vector3 basePosition = Vector3.Lerp(start.position, end.position, 0.5f);
                float sideSign = rockIndex % 2 == 0 ? -1f : 1f;
                CreateRock(basePosition + side * sideSign * Random.Range(2.6f, 3.6f));
            }
        }
    }

    void CreateTree(Vector3 position)
    {
        position.y = 0f;

        GameObject tree = new GameObject("Pine Tree");
        tree.transform.SetParent(visualRoot);
        tree.transform.position = position;

        GameObject trunk = CreatePrimitiveChild("Trunk", PrimitiveType.Cylinder, tree.transform);
        trunk.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        trunk.transform.localScale = new Vector3(0.22f, 0.7f, 0.22f);
        SetMaterial(trunk, treeTrunkMaterial);

        GameObject leavesBottom = CreatePrimitiveChild("Leaves Bottom", PrimitiveType.Cylinder, tree.transform);
        leavesBottom.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        leavesBottom.transform.localScale = new Vector3(0.85f, 0.55f, 0.85f);
        SetMaterial(leavesBottom, treeLeafMaterial);

        GameObject leavesTop = CreatePrimitiveChild("Leaves Top", PrimitiveType.Cylinder, tree.transform);
        leavesTop.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        leavesTop.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        SetMaterial(leavesTop, treeLeafMaterial);
    }

    void CreateRock(Vector3 position)
    {
        position.y = 0.18f;

        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        rock.transform.SetParent(visualRoot);
        rock.transform.position = position;
        rock.transform.localScale = new Vector3(Random.Range(0.5f, 0.9f), Random.Range(0.25f, 0.45f), Random.Range(0.5f, 0.9f));
        SetMaterial(rock, rockMaterial);

        Collider collider = rock.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    GameObject CreatePrimitiveChild(string objectName, PrimitiveType primitiveType, Transform parent)
    {
        GameObject child = GameObject.CreatePrimitive(primitiveType);
        child.name = objectName;
        child.transform.SetParent(parent);
        child.transform.localRotation = Quaternion.identity;

        Collider collider = child.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return child;
    }

    void SetMaterial(GameObject target, Material material)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    void SetColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        mainCamera.transform.position = new Vector3(0f, 16f, -12f);
        mainCamera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 9f;
    }
}
