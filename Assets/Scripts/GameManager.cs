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

    private int mageDamageTotal;
    private int archerDamageTotal;
    private int catapultDamageTotal;
    private int barracksDamageTotal;

    public int Gold { get { return gold; } }
    public int CastleHealth { get { return castleHealth; } }
    public bool IsGameOver { get { return gameOver; } }

    void Awake() { Instance = this; }

    void Start()
    {


        gold = startingGold;
        castleHealth = castleMaxHealth;
        uiManager = FindAnyObjectByType<UIManager>();
        waveManager = FindAnyObjectByType<WaveManager>();
        UpdateUI();
    }

    public void RegisterTowerDamage(TowerType type, int damage)
    {
        if (damage <= 0) return;
        if (type == TowerType.Mage) mageDamageTotal += damage;
        else if (type == TowerType.Archer) archerDamageTotal += damage;
        else if (type == TowerType.Catapult) catapultDamageTotal += damage;
        else barracksDamageTotal += damage;

        if (uiManager != null) uiManager.UpdateDamageRanking(mageDamageTotal, archerDamageTotal, catapultDamageTotal, barracksDamageTotal);
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

    public int GetGold() { return gold; }

    public void DamageCastle(int damage)
    {
        if (gameOver) return;
        castleHealth -= damage;
        castleHealth = Mathf.Max(0, castleHealth);

        if (castleHealth <= 0)
        {
            gameOver = true;
            int reachedWave = waveManager != null ? waveManager.CurrentWaveDisplay : 1;
            int best = PlayerPrefs.GetInt("BestWaveRecord", 1);
            if (reachedWave > best)
            {
                PlayerPrefs.SetInt("BestWaveRecord", reachedWave);
                PlayerPrefs.Save();
            }
        }

        UpdateUI();
    }

    public int GetCastleHealth() { return castleHealth; }

    void UpdateUI()
    {
        if (uiManager == null) return;
        int waveNumber = waveManager != null ? waveManager.CurrentWaveDisplay : 1;
        uiManager.UpdateGold(gold);
        uiManager.UpdateCastleHealth(castleHealth, castleMaxHealth);
        uiManager.UpdateWave(waveNumber);
        uiManager.UpdateGameOver(gameOver);
        uiManager.UpdateDamageRanking(mageDamageTotal, archerDamageTotal, catapultDamageTotal, barracksDamageTotal);
    }
}
