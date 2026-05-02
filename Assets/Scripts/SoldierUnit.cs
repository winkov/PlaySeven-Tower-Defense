using UnityEngine;

public class SoldierUnit : MonoBehaviour
{
    public float speed = 4.2f;
    public float attackRange = 1.1f;
    public float attackRate = 1.1f;
    public int damage = 14;
    public float lifetime = 10f;

    private float attackTimer;
    private float lifeTimer;

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Enemy target = FindNearestEnemy();
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.transform.position,
                speed * Time.deltaTime
            );
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            target.TakeDamage(damage);
            attackTimer = 1f / Mathf.Max(0.2f, attackRate);
        }
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>();

        Enemy nearest = null;
        float nearestDist = 999f;

        for (int i = 0; i < enemies.Length; i++)
        {
            float d = Vector3.Distance(transform.position, enemies[i].transform.position);

            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = enemies[i];
            }
        }

        return nearest;
    }
}