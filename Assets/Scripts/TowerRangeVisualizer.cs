using UnityEngine;

public class TowerRangeVisualizer : MonoBehaviour
{
    private static GameObject rangeObj;

    public static void Show(Vector3 position, float radius)
    {
        if (rangeObj == null)
        {
            rangeObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rangeObj.name = "TowerRange";

            // 🔥 REMOVE QUALQUER COLLIDER (garantia total)
            Collider col = rangeObj.GetComponent<Collider>();
            if (col != null)
                Object.Destroy(col);

            // 🔥 COLOCA NA LAYER QUE IGNORA RAYCAST
            rangeObj.layer = LayerMask.NameToLayer("Ignore Raycast");

            // 🔥 MATERIAL TRANSPARENTE
            Renderer r = rangeObj.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Standard"));

            r.material.color = new Color(0f, 1f, 0f, 0.15f);
            r.material.SetFloat("_Mode", 3);
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.DisableKeyword("_ALPHATEST_ON");
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            r.material.renderQueue = 3000;
        }

        rangeObj.SetActive(true);

        // 🔥 POSIÇÃO NO CHÃO
        rangeObj.transform.position = new Vector3(position.x, 0.05f, position.z);

        // 🔥 ESCALA (Cylinder usa diâmetro)
        rangeObj.transform.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
    }

    public static void Hide()
    {
        if (rangeObj != null)
            rangeObj.SetActive(false);
    }
}