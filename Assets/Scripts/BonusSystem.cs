using UnityEngine;

public class BonusSystem : MonoBehaviour
{
    public int goldBonus = 20;
    public float damageMultiplier = 1.1f;
    public float fireRateMultiplier = 1.08f;
    public int castleHeal = 2;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }


    public string[] GetBonusLabels()
    {
        return new string[]
        {
            "+Gold " + goldBonus,
            "+Damage 10%",
            "+Gold " + castleHeal + " + Fire Rate 8%"
        };
    }

    public void ApplyBonusChoice(int index)
    {
        if (gameManager != null)
        {
            if (index == 0)
            {
                gameManager.AddGold(goldBonus);
            }
            else if (index == 2)
            {
                gameManager.AddGold(castleHeal);
            }
        }

        Tower[] towers = FindObjectsByType<Tower>();
        foreach (Tower tower in towers)
        {
            if (index == 1)
            {
                tower.damage = Mathf.RoundToInt(tower.damage * damageMultiplier);
            }
            else if (index == 2)
            {
                tower.fireRate *= fireRateMultiplier;
            }
        }
    }
}
