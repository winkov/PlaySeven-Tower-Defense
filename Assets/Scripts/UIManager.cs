using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text goldText;
    public Text castleHealthText;
    public Text waveText;
    public Text messageText;
    public Button startWaveButton;
    public Button buildTowerButton;
    public bool autoLayout = true;

    private WaveManager waveManager;
    private BuildManager buildManager;

    void Start()
    {
        waveManager = FindAnyObjectByType<WaveManager>();
        buildManager = FindAnyObjectByType<BuildManager>();
        FindMissingReferences();

        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartWave);
        }
        else
        {
            Debug.LogWarning("UIManager could not find StartWaveButton. Drag it into the UIManager or name the button StartWaveButton.", this);
        }

        if (buildTowerButton != null)
        {
            buildTowerButton.onClick.AddListener(ToggleBuildMode);
        }

        if (autoLayout)
        {
            ApplySimpleLayout();
        }
    }

    public void UpdateGold(int gold)
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + gold;
        }
    }

    public void UpdateCastleHealth(int health, int maxHealth)
    {
        if (castleHealthText != null)
        {
            castleHealthText.text = "Castle HP: " + health + "/" + maxHealth;
        }
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + wave;
        }
    }

    public void UpdateBuildMode(bool active)
    {
        if (buildTowerButton != null)
        {
            buildTowerButton.gameObject.SetActive(!active);

            Text buttonText = buildTowerButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "Build Tower";
            }
        }

        ShowMessage(active ? "Tap a green build spot" : "");
    }

    public void UpdateStartWaveButton(bool waveRunning, bool allWavesFinished)
    {
        if (startWaveButton == null) return;

        startWaveButton.interactable = !waveRunning && !allWavesFinished;
        Text buttonText = startWaveButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = allWavesFinished ? "Victory" : waveRunning ? "Wave Running" : "Start Wave";
        }
    }

    public void UpdateGameOver(bool gameOver)
    {
        if (gameOver)
        {
            ShowMessage("Game Over");
        }
    }

    public void UpdateWaveMessage(bool waveRunning, int aliveEnemies)
    {
        if (waveRunning)
        {
            ShowMessage("Enemies: " + aliveEnemies);
        }
        else
        {
            ShowMessage("");
        }
    }

    void StartWave()
    {
        if (waveManager != null)
        {
            Debug.Log("Start Wave button clicked.", this);
            waveManager.StartNextWave();
        }
        else
        {
            Debug.LogWarning("No WaveManager found in the scene.", this);
        }
    }

    void ToggleBuildMode()
    {
        if (buildManager != null)
        {
            buildManager.ToggleBuildMode();
        }
    }

    void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    void FindMissingReferences()
    {
        if (goldText == null)
        {
            goldText = FindTextByName("GoldText");
        }

        if (castleHealthText == null)
        {
            castleHealthText = FindTextByName("CastleHPText");
        }

        if (waveText == null)
        {
            waveText = FindTextByName("WaveText");
        }

        if (messageText == null)
        {
            messageText = FindTextByName("MessageText");
        }

        if (startWaveButton == null)
        {
            startWaveButton = FindButtonByName("StartWaveButton");
        }

        if (buildTowerButton == null)
        {
            buildTowerButton = FindButtonByName("BuildTowerButton");
        }
    }

    Text FindTextByName(string objectName)
    {
        GameObject foundObject = GameObject.Find(objectName);
        if (foundObject == null) return null;

        return foundObject.GetComponent<Text>();
    }

    Button FindButtonByName(string objectName)
    {
        GameObject foundObject = GameObject.Find(objectName);
        if (foundObject == null) return null;

        return foundObject.GetComponent<Button>();
    }

    void ApplySimpleLayout()
    {
        PlaceText(goldText, new Vector2(16f, -16f));
        PlaceText(castleHealthText, new Vector2(16f, -48f));
        PlaceText(waveText, new Vector2(16f, -80f));
        PlaceText(messageText, new Vector2(0f, -16f), TextAnchor.UpperCenter);

        PlaceButton(startWaveButton, new Vector2(-20f, -20f));
        PlaceButton(buildTowerButton, new Vector2(-20f, -78f));
    }

    void PlaceText(Text text, Vector2 anchoredPosition)
    {
        PlaceText(text, anchoredPosition, TextAnchor.UpperLeft);
    }

    void PlaceText(Text text, Vector2 anchoredPosition, TextAnchor alignment)
    {
        if (text == null) return;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = alignment == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = alignment == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = alignment == TextAnchor.UpperCenter ? new Vector2(260f, 30f) : new Vector2(220f, 28f);

        text.alignment = alignment;
        text.fontSize = 18;
        text.color = Color.black;
    }

    void PlaceButton(Button button, Vector2 anchoredPosition)
    {
        if (button == null) return;

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(150f, 42f);

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.fontSize = 16;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;
        }
    }
}
