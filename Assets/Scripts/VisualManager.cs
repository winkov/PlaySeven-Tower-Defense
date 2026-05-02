using UnityEngine;

public class VisualManager : MonoBehaviour
{
    public bool setupCamera = true;
    public bool colorExistingObjects = true;
    public bool createPathVisuals = true;
    public bool createCastleVisual = true;
    public bool createEnvironmentProps = true;
    public bool hideWaypointMeshes = true;

    public float pathWidth = 3.2f;
    public float pathHeight = 0.08f;
    public float cornerCut = 0.6f; // quanto recorta antes do canto

    private Material groundMaterial;
    private Material pathMaterial;
    private Transform visualRoot;

    public GameObject treePrefabOverride;
    public GameObject castlePrefab;

    void Start()
    {
        RefreshWorldVisuals();
    }

    public void RefreshWorldVisuals()
    {
        CreateMaterials();
        CreateVisualRoot();

        WaypointPath path = FindAnyObjectByType<WaypointPath>();

        if (colorExistingObjects)
        {
            ColorGround();
            ColorWaypointSpheres();
        }

        if (createPathVisuals) CreatePath(path);
        if (createCastleVisual) CreateCastle(path);
        if (createEnvironmentProps) CreateEnvironmentProps(path);
        if (setupCamera) SetupCamera(path);
    }

    void CreateMaterials()
    {
        groundMaterial = CreateMaterial("Ground", new Color(0.42f, 0.67f, 0.24f));
        pathMaterial = CreateMaterial("Path", new Color(0.88f, 0.70f, 0.46f));
    }

    Material CreateMaterial(string name, Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        mat.SetFloat("_Glossiness", 0.04f);
        return mat;
    }

    void CreateVisualRoot()
    {
        GameObject old = GameObject.Find("Runtime Visuals");
        if (old != null) Destroy(old);
        visualRoot = new GameObject("Runtime Visuals").transform;
    }

    void ColorGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return;

        Renderer r = ground.GetComponent<Renderer>();
        if (r != null) r.material = groundMaterial;
    }

    void ColorWaypointSpheres()
    {
        WaypointPath path = FindAnyObjectByType<WaypointPath>();
        if (path == null) return;

        for (int i = 0; i < path.Count; i++)
        {
            Transform wp = path.GetWaypoint(i);
            if (wp == null) continue;

            Renderer[] rends = wp.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
                r.enabled = false;
        }
    }

    // =========================
    // 🔥 CAMINHO COM CANTOS CORRETOS
    // =========================
    void CreatePath(WaypointPath path)
    {
        if (path == null || path.Count < 2) return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 a = path.GetWaypoint(i).position;
            Vector3 b = path.GetWaypoint(i + 1).position;

            Vector3 dir = (b - a).normalized;

            // verifica se existe curva
            bool hasCorner = (i < path.Count - 2);

            Vector3 start = a;
            Vector3 end = b;

            if (hasCorner)
            {
                Vector3 next = path.GetWaypoint(i + 2).position;
                Vector3 nextDir = (next - b).normalized;

                // se muda direção → recorta antes do canto
                if (Vector3.Dot(dir, nextDir) < 0.99f)
                {
                    end -= dir * cornerCut;
                }
            }

            if (i > 0)
            {
                Vector3 prev = path.GetWaypoint(i - 1).position;
                Vector3 prevDir = (a - prev).normalized;

                if (Vector3.Dot(prevDir, dir) < 0.99f)
                {
                    start += dir * cornerCut;
                }
            }

            CreateSegment(start, end);

            // cria o “tampão” do canto
            if (hasCorner)
            {
                Vector3 next = path.GetWaypoint(i + 2).position;
                Vector3 nextDir = (next - b).normalized;

                if (Vector3.Dot(dir, nextDir) < 0.99f)
                {
                    CreateCorner(b);
                }
            }
        }
    }

    void CreateSegment(Vector3 a, Vector3 b)
    {
        a.y = 0.2f;
        b.y = 0.2f;

        Vector3 delta = b - a;

        // 🔥 FORÇA ORTOGONAL
        bool horizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.z);

        Vector3 dir;
        float length;

        if (horizontal)
        {
            dir = new Vector3(Mathf.Sign(delta.x), 0, 0);
            length = Mathf.Abs(delta.x);
        }
        else
        {
            dir = new Vector3(0, 0, Mathf.Sign(delta.z));
            length = Mathf.Abs(delta.z);
        }

        Vector3 mid = (a + b) * 0.5f;

        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.transform.SetParent(visualRoot);
        segment.transform.position = mid;
        segment.transform.rotation = Quaternion.LookRotation(dir);

        segment.transform.localScale = new Vector3(pathWidth, pathHeight, length);

        Renderer r = segment.GetComponent<Renderer>();
        if (r != null) r.material = pathMaterial;

        Destroy(segment.GetComponent<Collider>());
    }

    // 🔥 peça de canto (remove o triângulo)
    void CreateCorner(Vector3 pos)
    {
        GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        corner.transform.SetParent(visualRoot);
        corner.transform.position = new Vector3(pos.x, 0.19f, pos.z);
        corner.transform.localScale = new Vector3(pathWidth * 0.5f, pathHeight * 0.5f, pathWidth * 0.5f);

        Renderer r = corner.GetComponent<Renderer>();
        if (r != null) r.material = pathMaterial;

        Destroy(corner.GetComponent<Collider>());
    }

    void CreateCastle(WaypointPath path)
    {
        if (path == null || path.Count < 2) return;

        Transform last = path.GetWaypoint(path.Count - 1);
        Transform prev = path.GetWaypoint(path.Count - 2);

        Vector3 dir = (last.position - prev.position).normalized;
        Vector3 pos = last.position + dir * 3f;
        pos.y = 0.08f;

        if (castlePrefab == null) return;

        GameObject castle = Instantiate(castlePrefab, pos, Quaternion.LookRotation(-dir), visualRoot);
        castle.transform.localScale = Vector3.one * 0.5f;
    }

    void CreateEnvironmentProps(WaypointPath path)
    {
        if (path == null || path.Count < 2) return;

        for (int i = 0; i < path.Count - 1; i += 6)
        {
            Transform a = path.GetWaypoint(i);
            Transform b = path.GetWaypoint(i + 1);
            if (a == null || b == null) continue;

            Vector3 dir = (b.position - a.position).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, dir);

            Vector3 mid = (a.position + b.position) / 2f;

            float dist = 14f;

            CreateTree(mid + side * dist);
            CreateTree(mid - side * dist);
        }
    }

    void CreateTree(Vector3 pos)
    {
        if (treePrefabOverride == null) return;

        pos.y = 0;

        GameObject tree = Instantiate(
            treePrefabOverride,
            pos,
            Quaternion.Euler(0, Random.Range(0, 360), 0),
            visualRoot
        );

        tree.transform.localScale = Vector3.one * Random.Range(1.8f, 2.3f);
    }

    void SetupCamera(WaypointPath path)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 center = Vector3.zero;

        if (path != null && path.Count > 0)
        {
            for (int i = 0; i < path.Count; i++)
                center += path.GetWaypoint(i).position;

            center /= path.Count;
        }

        cam.transform.position = new Vector3(center.x, 13f, center.z - 9f);
        cam.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        cam.orthographic = true;
        cam.orthographicSize = 10f;
    }
}