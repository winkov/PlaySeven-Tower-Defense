using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class UIManager : MonoBehaviour
{
    public Text goldText;
    public Text castleHealthText;
    public Text waveText;
    public Text messageText;
    public Button startWaveButton;
    public Button buildTowerButton;
    public bool autoLayout = false;

    public Text towerInfoText;
    public Button upgradeDamageButton;
    public Button upgradeRangeButton;
    public Button upgradeFireRateButton;

    private WaveManager waveManager;
    private BuildManager buildManager;
    private Tower selectedTower;
    private Canvas canvas;
    private Camera mainCamera;
    private GameObject towerInfoPanel;
    private RectTransform towerInfoRect;
    private RectTransform upgradeDamageRect;
    private RectTransform upgradeRangeRect;
    private RectTransform upgradeFireRateRect;

    void Start()
    {
        waveManager = FindAnyObjectByType<WaveManager>();
        Debug.Log("WaveManager found: " + (waveManager != null), this);
        canvas = FindAnyObjectByType<Canvas>();
        Debug.Log("Canvas found: " + (canvas != null), this);
        mainCamera = Camera.main;
        Debug.Log("Main Camera found: " + (mainCamera != null), this);
        buildManager = FindAnyObjectByType<BuildManager>();
        Debug.Log("BuildManager found: " + (buildManager != null), this);
        FindMissingReferences();
        CreateUIIfMissing();

        Debug.Log("UIManager Start - Finding buttons...", this);

        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartWave);
            Debug.Log("StartWave button listener added", this);
        }
        else
        {
            Debug.LogWarning("UIManager could not find StartWaveButton. Drag it into the UIManager or name the button StartWaveButton.", this);
        }

        if (buildTowerButton != null)
        {
            buildTowerButton.onClick.AddListener(ToggleBuildMode);
            Debug.Log("BuildTower button listener added", this);
        }
        else
        {
            Debug.LogWarning("UIManager could not find BuildTowerButton. Drag it into the UIManager or name the button BuildTowerButton.", this);
        }

        if (autoLayout)
        {
            ApplySimpleLayout();
        }

        Debug.Log("UIManager Start completed", this);
    }

    public void UpdateGold(int gold)
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + gold;
        }
    }
    void CreateUIIfMissing()
{
    // 🔥 SE JÁ EXISTE UI NO CANVAS → USA ELA
    if (towerInfoText != null)
    {
        towerInfoPanel = towerInfoText.transform.parent.gameObject;
        towerInfoRect = towerInfoPanel.GetComponent<RectTransform>();
        return;
    }

    // 🔥 SE NÃO EXISTE → CRIA (seu código original)
    towerInfoPanel = new GameObject("TowerInfoPanel");
    towerInfoRect = towerInfoPanel.AddComponent<RectTransform>();
    towerInfoRect.SetParent(canvas.transform, false);
    towerInfoRect.anchorMin = new Vector2(0.5f, 0f);
    towerInfoRect.anchorMax = new Vector2(0.5f, 0f);
    towerInfoRect.pivot = new Vector2(0.5f, 0f);
    towerInfoRect.anchoredPosition = new Vector2(0f, 20f);
    towerInfoRect.sizeDelta = new Vector2(250f, 140f);

    Image bgImage = towerInfoPanel.AddComponent<Image>();
    bgImage.color = new Color(0.08f, 0.10f, 0.15f, 0.94f);
    bgImage.raycastTarget = false;

    GameObject headerObj = new GameObject("Header");
    RectTransform headerRect = headerObj.AddComponent<RectTransform>();
    headerRect.SetParent(towerInfoRect, false);
    headerRect.anchorMin = new Vector2(0f, 1f);
    headerRect.anchorMax = new Vector2(1f, 1f);
    headerRect.pivot = new Vector2(0.5f, 1f);
    headerRect.anchoredPosition = Vector2.zero;
    headerRect.sizeDelta = new Vector2(0f, 28f);

    Image headerImage = headerObj.AddComponent<Image>();
    headerImage.color = new Color(0.12f, 0.45f, 0.68f, 1f);
    headerImage.raycastTarget = false;

    GameObject headerTextObj = new GameObject("HeaderText");
    Text headerText = headerTextObj.AddComponent<Text>();
    headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    headerText.text = "Tower Control";
    headerText.fontSize = 14;
    headerText.fontStyle = FontStyle.Bold;
    headerText.alignment = TextAnchor.MiddleCenter;
    headerText.color = Color.white;
    headerText.raycastTarget = false;

    RectTransform headerTextRect = headerTextObj.GetComponent<RectTransform>();
    headerTextRect.SetParent(headerRect, false);
    headerTextRect.anchorMin = Vector2.zero;
    headerTextRect.anchorMax = Vector2.one;
    headerTextRect.offsetMin = Vector2.zero;
    headerTextRect.offsetMax = Vector2.zero;

    GameObject textObj = new GameObject("InfoText");
    towerInfoText = textObj.AddComponent<Text>();
    towerInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    towerInfoText.text = "";
    towerInfoText.fontSize = 13;
    towerInfoText.alignment = TextAnchor.UpperLeft;
    towerInfoText.color = new Color(0.92f, 0.96f, 1f, 1f);

    RectTransform textRect = textObj.GetComponent<RectTransform>();
    textRect.SetParent(towerInfoRect, false);
    textRect.anchorMin = new Vector2(0f, 0f);
    textRect.anchorMax = new Vector2(1f, 1f);
    textRect.offsetMin = new Vector2(12f, 50f);
    textRect.offsetMax = new Vector2(-12f, -42f);

    towerInfoPanel.SetActive(false);
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
        Debug.Log("StartWave button clicked!", this);
        if (waveManager != null)
        {
            Debug.Log("Calling waveManager.StartNextWave()", this);
            waveManager.StartNextWave();
        }
        else
        {
            Debug.LogWarning("No WaveManager found in the scene.", this);
        }
    }

    void ToggleBuildMode()
    {
        Debug.Log("BuildTower button clicked!", this);
        if (buildManager != null)
        {
            Debug.Log("Calling buildManager.ToggleBuildMode()", this);
            buildManager.ToggleBuildMode();
        }
        else
        {
            Debug.LogWarning("No BuildManager found in the scene.", this);
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
        Debug.Log("Finding missing UI references...", this);

        if (goldText == null)
        {
            goldText = FindTextByName("GoldText");
            Debug.Log("GoldText found: " + (goldText != null), this);
        }

        if (castleHealthText == null)
        {
            castleHealthText = FindTextByName("CastleHPText");
            Debug.Log("CastleHPText found: " + (castleHealthText != null), this);
        }

        if (waveText == null)
        {
            waveText = FindTextByName("WaveText");
            Debug.Log("WaveText found: " + (waveText != null), this);
        }

        if (messageText == null)
        {
            messageText = FindTextByName("MessageText");
            Debug.Log("MessageText found: " + (messageText != null), this);
        }

        if (startWaveButton == null)
        {
            startWaveButton = FindButtonByName("StartWaveButton");
            Debug.Log("StartWaveButton found: " + (startWaveButton != null), this);
        }

        if (buildTowerButton == null)
        {
            buildTowerButton = FindButtonByName("BuildTowerButton");
            Debug.Log("BuildTowerButton found: " + (buildTowerButton != null), this);
        }
    }
void HandleClickOutside()
{
    if (Mouse.current == null) return;

    if (!Mouse.current.leftButton.wasPressedThisFrame)
        return;

    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

    if (Physics.Raycast(ray, out RaycastHit hit))
    {
        Tower tower = hit.collider.GetComponent<Tower>();

        if (tower != null)
        {
            SelectTower(tower);
            return;
        }
    }

    if (selectedTower != null)
    {
        DeselectTower();
    }
}
    void Update()
    {
        HandleClickOutside();
    }


    Button CreateUpgradeButton(string name, string label, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.SetParent(towerInfoRect != null ? towerInfoRect : canvas.transform, false);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(72f, 42f);

        Image bgImage = buttonObj.AddComponent<Image>();
        bgImage.color = new Color(0.14f, 0.22f, 0.34f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.14f, 0.22f, 0.34f, 1f);
        colors.highlightedColor = new Color(0.22f, 0.34f, 0.54f, 1f);
        colors.pressedColor = new Color(0.10f, 0.16f, 0.26f, 1f);
        colors.disabledColor = new Color(0.08f, 0.12f, 0.18f, 0.45f);
        button.colors = colors;
        button.targetGraphic = bgImage;

        GameObject textObj = new GameObject("Text");
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(rectTransform, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(4f, 4f);
        textRect.offsetMax = new Vector2(-4f, -4f);

        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.text = label;
        buttonText.fontSize = 16;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        buttonText.horizontalOverflow = HorizontalWrapMode.Wrap;
        buttonText.verticalOverflow = VerticalWrapMode.Truncate;

        buttonObj.SetActive(false);
        return button;
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

    public void SelectTower(Tower tower)
    {
        Debug.Log("Tower selected: " + (tower != null ? tower.name : "null"), this);


        selectedTower = tower;
        UpdateTowerInfo();
    }

    public void DeselectTower()
    {
        selectedTower = null;
        UpdateTowerInfo();
    }

    void UpdateTowerInfo()
    {
        if (towerInfoText == null) return;

        if (selectedTower == null)
        {
            towerInfoText.text = "";
            if (towerInfoPanel != null)
            {
                towerInfoPanel.SetActive(false);
            }
            SetUpgradeButtonsActive(false);
            return;
        }

        if (towerInfoPanel != null)
        {
            towerInfoPanel.SetActive(true);
        }

        SetUpgradeButtonsActive(true);
        UpdateUpgradeButtonLabels();

        string info = "Tower Info:\n";
        info += "Damage: " + selectedTower.damage + " (Lvl " + selectedTower.upgradeDamageLevel + ")\n";
        info += "Range: " + selectedTower.range.ToString("F1") + " (Lvl " + selectedTower.upgradeRangeLevel + ")\n";
        info += "Fire Rate: " + selectedTower.fireRate.ToString("F1") + " (Lvl " + selectedTower.upgradeFireRateLevel + ")";

        towerInfoText.text = info;
    }

    void SetUpgradeButtonsActive(bool active)
    {
        if (upgradeDamageButton != null)
            upgradeDamageButton.gameObject.SetActive(active);
        if (upgradeRangeButton != null)
            upgradeRangeButton.gameObject.SetActive(active);
        if (upgradeFireRateButton != null)
            upgradeFireRateButton.gameObject.SetActive(active);
    }

    void UpdateUpgradeButtonLabels()
    {
        if (selectedTower == null) return;

        if (upgradeDamageButton != null)
        {
            Text buttonText = upgradeDamageButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "⚔ Damage\n" + selectedTower.upgradeDamageCost + "g";
            }
            upgradeDamageButton.interactable = selectedTower.upgradeDamageLevel < selectedTower.maxUpgradeLevel && GameManager.Instance.Gold >= selectedTower.upgradeDamageCost;
        }

        if (upgradeRangeButton != null)
        {
            Text buttonText = upgradeRangeButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "🎯 Range\n" + selectedTower.upgradeRangeCost + "g";
            }
            upgradeRangeButton.interactable = selectedTower.upgradeRangeLevel < selectedTower.maxUpgradeLevel && GameManager.Instance.Gold >= selectedTower.upgradeRangeCost;
        }

        if (upgradeFireRateButton != null)
        {
            Text buttonText = upgradeFireRateButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "🔥 Fire\n" + selectedTower.upgradeFireRateCost + "g";
            }
            upgradeFireRateButton.interactable = selectedTower.upgradeFireRateLevel < selectedTower.maxUpgradeLevel && GameManager.Instance.Gold >= selectedTower.upgradeFireRateCost;
        }
    }

    void UpgradeDamage()
    {
        if (selectedTower != null)
        {
            selectedTower.UpgradeDamage();
            UpdateTowerInfo();
        }
    }

    void UpgradeRange()
    {
        if (selectedTower != null)
        {
            selectedTower.UpgradeRange();
            UpdateTowerInfo();
        }
    }

    void UpgradeFireRate()
    {
        if (selectedTower != null)
        {
            selectedTower.UpgradeFireRate();
            UpdateTowerInfo();
        }
    }
}
