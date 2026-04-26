using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int[] enemiesPerWave = { 5, 8, 12, 16, 20 };
    public float spawnDelay = 0.75f;
    public float enemyHealthMultiplierPerWave = 1.25f;
    public float enemySpeedBonusPerWave = 0.15f;
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
        if (debugLogs)
        {
            Debug.Log("WaveManager received StartNextWave.", this);
        }

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
            enemyObject = CreateRuntimeEnemy(spawnPosition);
        }

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        ScaleEnemyForWave(enemy);
    }

    GameObject CreateRuntimeEnemy(Vector3 spawnPosition)
    {
        GameObject enemyObject = new GameObject("Runtime Enemy");
        enemyObject.name = "Runtime Enemy";
        enemyObject.transform.position = spawnPosition;

        GameObject body = CreateEnemyPart("Body", PrimitiveType.Capsule, enemyObject.transform);
        body.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        body.transform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
        SetColor(body, new Color(0.55f, 0.12f, 0.1f));

        GameObject head = CreateEnemyPart("Head", PrimitiveType.Sphere, enemyObject.transform);
        head.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        SetColor(head, new Color(0.75f, 0.18f, 0.12f));

        GameObject marker = CreateEnemyPart("Helmet", PrimitiveType.Cube, enemyObject.transform);
        marker.transform.localPosition = new Vector3(0f, 1.63f, 0f);
        marker.transform.localScale = new Vector3(0.55f, 0.12f, 0.55f);
        SetColor(marker, new Color(0.18f, 0.18f, 0.2f));

        enemyObject.AddComponent<Enemy>();
        return enemyObject;
    }

    GameObject CreateEnemyPart(string partName, PrimitiveType primitiveType, Transform parent)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = partName;
        part.transform.SetParent(parent);
        part.transform.localRotation = Quaternion.identity;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return part;
    }

    void SetColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    void ScaleEnemyForWave(Enemy enemy)
    {
        if (enemy == null) return;

        float healthMultiplier = Mathf.Pow(enemyHealthMultiplierPerWave, currentWave);
        enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * healthMultiplier);
        enemy.speed += enemySpeedBonusPerWave * currentWave;
        enemy.goldValue += currentWave * 2;
    }

    public void NotifyEnemyRemoved()
    {
        if (!waveRunning) return;

        aliveEnemies--;

        if (aliveEnemies <= 0)
        {
            FinishWave();
        }
        else
        {
            RefreshUI();
        }
    }

    void FinishWave()
    {
        waveRunning = false;
        currentWave++;

        if (debugLogs)
        {
            Debug.Log("Wave finished.", this);
        }

        if (bonusSystem != null)
        {
            bonusSystem.ApplyBonus();
        }

        RefreshUI();
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
}
