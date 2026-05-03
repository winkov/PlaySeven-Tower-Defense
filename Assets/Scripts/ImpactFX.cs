using UnityEngine;

public class ImpactFX : MonoBehaviour
{
    public float duration = 0.2f;
    public float maxScale = 1.5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;

        transform.localScale = Vector3.one * (1f + t * maxScale);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}