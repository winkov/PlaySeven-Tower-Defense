using UnityEngine;

public class SoldierUnit : MonoBehaviour
{
    public int damage = 10;
    public float attackRate = 1f;
    public float attackRange = 1.5f;
    public float lifetime = 8f;

    private float attackTimer;
    private Enemy currentTarget;

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            ReleaseTarget();
            Destroy(gameObject);
            return;
        }

        attackTimer -= Time.deltaTime;

        if (currentTarget == null)
        {
            FindTarget();
            return;
        }

        if (currentTarget == null)
        {
            ReleaseTarget();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (dist > attackRange)
        {
            ReleaseTarget();
            return;
        }

        // 🔥 trava inimigo
        currentTarget.SetBlocked(transform);

        // 🔥 olha pro inimigo
        Vector3 dir = currentTarget.transform.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);

        if (attackTimer <= 0f)
        {
            currentTarget.TakeDamage(damage);
            attackTimer = 1f / attackRate;
        }
    }

    void FindTarget()
    {
        float bestDist = attackRange;
        Enemy best = null;

        foreach (var e in Enemy.ActiveEnemies)
        {
            if (e == null) continue;

            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d <= bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        if (best != null)
        {
            currentTarget = best;
            currentTarget.SetBlocked(transform);
        }
    }

    void ReleaseTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.ReleaseBlock(transform);
            currentTarget = null;
        }
    }

    void OnDestroy()
    {
        ReleaseTarget();
    }
}