using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float hitDistance = 0.25f;
    public float visibleScale = 0.35f;

    private Enemy target;
    private int damage;

    void Start()
    {
        transform.localScale = Vector3.one * visibleScale;
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

        MoveToTarget();
    }

    void MoveToTarget()
    {
        Vector3 targetPosition = target.transform.position + Vector3.up * 0.5f;
        Vector3 previousPosition = transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        Vector3 movementDirection = transform.position - previousPosition;
        if (movementDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(movementDirection.normalized);
        }

        if (Vector3.Distance(transform.position, targetPosition) <= hitDistance)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
