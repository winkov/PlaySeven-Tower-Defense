using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float hitDistance = 0.25f;
    public float visibleScale = 0.35f;
    public float splashRadius = 0f;
    public Color visualColor = Color.white;

    public GameObject impactPrefab;
    public AudioClip shootSound;
    public AudioClip impactSound;

    private Enemy target;
    private int damage;
    private float arcOffset;

    // 🔥 controle de spam de som
    private static float lastImpactSoundTime;

    void Start()
    {
        transform.localScale = Vector3.one * visibleScale;

        arcOffset = Random.Range(0f, 10f);

        Renderer r = GetComponent<Renderer>();
        if (r != null)
            r.material.color = visualColor;

        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr != null)
            tr.Clear();

        // 🔊 SOM DE TIRO (AGORA COM AUDIOMANAGER)
        if (shootSound != null && AudioManager.Instance != null)
        {
            float volume = Random.Range(0.5f, 0.7f);
            AudioManager.Instance.PlaySFX(shootSound, volume);
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
        Vector3 dir = targetPosition - transform.position;

        if (dir.sqrMagnitude < 0.0001f)
        {
            HitTarget(targetPosition);
            return;
        }

        dir.Normalize();

        // 🔥 arco leve (catapult feel)
        if (splashRadius > 0.5f)
        {
            float arc = Mathf.Sin((Time.time + arcOffset) * 10f) * 0.15f;
            dir.y += arc;
        }

        transform.position += dir * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);

        // 🔥 giro leve (visual melhor)
        transform.Rotate(0f, 360f * Time.deltaTime, 0f);

        if ((transform.position - targetPosition).sqrMagnitude <= hitDistance * hitDistance)
        {
            HitTarget(targetPosition);
        }
    }

    void HitTarget(Vector3 hitPos)
    {
        // 💥 dano
        if (splashRadius > 0.01f)
        {
            var enemies = Enemy.ActiveEnemies;

            for (int i = 0; i < enemies.Count; i++)
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

        // 💥 FX
        CreateImpactFx();

        // 🔊 SOM DE IMPACTO (COM CONTROLE + AUDIOMANAGER)
        if (impactSound != null && AudioManager.Instance != null)
        {
            if (Time.time - lastImpactSoundTime > 0.05f)
            {
                float volume = Random.Range(0.6f, 0.8f);
                AudioManager.Instance.PlaySFX(impactSound, volume);
                lastImpactSoundTime = Time.time;
            }
        }

        Destroy(gameObject);
    }

    void CreateImpactFx()
    {
        if (impactPrefab != null)
        {
            GameObject fx = Instantiate(
                impactPrefab,
                transform.position + Vector3.up * 0.2f,
                Quaternion.identity
            );

            float scale = splashRadius > 0 ? 1.5f : 1f;
            fx.transform.localScale *= scale;

            Destroy(fx, 1.2f);
        }
    }
}