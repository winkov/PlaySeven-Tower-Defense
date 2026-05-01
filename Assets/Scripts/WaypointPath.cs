using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();
    public Color pathColor = Color.cyan;
    public float gizmoSphereSize = 0.35f;
    public int randomSeed = 0;

    public int Count { get { return waypoints.Count; } }

    void Awake()
    {
        RefreshWaypoints();
    }


    void RefreshWaypoints()
    {
        waypoints.Clear();

        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }
    }

    public Transform GetWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Count) return null;

        return waypoints[index];
    }

    public void GenerateRandomPath(int stage)
    {
        int seed = randomSeed != 0 ? randomSeed + stage : System.DateTime.Now.Millisecond + (stage * 1000);
        Random.InitState(seed);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        int waypointCount = Mathf.Clamp(5 + stage, 5, 9);
        float length = 22f + stage * 2f;
        float zStep = length / (waypointCount - 1);
        float maxX = Mathf.Min(4f + stage * 0.8f, 9f);

        for (int i = 0; i < waypointCount; i++)
        {
            GameObject wp = new GameObject("WP_" + i);
            wp.transform.SetParent(transform);

            float z = i * zStep;
            float x = i == 0 || i == waypointCount - 1 ? 0f : Random.Range(-maxX, maxX);
            wp.transform.position = new Vector3(x, 0f, z);
        }

        RefreshWaypoints();
    }

    void OnDrawGizmos()
    {
        RefreshWaypoints();

        Gizmos.color = pathColor;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawSphere(waypoints[i].position, gizmoSphereSize);

            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
