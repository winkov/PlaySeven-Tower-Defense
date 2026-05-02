using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();
    public Color pathColor = Color.cyan;
    public float gizmoSphereSize = 0.35f;
    public int randomSeed = 0;
    public bool generateLayoutOnAwake = true;

    public int Count { get { return waypoints.Count; } }

    void Awake()
    {
        if (generateLayoutOnAwake) GenerateRandomPath(1);
        else RefreshWaypoints();
    }

    void RefreshWaypoints()
    {
        waypoints.Clear();
        foreach (Transform child in transform) waypoints.Add(child);
    }

    public Transform GetWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Count) return null;
        return waypoints[index];
    }

    public void GenerateRandomPath(int stage)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        float left = -20f;
        float right = 20f;

        float z1 = 8f;
        float z2 = 2f;
        float z3 = -4f;

        CreateWaypoint("WP_0", new Vector3(left, 0.2f, z1));
        CreateWaypoint("WP_1", new Vector3(right, 0.2f, z1));

        CreateWaypoint("WP_2", new Vector3(right, 0.2f, z2));

        CreateWaypoint("WP_3", new Vector3(left, 0.2f, z2));

        CreateWaypoint("WP_4", new Vector3(left, 0.2f, z3));

        CreateWaypoint("WP_5", new Vector3(right, 0.2f, z3));

        RefreshWaypoints();
    }
    int CreateLine(Vector3 start, Vector3 end, int index, float spacing)
    {
        float dist = Vector3.Distance(start, end);
        int steps = Mathf.CeilToInt(dist / spacing);

        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)steps;
            Vector3 pos = Vector3.Lerp(start, end, t);
            CreateWaypoint("WP_" + index, pos);
            index++;
        }

        return index;
    }
    void CreateWaypoint(string waypointName, Vector3 position)
    {
        GameObject waypoint = new GameObject(waypointName);
        waypoint.transform.SetParent(transform);
        waypoint.transform.position = position;
    }

    void OnDrawGizmos()
    {
        RefreshWaypoints();
        Gizmos.color = pathColor;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, gizmoSphereSize);
            if (i < waypoints.Count - 1 && waypoints[i + 1] != null) Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
