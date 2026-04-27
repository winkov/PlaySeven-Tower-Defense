using UnityEngine;

public enum EnemyTypeEnum
{
    Mage,
    Archer,
    Warrior
}

public static class EnemyTypeHelper
{
    public static Color GetColor(EnemyTypeEnum type)
    {
        switch (type)
        {
            case EnemyTypeEnum.Mage:
                return new Color(0.7f, 0.3f, 1f); // Purple
            case EnemyTypeEnum.Archer:
                return new Color(1f, 0.8f, 0.2f); // Yellow
            case EnemyTypeEnum.Warrior:
                return new Color(1f, 0.3f, 0.3f); // Red
            default:
                return Color.white;
        }
    }

    public static float GetSpeed(EnemyTypeEnum type)
    {
        switch (type)
        {
            case EnemyTypeEnum.Mage:
                return 2.2f; // Slow
            case EnemyTypeEnum.Archer:
                return 3.0f; // Medium
            case EnemyTypeEnum.Warrior:
                return 4.0f; // Fast
            default:
                return 2.8f;
        }
    }

    public static int GetMaxHealth(EnemyTypeEnum type)
    {
        switch (type)
        {
            case EnemyTypeEnum.Mage:
                return 40; // Weak
            case EnemyTypeEnum.Archer:
                return 70; // Medium
            case EnemyTypeEnum.Warrior:
                return 120; // Strong
            default:
                return 100;
        }
    }

    public static int GetGoldValue(EnemyTypeEnum type)
    {
        switch (type)
        {
            case EnemyTypeEnum.Mage:
                return 15; // Low gold
            case EnemyTypeEnum.Archer:
                return 25; // Medium gold
            case EnemyTypeEnum.Warrior:
                return 40; // High gold
            default:
                return 10;
        }
    }

    public static float GetScale(EnemyTypeEnum type)
    {
        switch (type)
        {
            case EnemyTypeEnum.Mage:
                return 0.7f; // Small
            case EnemyTypeEnum.Archer:
                return 1f; // Normal
            case EnemyTypeEnum.Warrior:
                return 1.3f; // Large
            default:
                return 1f;
        }
    }
}
