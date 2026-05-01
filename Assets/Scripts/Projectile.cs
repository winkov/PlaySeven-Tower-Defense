using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float hitDistance = 0.25f;
    public float visibleScale = 0.35f;
    public float splashRadius = 0f;
    public Color visualColor = Color.white;

    private Enemy target;
    private int damage;

    void Start()
    {
        transform.localScale = Vector3.one * visibleScale;
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = visualColor;
    }

    public void SetTarget(Enemy enemy, int projectileDamage)
    {
        target = enemy;
        damage = projectileDamage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position + Vector3.up * 0.5f;
        Vector3 prev = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        Vector3 dir = transform.position - prev;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir.normalized);

        if (Vector3.Distance(transform.position, targetPosition) <= hitDistance)
        {
            if (splashRadius > 0.01f)
            {
                Enemy[] enemies = FindObjectsByType<Enemy>();
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (Vector3.Distance(transform.position, enemies[i].transform.position) <= splashRadius)
                    {
                        enemies[i].TakeDamage(damage);
                    }
                }
            }
            else
            {
                target.TakeDamage(damage);
            }
            CreateImpactFx();
            Destroy(gameObject);
        }
    }

    void CreateImpactFx()
    {
        GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fx.transform.position = transform.position;
        fx.transform.localScale = Vector3.one * (splashRadius > 0f ? splashRadius * 0.45f : 0.25f);
        Collider c = fx.GetComponent<Collider>();
        if (c != null) Destroy(c);
        Renderer r = fx.GetComponent<Renderer>();
        if (r != null) r.material.color = visualColor;
        Destroy(fx, 0.18f);
    }
}
