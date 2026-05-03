using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Play(); // 🔥 FORÇA tocar
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }
}