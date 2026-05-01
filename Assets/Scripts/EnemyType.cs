using UnityEngine;

public enum EnemyTypeEnum
{
    Mage,
    Archer,
    Warrior,
    Flying
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
            case EnemyTypeEnum.Flying:
                return new Color(0.55f, 0.9f, 1f); // Sky
            default:
                return Color.white;
        }
    }

    public static float GetSpeed(EnemyTypeEnum type)
    {
        switch (type)
        {
            case EnemyTypeEnum.Mage:
                return 3.6f;
            case EnemyTypeEnum.Archer:
                return 4.4f;
            case EnemyTypeEnum.Warrior:
                return 4.9f;
            case EnemyTypeEnum.Flying:
                return 5.4f;
            default:
                return 4.5f;
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
            case EnemyTypeEnum.Flying:
                return 65; // Mid
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
            case EnemyTypeEnum.Flying:
                return 30;
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
            case EnemyTypeEnum.Flying:
                return 0.9f;
            default:
                return 1f;
        }
    }
}
