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
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);

        // Caminho em zigzag com elevação: esquerda→direita, desce, direita→esquerda, desce, esquerda→direita
        Vector3[] layout =
        {
            new Vector3(-26f, 0.2f, -8f),      // Esquerda - subida
            new Vector3(24f, 0.5f, -8f),       // Direita - topo
            new Vector3(24f, 0.3f, -2f),       // Desce um pouco
            new Vector3(-26f, 0.5f, -2f),      // Volta pra esquerda - topo
            new Vector3(-26f, 0.3f, 4f),       // Desce mais
            new Vector3(24f, 0.5f, 4f),        // Vai pra direita de novo - topo
            new Vector3(24f, 0.2f, 10f)        // Final na direita
        };

        for (int i = 0; i < layout.Length; i++) CreateWaypoint("WP_" + i, layout[i]);

        RefreshWaypoints();
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
