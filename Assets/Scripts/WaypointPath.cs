using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();
    public Color pathColor = Color.cyan;
    public float gizmoSphereSize = 0.35f;

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
