using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int startingGold = 100;
    public int castleMaxHealth = 10;

    private int gold;
    private int castleHealth;
    private bool gameOver;
    private UIManager uiManager;
    private WaveManager waveManager;

    public int Gold { get { return gold; } }
    public int CastleHealth { get { return castleHealth; } }
    public bool IsGameOver { get { return gameOver; } }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gold = startingGold;
        castleHealth = castleMaxHealth;
        uiManager = FindAnyObjectByType<UIManager>();
        waveManager = FindAnyObjectByType<WaveManager>();
        UpdateUI();
    }

    public void AddGold(int amount)
    {
        if (gameOver) return;

        gold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (gameOver || gold < amount) return false;

        gold -= amount;
        UpdateUI();
        return true;
    }

    public int GetGold()
    {
        return gold;
    }

    public void DamageCastle(int damage)
    {
        if (gameOver) return;

        castleHealth -= damage;
        castleHealth = Mathf.Max(0, castleHealth);

        if (castleHealth <= 0)
        {
            gameOver = true;
            Debug.Log("Game Over");
        }

        UpdateUI();
    }

    public int GetCastleHealth()
    {
        return castleHealth;
    }

    void UpdateUI()
    {
        if (uiManager == null) return;

        int waveNumber = waveManager != null ? waveManager.CurrentWaveDisplay : 1;
        uiManager.UpdateGold(gold);
        uiManager.UpdateCastleHealth(castleHealth, castleMaxHealth);
        uiManager.UpdateWave(waveNumber);
        uiManager.UpdateGameOver(gameOver);
    }
}
