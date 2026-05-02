using UnityEngine;
using System.Collections;

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
        if (r != null)
        {
            r.material = new Material(Shader.Find("Standard"));
            r.material.color = visualColor;

            // 🔥 deixa emissivo (mais bonito)
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", visualColor * 1.5f);
        }
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

        Vector3 dir = (targetPosition - transform.position);

        if (dir.sqrMagnitude < 0.0001f)
        {
            HitTarget(targetPosition);
            return;
        }

        dir.Normalize();

        // 🔥 arco leve (catapult feel)
        if (splashRadius > 0.5f)
        {
            float arc = Mathf.Sin(Time.time * 10f) * 0.15f;
            dir.y += arc;
        }

        transform.position += dir * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, targetPosition) <= hitDistance)
        {
            HitTarget(targetPosition);
        }
    }

    void HitTarget(Vector3 hitPos)
    {
        if (splashRadius > 0.01f)
        {
            Enemy[] enemies = FindObjectsByType<Enemy>();
            for (int i = 0; i < enemies.Length; i++)
            {
                if (Vector3.Distance(hitPos, enemies[i].transform.position) <= splashRadius)
                {
                    enemies[i].TakeDamage(damage);
                }
            }
        }
        else
        {
            if (target != null)
                target.TakeDamage(damage);
        }

        CreateImpactFx();
        Destroy(gameObject);
    }

    // 🔥 IMPACTO VISUAL MELHORADO
    void CreateImpactFx()
    {
        GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fx.transform.position = transform.position + Vector3.up * 0.2f;
        fx.transform.localScale = Vector3.one * 0.2f;

        Renderer r = fx.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = visualColor;
        }

        Destroy(fx.GetComponent<Collider>());

        // 🔥 SUMIR RÁPIDO (ANTES TAVA POLUINDO)
        Destroy(fx, 0.08f);
    }

    // 🔥 animação do impacto (expansão)
    IEnumerator ImpactAnim(GameObject fx)
    {
        float t = 0f;
        Vector3 start = fx.transform.localScale;

        while (t < 0.2f)
        {
            t += Time.deltaTime;
            fx.transform.localScale = start * (1f + t * 2f);
            yield return null;
        }

        Destroy(fx);
    }
}