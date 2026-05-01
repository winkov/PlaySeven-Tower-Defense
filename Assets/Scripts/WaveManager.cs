using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int wavesPerStage = 10;
    public int baseEnemies = 5;
    public float spawnDelay = 1.1f;
    public float minSpawnDelay = 0.5f;
    public float enemyHealthMultiplierPerWave = 1.075f;
    public float enemyHealthMultiplierPerStage = 1.15f;
    public float enemySpeedBonusPerWave = 0.09f;
    public float enemySpeedBonusPerStage = 0.18f;
    public bool debugLogs = true;

    private int currentWaveInStage;
    private int currentStage = 1;
    private int aliveEnemies;
    private bool waveRunning;
    private BonusSystem bonusSystem;
    private UIManager uiManager;
    private WaypointPath waypointPath;
    private BuildSpotGenerator buildSpotGenerator;
    private bool waitingBonusChoice;
    private int enemiesSpawnedThisWave;
    private int enemiesToSpawnThisWave;

    public int CurrentWaveDisplay { get { return currentWaveInStage + 1; } }
    public bool WaveRunning { get { return waveRunning; } }

    void Start()
    {
        bonusSystem = FindAnyObjectByType<BonusSystem>();
        uiManager = FindAnyObjectByType<UIManager>();
        waypointPath = FindAnyObjectByType<WaypointPath>();
        buildSpotGenerator = FindAnyObjectByType<BuildSpotGenerator>();
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

    void Update()
    {
        if (!waveRunning) return;
        if (enemiesSpawnedThisWave < enemiesToSpawnThisWave) return;

        Enemy[] alive = FindObjectsByType<Enemy>();
        if (alive.Length == 0 && aliveEnemies > 0)
        {
            aliveEnemies = 0;
            OnEnemyDied();
        }
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

        if (waitingBonusChoice)
        {
            return;
        }

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        waveRunning = true;
        int enemiesThisWave = GetEnemiesForCurrentWave();
        enemiesToSpawnThisWave = enemiesThisWave;
        enemiesSpawnedThisWave = 0;
        aliveEnemies = enemiesThisWave;
        RefreshUI();

        if (debugLogs)
        {
            Debug.Log("Starting Stage " + currentStage + " Wave " + CurrentWaveDisplay + " with " + aliveEnemies + " enemies.", this);
        }

        for (int i = 0; i < enemiesThisWave; i++)
        {
            SpawnEnemy();
            enemiesSpawnedThisWave++;
            yield return new WaitForSeconds(GetSpawnDelayForCurrentWave());
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
            EnemyTypeEnum type = GetEnemyTypeForWave(currentWaveInStage, currentStage);
            enemy.SetEnemyType(type);

            float healthMultiplier = Mathf.Pow(enemyHealthMultiplierPerWave, currentWaveInStage) * Mathf.Pow(enemyHealthMultiplierPerStage, currentStage - 1);
            float speedBonus = (enemySpeedBonusPerWave * currentWaveInStage) + (enemySpeedBonusPerStage * (currentStage - 1));
            enemy.ApplyWaveModifiers(healthMultiplier, speedBonus);

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
            uiManager.UpdateStage(currentStage);
            uiManager.UpdateWaveMessage(waveRunning, aliveEnemies);
        }
    }

    public void OnEnemyDied()
    {
        aliveEnemies--;

        if (aliveEnemies <= 0 && waveRunning)
        {
            waveRunning = false;
            currentWaveInStage++;

            if (currentWaveInStage >= wavesPerStage)
            {
                AdvanceToNextStage();
            }
            else
            {
                RefreshUI();
            }

            if (debugLogs)
            {
                Debug.Log("Wave completed. Stage " + currentStage + " Wave " + currentWaveInStage, this);
            }
        }
    }

    void AdvanceToNextStage()
    {
        currentStage++;
        currentWaveInStage = 0;

        waitingBonusChoice = true;
        if (bonusSystem != null && uiManager != null)
        {
            string[] labels = bonusSystem.GetBonusLabels();
            uiManager.ShowBonusChoices(labels, OnBonusChoiceSelected);
        }
        else
        {
            OnBonusChoiceSelected(0);
        }

        GenerateStageMap();
        RefreshUI();

        if (debugLogs)
        {
            Debug.Log("Advanced to Stage " + currentStage + ". New map generated.", this);
        }
    }

    void OnBonusChoiceSelected(int index)
    {
        if (bonusSystem != null)
        {
            bonusSystem.ApplyBonusChoice(index);
        }
        waitingBonusChoice = false;
    }

    void GenerateStageMap()
    {
        if (waypointPath != null)
        {
            waypointPath.GenerateRandomPath(currentStage);
        }

        if (buildSpotGenerator != null)
        {
            buildSpotGenerator.GenerateBuildSpots();
        }

        if (spawnPoint == null)
        {
            return;
        }

        if (waypointPath != null && waypointPath.Count > 0)
        {
            Transform firstWaypoint = waypointPath.GetWaypoint(0);
            if (firstWaypoint != null)
            {
                spawnPoint.position = firstWaypoint.position;
            }
        }
    }

    int GetEnemiesForCurrentWave()
    {
        int stageBonus = (currentStage - 1) * 3;
        int waveBonus = currentWaveInStage <= 2 ? currentWaveInStage : 2 + ((currentWaveInStage - 2) * 2);
        return Mathf.Max(1, baseEnemies + stageBonus + waveBonus);
    }

    float GetSpawnDelayForCurrentWave()
    {
        float stageReduction = (currentStage - 1) * 0.05f;
        float waveReduction = currentWaveInStage * 0.025f;
        return Mathf.Max(minSpawnDelay, spawnDelay - stageReduction - waveReduction);
    }

    EnemyTypeEnum GetEnemyTypeForWave(int waveInStage, int stage)
    {
        int random = Random.Range(0, 100);

        if (stage <= 1 && waveInStage <= 1)
        {
            if (random < 65) return EnemyTypeEnum.Mage;
            if (random < 93) return EnemyTypeEnum.Archer;
            return EnemyTypeEnum.Warrior;
        }

        if (stage <= 1 && waveInStage <= 3)
        {
            if (random < 48) return EnemyTypeEnum.Mage;
            if (random < 84) return EnemyTypeEnum.Archer;
            return EnemyTypeEnum.Warrior;
        }

        if (stage == 2)
        {
            if (random < 30) return EnemyTypeEnum.Mage;
            if (random < 72) return EnemyTypeEnum.Archer;
            return EnemyTypeEnum.Warrior;
        }

        if (random < 10) return EnemyTypeEnum.Mage;
        if (random < 55) return EnemyTypeEnum.Archer;
        return EnemyTypeEnum.Warrior;
    }
}
