using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int[] enemiesPerWave = { 5, 8, 12, 16, 20 };
    public float spawnDelay = 0.75f;
    public float enemyHealthMultiplierPerWave = 1.20f;
    public float enemySpeedBonusPerWave = 0.35f;
    public bool debugLogs = true;

    private int currentWave;
    private int aliveEnemies;
    private bool waveRunning;
    private BonusSystem bonusSystem;
    private UIManager uiManager;

    public int CurrentWaveDisplay { get { return currentWave + 1; } }
    public bool WaveRunning { get { return waveRunning; } }

    void Start()
    {
        bonusSystem = FindAnyObjectByType<BonusSystem>();
        uiManager = FindAnyObjectByType<UIManager>();
        Debug.Log("WaveManager Start - UIManager found: " + (uiManager != null), this);

        if (spawnPoint == null)
        {
            GameObject spawnObject = GameObject.Find("SpawnPoint");
            if (spawnObject != null)
            {
                spawnPoint = spawnObject.transform;
            }
        }

        RefreshUI();
    }

    public void StartNextWave()
    {
        Debug.Log("WaveManager StartNextWave called", this);

        if (waveRunning)
        {
            Debug.Log("Wave is already running.", this);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            Debug.Log("Cannot start wave because game is over.", this);
            return;
        }

        if (currentWave >= enemiesPerWave.Length)
        {
            Debug.Log("All waves are finished.", this);
            return;
        }

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        waveRunning = true;
        aliveEnemies = enemiesPerWave[currentWave];
        RefreshUI();

        if (debugLogs)
        {
            Debug.Log("Starting wave " + CurrentWaveDisplay + " with " + aliveEnemies + " enemies.", this);
        }

        for (int i = 0; i < enemiesPerWave[currentWave]; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position;

        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }

        GameObject enemyObject;

        if (enemyPrefab != null)
        {
            enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            enemyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyObject.name = "Enemy";
            enemyObject.transform.position = spawnPosition;
            enemyObject.transform.localScale = Vector3.one * 0.5f;

            Collider collider = enemyObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = enemyObject.AddComponent<Enemy>();
        }

        if (enemy != null)
        {
            EnemyTypeEnum type = GetEnemyTypeForWave(currentWave);
            enemy.SetEnemyType(type);

            if (currentWave > 0)
            {
                float healthMultiplier = Mathf.Pow(enemyHealthMultiplierPerWave, currentWave);
                float speedBonus = enemySpeedBonusPerWave * currentWave;
                enemy.ApplyWaveModifiers(healthMultiplier, speedBonus);
            }

            if (debugLogs)
            {
                Debug.Log("Spawned " + type + " enemy at " + spawnPosition, this);
            }
        }
        else if (debugLogs)
        {
            Debug.LogWarning("Failed to add Enemy script to object at " + spawnPosition, this);
        }
    }

    void RefreshUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateWave(CurrentWaveDisplay);
            uiManager.UpdateStartWaveButton(waveRunning, currentWave >= enemiesPerWave.Length);
            uiManager.UpdateWaveMessage(waveRunning, aliveEnemies);
        }
    }

    public void OnEnemyDied()
    {
        aliveEnemies--;

        if (aliveEnemies <= 0 && waveRunning)
        {
            waveRunning = false;
            currentWave++;
            RefreshUI();

            if (debugLogs)
            {
                Debug.Log("Wave " + (currentWave) + " completed!", this);
            }
        }
    }

    EnemyTypeEnum GetEnemyTypeForWave(int wave)
    {
        int random = Random.Range(0, 100);

        if (wave <= 0)
        {
            if (random < 55) return EnemyTypeEnum.Mage;
            if (random < 90) return EnemyTypeEnum.Archer;
            return EnemyTypeEnum.Warrior;
        }

        if (wave == 1)
        {
            if (random < 30) return EnemyTypeEnum.Mage;
            if (random < 75) return EnemyTypeEnum.Archer;
            return EnemyTypeEnum.Warrior;
        }

        if (wave == 2)
        {
            if (random < 20) return EnemyTypeEnum.Mage;
            if (random < 60) return EnemyTypeEnum.Archer;
            return EnemyTypeEnum.Warrior;
        }

        if (random < 10) return EnemyTypeEnum.Mage;
        if (random < 55) return EnemyTypeEnum.Archer;
        return EnemyTypeEnum.Warrior;
    }
}
