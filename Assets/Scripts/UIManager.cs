using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public Text goldText;
    public Text castleHealthText;
    public Text waveText;
    public Text messageText;
    public Text autoWaveText;
    public Button startWaveButton;
    public Text towerInfoText;
    public Button upgradeDamageButton;
    public Button upgradeRangeButton;
    public Button upgradeFireRateButton;

    private WaveManager waveManager;
    private Tower selectedTower;
    private Canvas canvas;
    private Camera mainCamera;
    private RectTransform rootRect;
    private int stageDisplay = 1;
    private int waveDisplay = 1;

    private GameObject hudRoot;
    private GameObject statsPanel;
    private GameObject rankingPanel;
    private GameObject buildChoicePanel;
    private GameObject bonusChoicePanel;
    private GameObject towerInfoPanel;
    private RectTransform towerInfoRect;

    private BuildSpot pendingSpot;
    private float buildChoiceReadyTime;
    private float bonusChoiceReadyTime;
    private Action<int> bonusChoiceCallback;

    private Text nextEnemyText;
    private readonly Image[] rankFills = new Image[4];
    private readonly Text[] rankLabels = new Text[4];

    private readonly string[] towerDisplayNames = { "MAGE", "ARCHER", "CATAPULT", "BARRACKS" };
    private readonly TowerType[] towerTypes = { TowerType.Mage, TowerType.Archer, TowerType.Catapult, TowerType.Barracks };

    void Start()
    {
        waveManager = FindAnyObjectByType<WaveManager>();
        canvas = FindAnyObjectByType<Canvas>();
        mainCamera = Camera.main;

        if (canvas == null) return;

        rootRect = canvas.GetComponent<RectTransform>();

        ConfigureCanvasScaler();
        FindMissingReferences();
        EnsureCoreLabels();
        HideLegacySceneWidgets();
        BuildHudLayout();
        HookButtons();
        RefreshAll();
    }

    void ConfigureCanvasScaler()
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
    }

    void FindMissingReferences()
    {
        if (goldText == null) goldText = FindTextByName("GoldText");
        if (castleHealthText == null) castleHealthText = FindTextByName("CastleHPText");
        if (waveText == null) waveText = FindTextByName("WaveText");
        if (messageText == null) messageText = FindTextByName("MessageText");
        if (autoWaveText == null) autoWaveText = FindTextByName("AutoWaveText");
        if (startWaveButton == null) startWaveButton = FindButtonByName("StartWaveButton");
    }

    void EnsureCoreLabels()
    {
        if (goldText == null) goldText = CreateStandaloneText("GoldText");
        if (castleHealthText == null) castleHealthText = CreateStandaloneText("CastleHPText");
        if (waveText == null) waveText = CreateStandaloneText("WaveText");
        if (messageText == null) messageText = CreateStandaloneText("MessageText");
        if (autoWaveText == null) autoWaveText = CreateStandaloneText("AutoWaveText");
    }

    void HideLegacySceneWidgets()
    {
        Button buildButton = FindButtonByName("BuildTowerButton");
        if (buildButton != null) buildButton.gameObject.SetActive(false);
    }

    void BuildHudLayout()
    {
        CleanupGeneratedHud();

        hudRoot = new GameObject("HUDRoot");
        RectTransform hudRect = hudRoot.AddComponent<RectTransform>();
        hudRect.SetParent(canvas.transform, false);
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        statsPanel = CreatePanel("StatsPanel", hudRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(420f, 230f), new Color(0.08f, 0.06f, 0.04f, 0.88f));
        rankingPanel = CreatePanel("RankingPanel", hudRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 18f), new Vector2(400f, 200f), new Color(0.1f, 0.08f, 0.06f, 0.85f));
        towerInfoPanel = CreatePanel("TowerInfoPanel", hudRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-28f, -40f), new Vector2(540f, 420f), new Color(0.09f, 0.07f, 0.05f, 0.96f));
        towerInfoRect = towerInfoPanel.GetComponent<RectTransform>();
        CreateBuildChoicePanel();
        CreateBonusChoicePanel();
        CreateRankingRows();
        CreateNextEnemyLabel();
        LayoutCoreTexts();
        CreateTowerInfoContent();
        StyleStartWaveButton();
        rankingPanel.SetActive(true);
    }

    void CleanupGeneratedHud()
    {
        string[] names =
        {
            "HUDRoot",
            "HUDLeft",
            "HUDTop",
            "StatsPanel",
            "RankingPanel",
            "TowerInfoPanel",
            "BuildChoicePanel",
            "BonusChoicePanel",
            "NextEnemyText",
            "DamageRankingTitle",
            "DamageRankingRoot"
        };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject existing = GameObject.Find(names[i]);
            if (existing != null) Destroy(existing);
        }
    }
    bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
    void HookButtons()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.RemoveAllListeners();
            startWaveButton.onClick.AddListener(() =>
            {
                if (waveManager != null) waveManager.StartNextWave();
            });
        }

        if (upgradeDamageButton != null)
        {
            upgradeDamageButton.onClick.RemoveAllListeners();
            upgradeDamageButton.onClick.AddListener(UpgradeDamage);
        }

        if (upgradeRangeButton != null)
        {
            upgradeRangeButton.onClick.RemoveAllListeners();
            upgradeRangeButton.onClick.AddListener(UpgradeRange);
        }

        if (upgradeFireRateButton != null)
        {
            upgradeFireRateButton.onClick.RemoveAllListeners();
            upgradeFireRateButton.onClick.AddListener(UpgradeFireRate);
        }
    }

    void RefreshAll()
    {
        if (GameManager.Instance != null)
        {
            UpdateGold(GameManager.Instance.Gold);
            UpdateCastleHealth(GameManager.Instance.CastleHealth, GameManager.Instance.castleMaxHealth);
            UpdateDamageRanking(0, 0, 0, 0);
        }

        if (waveManager != null)
        {
            UpdateStage(1);
            UpdateWave(waveManager.CurrentWaveDisplay);
            UpdateStartWaveButton(waveManager.WaveRunning, GameManager.Instance != null && GameManager.Instance.IsGameOver);
            UpdateNextEnemyPreview(waveManager.NextWavePreviewType, waveManager.NextWavePreviewCount);
        }

        UpdateAutoWaveCountdown(0f, true);
        UpdateWaveMessage(false, 0);
        DeselectTower();
    }

    GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    Text CreateStandaloneText(string name)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        Text text = obj.AddComponent<Text>();
        ApplyTextStyle(text, 28, new Color(0.98f, 0.94f, 0.84f), TextAnchor.MiddleLeft);
        return text;
    }

    Text CreateTextChild(string name, Transform parent, int fontSize, Color color, TextAnchor anchor)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        Text text = obj.AddComponent<Text>();
        ApplyTextStyle(text, fontSize, color, anchor);
        return text;
    }

    void ApplyTextStyle(Text text, int fontSize, Color color, TextAnchor anchor)
    {
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontStyle = FontStyle.Bold;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void LayoutCoreTexts()
    {
        LayoutStatText(goldText, statsPanel.transform, new Vector2(18f, -16f), new Vector2(360f, 52f), 48);
        LayoutStatText(castleHealthText, statsPanel.transform, new Vector2(18f, -68f), new Vector2(360f, 48f), 44);
        LayoutStatText(waveText, statsPanel.transform, new Vector2(18f, -116f), new Vector2(360f, 48f), 44);
        LayoutStatText(autoWaveText, statsPanel.transform, new Vector2(18f, -164f), new Vector2(360f, 44f), 40);

        RectTransform messageRect = messageText.GetComponent<RectTransform>();
        messageText.transform.SetParent(hudRoot.transform, false);
        messageRect.anchorMin = new Vector2(0.5f, 1f);
        messageRect.anchorMax = new Vector2(0.5f, 1f);
        messageRect.pivot = new Vector2(0.5f, 1f);
        messageRect.anchoredPosition = new Vector2(0f, -14f);
        messageRect.sizeDelta = new Vector2(700f, 60f);
        ApplyTextStyle(messageText, 40, new Color(1f, 0.97f, 0.89f), TextAnchor.MiddleCenter);
        messageText.text = "Select a build slot to place a tower";
    }

    void LayoutStatText(Text text, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize)
    {
        RectTransform rect = text.GetComponent<RectTransform>();
        text.transform.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        ApplyTextStyle(text, fontSize, new Color(0.98f, 0.94f, 0.84f), TextAnchor.MiddleLeft);
    }

    void StyleStartWaveButton()
    {
        if (startWaveButton == null) return;

        RectTransform rect = startWaveButton.GetComponent<RectTransform>();
        rect.SetParent(hudRoot.transform, false);
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-28f, -28f);
        rect.sizeDelta = new Vector2(240f, 72f);

        Image image = startWaveButton.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = new Color(1f, 0.4f, 0.2f, 0.99f);
        }

        ColorBlock colors = startWaveButton.colors;
        colors.normalColor = new Color(1f, 0.4f, 0.2f, 0.99f);
        colors.highlightedColor = new Color(1f, 0.6f, 0.4f, 1f);
        colors.pressedColor = new Color(0.9f, 0.3f, 0.1f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        startWaveButton.colors = colors;

        Text label = startWaveButton.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            ApplyTextStyle(label, 28, new Color(1f, 1f, 0.95f), TextAnchor.MiddleCenter);
            label.text = "▶ START WAVE";
        }
    }

    void CreateNextEnemyLabel()
    {
        nextEnemyText = CreateTextChild("NextEnemyText", statsPanel.transform, 28, new Color(1f, 0.95f, 0.56f), TextAnchor.MiddleLeft);
        RectTransform rect = nextEnemyText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(18f, -212f);
        rect.sizeDelta = new Vector2(360f, 32f);
    }

    void CreateRankingRows()
    {
        Text title = CreateTextChild("DamageRankingTitle", rankingPanel.transform, 28, new Color(0.98f, 0.94f, 0.84f), TextAnchor.MiddleLeft);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(12f, -16f);
        titleRect.sizeDelta = new Vector2(200f, 32f);
        title.text = "Damage Ranking";

        string[] names = { "Mage", "Archer", "Catapult", "Barracks" };
        Color[] colors =
        {
            new Color(0.32f, 0.78f, 1f),
            new Color(0.5f, 0.92f, 0.44f),
            new Color(1f, 0.66f, 0.28f),
            new Color(1f, 0.42f, 0.42f)
        };

        for (int i = 0; i < rankLabels.Length; i++)
        {
            GameObject row = new GameObject("RankingRow" + i);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.SetParent(rankingPanel.transform, false);
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0f, 1f);
            rowRect.anchoredPosition = new Vector2(12f, -60f - (i * 26f));
            rowRect.sizeDelta = new Vector2(-24f, 22f);

            Text label = CreateTextChild("Label", row.transform, 20, Color.white, TextAnchor.MiddleLeft);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(120f, 0f);
            label.text = names[i] + " 0%";
            rankLabels[i] = label;

            GameObject barBase = new GameObject("BarBase");
            RectTransform barRect = barBase.AddComponent<RectTransform>();
            barRect.SetParent(row.transform, false);
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.offsetMin = new Vector2(126f, 2f);
            barRect.offsetMax = new Vector2(-4f, -2f);

            Image baseImage = barBase.AddComponent<Image>();
            baseImage.color = new Color(0f, 0f, 0f, 0.28f);

            GameObject fill = new GameObject("Fill");
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.SetParent(barRect, false);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = colors[i];
            rankFills[i] = fillImage;
        }
    }

    void CreateTowerInfoContent()
    {
        towerInfoText = CreateTextChild("TowerInfoText", towerInfoPanel.transform, 36, new Color(1f, 0.95f, 0.80f), TextAnchor.UpperLeft);
        RectTransform infoRect = towerInfoText.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0f, 1f);
        infoRect.anchorMax = new Vector2(1f, 1f);
        infoRect.pivot = new Vector2(0.5f, 1f);
        infoRect.anchoredPosition = new Vector2(0f, -24f);
        infoRect.offsetMin = new Vector2(24f, 0f);
        infoRect.offsetMax = new Vector2(-24f, -240f);

        upgradeDamageButton = CreateUpgradeButton("UpgradeDamageButton", towerInfoPanel.transform, new Vector2(24f, 24f), "⚔ UPG DMG");
        upgradeRangeButton = CreateUpgradeButton("UpgradeRangeButton", towerInfoPanel.transform, new Vector2(24f, 100f), "◉ UPG RNG");
        upgradeFireRateButton = CreateUpgradeButton("UpgradeFireRateButton", towerInfoPanel.transform, new Vector2(24f, 176f), "⚡ UPG SPD");

        towerInfoPanel.SetActive(false);
    }

    Button CreateUpgradeButton(string name, Transform parent, Vector2 bottomLeftOffset, string label)
    {
        GameObject buttonObj = new GameObject(name);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(24f, bottomLeftOffset.y);
        rect.offsetMax = new Vector2(-24f, bottomLeftOffset.y + 66f);

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.7f, 0.99f);

        Button button = buttonObj.AddComponent<Button>();

        Text text = CreateTextChild("Text", buttonObj.transform, 32, new Color(1f, 0.98f, 0.90f), TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 8f);
        textRect.offsetMax = new Vector2(-14f, -8f);
        text.text = label;
        return button;
    }

    void CreateBuildChoicePanel()
    {
        buildChoicePanel = CreatePanel("BuildChoicePanel", hudRoot.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(1000f, 180f), new Color(0.08f, 0.06f, 0.04f, 0.96f));

        Text title = CreateTextChild("Title", buildChoicePanel.transform, 48, new Color(1f, 1f, 0.8f), TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(700f, 48f);
        title.text = "⚔ Choose a Tower";

        for (int i = 0; i < towerTypes.Length; i++)
        {
            float x = -310f + (i * 210f);
            CreateBuildChoiceButton(buildChoicePanel.transform, i, new Vector2(x, -30f));
        }

        buildChoicePanel.SetActive(false);
    }

    void CreateBuildChoiceButton(Transform parent, int index, Vector2 anchoredPosition)
    {
        GameObject buttonObj = new GameObject("BuildChoiceButton" + index);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(220f, 100f);

        Image image = buttonObj.AddComponent<Image>();
        // Cores diferentes por tipo de torre
        Color[] buttonColors = new Color[]
        {
            new Color(0.4f, 0.6f, 1f, 1f),    // Mage - azul
            new Color(0.6f, 1f, 0.4f, 1f),    // Archer - verde
            new Color(1f, 0.7f, 0.2f, 1f),    // Catapult - laranja
            new Color(1f, 0.5f, 0.5f, 1f)     // Barracks - vermelho
        };
        image.color = buttonColors[index % 4];

        Button button = buttonObj.AddComponent<Button>();
        TowerType towerType = towerTypes[index];
        button.onClick.AddListener(() => ConfirmBuildTower(towerType));

        Text text = CreateTextChild("Text", buttonObj.transform, 38, new Color(0.15f, 0.1f, 0.05f, 1f), TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 14f);
        textRect.offsetMax = new Vector2(-14f, -14f);
        text.text = towerDisplayNames[index] + "\n50g";
    }

    void CreateBonusChoicePanel()
    {
        bonusChoicePanel = CreatePanel("BonusChoicePanel", hudRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 300f), new Color(0.1f, 0.08f, 0.06f, 0.99f));

        Text title = CreateTextChild("Title", bonusChoicePanel.transform, 44, new Color(1f, 1f, 0.8f), TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -24f);
        titleRect.sizeDelta = new Vector2(800f, 48f);
        title.text = "\u2b50 Choose Stage Bonus";

        for (int i = 0; i < 3; i++)
        {
            float x = -310f + (i * 310f);
            CreateBonusButton(i, new Vector2(x, -40f));
        }

        bonusChoicePanel.SetActive(false);
    }

    void CreateBonusButton(int index, Vector2 anchoredPosition)
    {
        GameObject buttonObj = new GameObject("BonusChoiceButton" + index);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.SetParent(bonusChoicePanel.transform, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(260f, 140f);

        Image image = buttonObj.AddComponent<Image>();
        Color[] bonusColors = new Color[]
        {
            new Color(1f, 0.6f, 0.2f, 1f),    // Laranja
            new Color(0.8f, 0.2f, 1f, 1f),    // Roxo
            new Color(0.2f, 0.8f, 1f, 1f)     // Ciano
        };
        image.color = bonusColors[index % 3];

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => ConfirmBonusChoice(index));

        Text text = CreateTextChild("Text", buttonObj.transform, 36, new Color(0.15f, 0.1f, 0.05f, 1f), TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 16f);
        textRect.offsetMax = new Vector2(-16f, -16f);
        text.text = "BONUS\n" + (index + 1);
    }

    public void UpdateGold(int value)
    {
        if (goldText != null) goldText.text = "Gold " + value;
        if (selectedTower != null) UpdateUpgradeButtonLabels();
        UpdateBuildChoiceLabels();
    }

    public void UpdateCastleHealth(int health, int maxHealth)
    {
        if (castleHealthText != null) castleHealthText.text = "Castle " + health + " / " + maxHealth;
    }

    public void UpdateWave(int wave)
    {
        waveDisplay = wave;
        if (waveText != null) waveText.text = "Stage " + stageDisplay + "   Wave " + wave;
    }

    public void UpdateStage(int stage)
    {
        stageDisplay = stage;
        if (waveText != null) waveText.text = "Stage " + stage + "   Wave " + waveDisplay;
    }

    public void UpdateBuildMode(bool active) { }

    public void UpdateWaveMessage(bool running, int alive)
    {
        if (messageText == null) return;
        if (running) messageText.text = "Enemies alive: " + alive;
        else if (bonusChoicePanel != null && bonusChoicePanel.activeSelf) messageText.text = "Choose a bonus to continue";
        else if (buildChoicePanel != null && buildChoicePanel.activeSelf) messageText.text = "Choose a tower for this slot";
        else messageText.text = "Select a build slot to place a tower";
    }

    public void UpdateAutoWaveCountdown(float seconds, bool firstPending)
    {
        if (autoWaveText == null) return;
        autoWaveText.text = firstPending ? "Ready to start" : "Next wave in " + Mathf.CeilToInt(seconds) + "s";
    }

    public void UpdateStartWaveButton(bool waveRunning, bool gameFinished)
    {
        if (startWaveButton == null) return;
        startWaveButton.interactable = !waveRunning && !gameFinished && (bonusChoicePanel == null || !bonusChoicePanel.activeSelf);
    }

    public void UpdateGameOver(bool over)
    {
        if (!over) return;
        if (messageText != null) messageText.text = "Castle destroyed";
        if (startWaveButton != null) startWaveButton.interactable = false;
        if (buildChoicePanel != null) buildChoicePanel.SetActive(false);
        if (bonusChoicePanel != null) bonusChoicePanel.SetActive(false);
    }

    public void UpdateNextEnemyPreview(EnemyTypeEnum type, int count)
    {
        if (nextEnemyText == null) return;
        string label = type == EnemyTypeEnum.Warrior ? "Warrior Rush" :
                       type == EnemyTypeEnum.Archer ? "Archer Pack" :
                       type == EnemyTypeEnum.Flying ? "Flying Wave" :
                       "Caster Squad";
        nextEnemyText.text = "Next: " + label + " x" + count;
    }

    public void UpdateDamageRanking(int mage, int archer, int catapult, int barracks)
    {
        int[] values = { mage, archer, catapult, barracks };
        string[] names = { "Mage", "Archer", "Catapult", "Barracks" };
        int totalRaw = mage + archer + catapult + barracks;
        if (rankingPanel != null) rankingPanel.SetActive(totalRaw > 0);
        int total = Mathf.Max(1, totalRaw);

        for (int i = 0; i < values.Length; i++)
        {
            float ratio = (float)values[i] / total;
            if (rankLabels[i] != null) rankLabels[i].text = names[i] + " " + Mathf.RoundToInt(ratio * 100f) + "%";
            if (rankFills[i] != null)
            {
                RectTransform fillRect = rankFills[i].rectTransform;
                fillRect.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
            }
        }
    }

    public void ShowBuildChoice(BuildSpot spot)
    {
        // 🔥 NÃO deixa trocar spot enquanto menu aberto
        if (buildChoicePanel != null && buildChoicePanel.activeSelf)
            return;

        if (spot == null || buildChoicePanel == null) return;

        if (pendingSpot != null && pendingSpot != spot)
            pendingSpot.SetBuildPreviewSelected(false);

        pendingSpot = spot;
        pendingSpot.SetBuildPreviewSelected(true);

        buildChoiceReadyTime = Time.time + 0.15f;
        buildChoicePanel.SetActive(true);

        if (bonusChoicePanel != null)
            bonusChoicePanel.SetActive(false);

        UpdateBuildChoiceLabels();
        DeselectTower();
        UpdateWaveMessage(false, 0);
    }

    public void ShowBonusChoices(string[] labels, Action<int> onSelected)
    {
        if (bonusChoicePanel == null)
        {
            if (onSelected != null) onSelected(0);
            return;
        }

        if (pendingSpot != null) pendingSpot.SetBuildPreviewSelected(false);
        pendingSpot = null;
        if (buildChoicePanel != null) buildChoicePanel.SetActive(false);

        bonusChoiceCallback = onSelected;
        bonusChoiceReadyTime = Time.time + 0.15f;
        bonusChoicePanel.SetActive(true);
        UpdateBonusChoiceButtons(labels);
        DeselectTower();
        UpdateWaveMessage(false, 0);
    }

    public void SelectTower(Tower tower)
    {
        selectedTower = tower;
        if (buildChoicePanel != null) buildChoicePanel.SetActive(false);
        if (pendingSpot != null) pendingSpot.SetBuildPreviewSelected(false);
        pendingSpot = null;
        UpdateTowerInfo();
    }

    public void DeselectTower()
    {
        selectedTower = null;
        UpdateTowerInfo();
    }

    void Update()
    {
        // 🔥 bloqueia interação se UI estiver aberta
        if (buildChoicePanel != null && buildChoicePanel.activeSelf)
            return;

        if (bonusChoicePanel != null && bonusChoicePanel.activeSelf)
            return;

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (IsPointerOverUI())
            return;

        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);

        float closestDistance = float.MaxValue;
        BuildSpot closestSpot = null;
        Tower closestTower = null;

        for (int i = 0; i < hits.Length; i++)
        {
            float dist = hits[i].distance;

            BuildSpot spot = hits[i].collider.GetComponentInParent<BuildSpot>();
            if (spot != null && dist < closestDistance)
            {
                closestDistance = dist;
                closestSpot = spot;
                closestTower = null;
                continue;
            }

            Tower tower = hits[i].collider.GetComponentInParent<Tower>();
            if (tower != null && dist < closestDistance)
            {
                closestDistance = dist;
                closestTower = tower;
                closestSpot = null;
            }
        }

        // 🔥 se clicou em um spot, deixa o próprio BuildSpot lidar (OnMouseDown)
        if (closestSpot != null)
            return;

        // 🔥 seleção de torre
        if (closestTower != null)
        {
            SelectTower(closestTower);
            return;
        }

        // 🔥 clique no vazio → limpa seleção
        if (buildChoicePanel != null)
            buildChoicePanel.SetActive(false);

        if (pendingSpot != null)
            pendingSpot.SetBuildPreviewSelected(false);

        pendingSpot = null;

        if (selectedTower != null)
            DeselectTower();

        UpdateWaveMessage(false, 0);
    }

    void UpdateTowerInfo()
    {
        if (towerInfoPanel == null || towerInfoText == null)
        {
            Debug.LogWarning("UIManager: towerInfoPanel or towerInfoText is null!", this);
            return;
        }

        if (selectedTower == null)
        {
            towerInfoPanel.SetActive(false);
            SetUpgradeButtonsActive(false);
            return;
        }

        Debug.Log("UpdateTowerInfo: Tower selected - " + selectedTower.towerType, this);
        towerInfoPanel.SetActive(true);
        SetUpgradeButtonsActive(true);
        UpdateUpgradeButtonLabels();

        towerInfoText.text =
            selectedTower.towerType.ToString().ToUpperInvariant() +
            "\n⚔ Damage    " + selectedTower.damage +
            "\n◉ Range      " + selectedTower.range.ToString("F1") +
            "\n⚡ Speed      " + selectedTower.fireRate.ToString("F2") +
            "\n✦ Levels  D" + selectedTower.upgradeDamageLevel + " R" + selectedTower.upgradeRangeLevel + " S" + selectedTower.upgradeFireRateLevel;
    }

    void UpdateUpgradeButtonLabels()
    {
        if (selectedTower == null) return;

        int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        SetUpgradeLabel(upgradeDamageButton, "UPG DMG", selectedTower.upgradeDamageLevel, selectedTower.maxUpgradeLevel, selectedTower.upgradeDamageCost, gold >= selectedTower.upgradeDamageCost);
        SetUpgradeLabel(upgradeRangeButton, "UPG RNG", selectedTower.upgradeRangeLevel, selectedTower.maxUpgradeLevel, selectedTower.upgradeRangeCost, gold >= selectedTower.upgradeRangeCost);
        SetUpgradeLabel(upgradeFireRateButton, "UPG SPD", selectedTower.upgradeFireRateLevel, selectedTower.maxUpgradeLevel, selectedTower.upgradeFireRateCost, gold >= selectedTower.upgradeFireRateCost);
    }

    void SetUpgradeLabel(Button button, string label, int level, int maxLevel, int cost, bool canPay)
    {
        if (button == null) return;

        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = level >= maxLevel ? label + "   MAX" : label + "   " + cost + "g";
        }

        button.interactable = level < maxLevel && canPay;
    }

    void SetUpgradeButtonsActive(bool active)
    {
        if (upgradeDamageButton != null) upgradeDamageButton.gameObject.SetActive(active);
        if (upgradeRangeButton != null) upgradeRangeButton.gameObject.SetActive(active);
        if (upgradeFireRateButton != null) upgradeFireRateButton.gameObject.SetActive(active);
    }

    void UpdateBuildChoiceLabels()
    {
        if (buildChoicePanel == null) return;

        int cost = pendingSpot != null ? pendingSpot.buildCost : 50;
        Button[] buttons = buildChoicePanel.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length && i < towerDisplayNames.Length; i++)
        {
            Text label = buttons[i].GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = towerDisplayNames[i] + "\n" + cost + "g";
            }

            buttons[i].interactable = GameManager.Instance == null || GameManager.Instance.Gold >= cost;
        }
    }

    void UpdateBonusChoiceButtons(string[] labels)
    {
        Button[] buttons = bonusChoicePanel.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            bool active = labels != null && i < labels.Length;
            buttons[i].gameObject.SetActive(active);
            if (!active) continue;

            Text label = buttons[i].GetComponentInChildren<Text>(true);
            if (label != null) label.text = labels[i];
        }
    }

    void ConfirmBuildTower(TowerType type)
    {
        if (pendingSpot == null) return;

        pendingSpot.BuildTower(type);

        pendingSpot.SetBuildPreviewSelected(false);
        pendingSpot = null;

        if (buildChoicePanel != null)
            buildChoicePanel.SetActive(false);

        UpdateWaveMessage(false, 0);
    }

    void ConfirmBonusChoice(int index)
    {
        if (Time.time < bonusChoiceReadyTime) return;

        if (bonusChoicePanel != null) bonusChoicePanel.SetActive(false);
        Action<int> callback = bonusChoiceCallback;
        bonusChoiceCallback = null;
        if (callback != null) callback(index);
        UpdateWaveMessage(false, 0);
    }

    void UpgradeDamage()
    {
        if (selectedTower != null && selectedTower.UpgradeDamage()) UpdateTowerInfo();
    }

    void UpgradeRange()
    {
        if (selectedTower != null && selectedTower.UpgradeRange()) UpdateTowerInfo();
    }

    void UpgradeFireRate()
    {
        if (selectedTower != null && selectedTower.UpgradeFireRate()) UpdateTowerInfo();
    }

    Text FindTextByName(string name)
    {
        GameObject obj = GameObject.Find(name);
        return obj != null ? obj.GetComponent<Text>() : null;
    }

    Button FindButtonByName(string name)
    {
        GameObject obj = GameObject.Find(name);
        return obj != null ? obj.GetComponent<Button>() : null;
    }

}
