using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public Text goldText;
    public Text castleHealthText;
    public Text waveText;
    public Text messageText;
    public Text autoWaveText;
    public Button startWaveButton;
    public bool autoLayout = false;
    public Text towerInfoText;
    public Button upgradeDamageButton;
    public Button upgradeRangeButton;
    public Button upgradeFireRateButton;

    private WaveManager waveManager;
    private Tower selectedTower;
    private Canvas canvas;
    private Camera mainCamera;
    private GameObject towerInfoPanel;
    private RectTransform towerInfoRect;
    private int stageDisplay = 1;
    private int waveDisplay = 1;
    private GameObject buildChoicePanel;
    private GameObject bonusChoicePanel;
    private BuildSpot pendingSpot;
    private bool bonusChoiceOpen;
    private System.Action<int> bonusChoiceCallback;
    private float buildChoiceReadyTime;
    private Sprite panelSprite;
    private Sprite buttonSprite;
    private Sprite smallBarBaseSprite;
    private Sprite smallBarFillSprite;
    private Sprite bannerSprite;
    private Image hudTopLeftPanel;
    private Image castleHpBarBase;
    private Image castleHpBarFill;
    private Text waveBannerText;
    private int lastCastleHp = 10;
    private int lastCastleMaxHp = 10;

    void Start()
    {
        waveManager = FindAnyObjectByType<WaveManager>();
        canvas = FindAnyObjectByType<Canvas>();
        mainCamera = Camera.main;
        FindMissingReferences();
        CreateUIIfMissing();
        SetupUpgradeButtons();
        CreateBuildChoicePanelIfMissing();
        CreateBonusChoicePanelIfMissing();
        HideLegacyBuildButton();
        AutoAssignUISpritesInEditor();
        if (startWaveButton != null) startWaveButton.onClick.AddListener(StartWave);
        if (autoLayout) ApplySimpleLayout();
        CreateHudPanel();
        CreateCastleHpBar();
        CreateWaveBanner();
        ApplyFantasySkin();
    }

    void Update()
    {
        HandleClickOutside();
    }

    public void UpdateGold(int gold) { if (goldText != null) goldText.text = "Gold: " + gold; if (selectedTower != null) UpdateUpgradeButtonLabels(); }
    public void UpdateCastleHealth(int health, int maxHealth) { lastCastleHp = health; lastCastleMaxHp = Mathf.Max(1, maxHealth); if (castleHealthText != null) castleHealthText.text = "Castle HP: " + health + "/" + maxHealth; UpdateCastleHpBarVisual(); }
    public void UpdateWave(int wave) { waveDisplay = wave; if (waveText != null) waveText.text = "Stage: " + stageDisplay + "  Wave: " + waveDisplay; if (waveBannerText != null) waveBannerText.text = "Stage " + stageDisplay + " - Wave " + waveDisplay; }
    public void UpdateStage(int stage) { stageDisplay = stage; if (waveText != null) waveText.text = "Stage: " + stageDisplay + "  Wave: " + waveDisplay; if (waveBannerText != null) waveBannerText.text = "Stage " + stageDisplay + " - Wave " + waveDisplay; }
    public void UpdateBuildMode(bool active) { }
    public void UpdateGameOver(bool gameOver) { if (gameOver) ShowMessage("Game Over"); }
    public void UpdateWaveMessage(bool waveRunning, int aliveEnemies) { if (!bonusChoiceOpen) ShowMessage(waveRunning ? "Enemies: " + aliveEnemies : ""); }
    public void UpdateAutoWaveCountdown(float secondsLeft, bool firstWavePending) { if (autoWaveText != null) autoWaveText.text = firstWavePending ? "Auto: after first wave" : "Next wave in: " + Mathf.CeilToInt(secondsLeft) + "s"; }

    public void UpdateStartWaveButton(bool waveRunning, bool allWavesFinished)
    {
        if (startWaveButton == null) return;
        startWaveButton.interactable = !waveRunning && !allWavesFinished;
        Text t = startWaveButton.GetComponentInChildren<Text>();
        if (t != null) t.text = allWavesFinished ? "Victory" : waveRunning ? "Wave Running" : "Start Wave";
    }

    public void ShowBuildChoice(BuildSpot spot)
    {
        if (spot == null || buildChoicePanel == null) return;
        if (pendingSpot != null) pendingSpot.SetBuildPreviewSelected(false);
        pendingSpot = spot;
        pendingSpot.SetBuildPreviewSelected(true);
        buildChoicePanel.SetActive(true);
        buildChoiceReadyTime = Time.time + 0.15f;
        ShowMessage("Choose tower for highlighted spot");
        DeselectTower();
    }

    public void ShowBonusChoices(string[] labels, System.Action<int> onSelect)
    {
        if (labels == null || labels.Length < 3 || bonusChoicePanel == null) return;
        bonusChoiceOpen = true;
        bonusChoiceCallback = onSelect;
        ShowMessage("Choose a bonus");
        bonusChoicePanel.SetActive(true);
        SetBonusButtonLabel(0, labels[0]);
        SetBonusButtonLabel(1, labels[1]);
        SetBonusButtonLabel(2, labels[2]);
    }

    public void SelectTower(Tower tower) { selectedTower = tower; if (buildChoicePanel != null) buildChoicePanel.SetActive(false); UpdateTowerInfo(); }
    public void DeselectTower() { selectedTower = null; UpdateTowerInfo(); }

    void StartWave() { if (waveManager != null) waveManager.StartNextWave(); }
    void ConfirmBuildTower(TowerType type) { if (Time.time < buildChoiceReadyTime) return; if (pendingSpot != null) { pendingSpot.BuildTower(type); pendingSpot.SetBuildPreviewSelected(false); } pendingSpot = null; if (buildChoicePanel != null) buildChoicePanel.SetActive(false); ShowMessage(""); }
    void ConfirmBonusChoice(int idx) { bonusChoiceOpen = false; if (bonusChoicePanel != null) bonusChoicePanel.SetActive(false); ShowMessage(""); if (bonusChoiceCallback != null) bonusChoiceCallback(idx); bonusChoiceCallback = null; }

    void HandleClickOutside()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.GetComponentInParent<BuildSpot>() != null) return;
            Tower tower = hit.collider.GetComponentInParent<Tower>();
            if (tower != null) { SelectTower(tower); return; }
        }
        if (buildChoicePanel != null) buildChoicePanel.SetActive(false);
        if (pendingSpot != null) pendingSpot.SetBuildPreviewSelected(false);
        pendingSpot = null;
        if (selectedTower != null) DeselectTower();
    }

    void CreateUIIfMissing()
    {
        if (canvas == null) return;
        if (towerInfoText != null) { towerInfoPanel = towerInfoText.transform.parent.gameObject; towerInfoRect = towerInfoPanel.GetComponent<RectTransform>(); return; }
        towerInfoPanel = new GameObject("TowerInfoPanel");
        towerInfoRect = towerInfoPanel.AddComponent<RectTransform>();
        towerInfoRect.SetParent(canvas.transform, false);
        towerInfoRect.anchorMin = new Vector2(1f, 0.5f); towerInfoRect.anchorMax = new Vector2(1f, 0.5f); towerInfoRect.pivot = new Vector2(1f, 0.5f);
        towerInfoRect.anchoredPosition = new Vector2(-10f, -10f); towerInfoRect.sizeDelta = new Vector2(250f, 160f);
        Image bg = towerInfoPanel.AddComponent<Image>(); bg.color = new Color(0.08f, 0.10f, 0.15f, 0.94f);
        GameObject textObj = new GameObject("InfoText");
        towerInfoText = textObj.AddComponent<Text>(); towerInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); towerInfoText.fontSize = 13; towerInfoText.alignment = TextAnchor.UpperLeft; towerInfoText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.SetParent(towerInfoRect, false); textRect.anchorMin = new Vector2(0f, 0f); textRect.anchorMax = new Vector2(1f, 1f); textRect.offsetMin = new Vector2(10f, 58f); textRect.offsetMax = new Vector2(-10f, -24f);
        towerInfoPanel.SetActive(false);
    }

    void CreateBuildChoicePanelIfMissing()
    {
        if (canvas == null || buildChoicePanel != null) return;
        buildChoicePanel = CreatePanel("BuildChoicePanel", new Vector2(380f, 120f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f));
        CreateChoiceButton(buildChoicePanel.transform, "Mage", new Vector2(-140f, 0f), () => ConfirmBuildTower(TowerType.Mage));
        CreateChoiceButton(buildChoicePanel.transform, "Archer", new Vector2(-45f, 0f), () => ConfirmBuildTower(TowerType.Archer));
        CreateChoiceButton(buildChoicePanel.transform, "Catapult", new Vector2(50f, 0f), () => ConfirmBuildTower(TowerType.Catapult));
        CreateChoiceButton(buildChoicePanel.transform, "Barracks", new Vector2(145f, 0f), () => ConfirmBuildTower(TowerType.Barracks));
        buildChoicePanel.SetActive(false);
    }

    void CreateBonusChoicePanelIfMissing()
    {
        if (canvas == null || bonusChoicePanel != null) return;
        bonusChoicePanel = CreatePanel("BonusChoicePanel", new Vector2(420f, 140f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f));
        CreateChoiceButton(bonusChoicePanel.transform, "Bonus 1", new Vector2(-130f, 0f), () => ConfirmBonusChoice(0), "BonusButton1");
        CreateChoiceButton(bonusChoicePanel.transform, "Bonus 2", new Vector2(0f, 0f), () => ConfirmBonusChoice(1), "BonusButton2");
        CreateChoiceButton(bonusChoicePanel.transform, "Bonus 3", new Vector2(130f, 0f), () => ConfirmBonusChoice(2), "BonusButton3");
        bonusChoicePanel.SetActive(false);
    }

    GameObject CreatePanel(string name, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 pos)
    {
        GameObject panel = new GameObject(name);
        RectTransform r = panel.AddComponent<RectTransform>();
        r.SetParent(canvas.transform, false);
        r.anchorMin = anchor; r.anchorMax = anchor; r.pivot = pivot; r.anchoredPosition = pos; r.sizeDelta = size;
        Image bg = panel.AddComponent<Image>(); bg.color = new Color(0.06f, 0.09f, 0.14f, 0.96f); if (panelSprite != null) { bg.sprite = panelSprite; bg.type = Image.Type.Sliced; bg.color = new Color(1f, 1f, 1f, 0.95f); }
        return panel;
    }

    void CreateChoiceButton(Transform parent, string label, Vector2 offset, UnityEngine.Events.UnityAction action, string name = null)
    {
        GameObject buttonObj = new GameObject(string.IsNullOrEmpty(name) ? label + "Button" : name);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = offset; rect.sizeDelta = new Vector2(120f, 72f);
        Image img = buttonObj.AddComponent<Image>(); img.color = new Color(0.2f, 0.13f, 0.08f, 0.95f); if (buttonSprite != null) { img.sprite = buttonSprite; img.type = Image.Type.Sliced; img.color = Color.white; }
        Button btn = buttonObj.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(action);
        GameObject txtObj = new GameObject("Text");
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.SetParent(rect, false); txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.offsetMin = new Vector2(4f, 4f); txtRect.offsetMax = new Vector2(-4f, -4f);
        Text txt = txtObj.AddComponent<Text>(); txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); txt.text = label; txt.alignment = TextAnchor.MiddleCenter; txt.color = new Color(0.98f, 0.96f, 0.9f); txt.fontSize = 13; txt.fontStyle = FontStyle.Bold;
    }

    void SetBonusButtonLabel(int index, string label)
    {
        if (bonusChoicePanel == null) return;
        Transform button = bonusChoicePanel.transform.Find("BonusButton" + (index + 1));
        if (button == null) return;
        Text txt = button.GetComponentInChildren<Text>();
        if (txt != null) txt.text = label;
    }

    void UpdateTowerInfo()
    {
        if (towerInfoText == null) return;
        if (selectedTower == null) { towerInfoText.text = ""; if (towerInfoPanel != null) towerInfoPanel.SetActive(false); SetUpgradeButtonsActive(false); return; }
        if (towerInfoPanel != null) towerInfoPanel.SetActive(true);
        SetUpgradeButtonsActive(true);
        UpdateUpgradeButtonLabels();
        towerInfoText.text = "Type: " + selectedTower.towerType + "\nDamage: " + selectedTower.damage + " (Lvl " + selectedTower.upgradeDamageLevel + "/" + selectedTower.maxUpgradeLevel + ")\nRange: " + selectedTower.range.ToString("F1") + " (Lvl " + selectedTower.upgradeRangeLevel + "/" + selectedTower.maxUpgradeLevel + ")\nFire Rate: " + selectedTower.fireRate.ToString("F1") + " (Lvl " + selectedTower.upgradeFireRateLevel + "/" + selectedTower.maxUpgradeLevel + ")";
    }

    void SetupUpgradeButtons()
    {
        if (towerInfoRect == null) return;
        if (upgradeDamageButton == null) upgradeDamageButton = CreateUpgradeButton("UpgradeDamageButton", "Damage", new Vector2(8f, -108f));
        if (upgradeRangeButton == null) upgradeRangeButton = CreateUpgradeButton("UpgradeRangeButton", "Range", new Vector2(86f, -108f));
        if (upgradeFireRateButton == null) upgradeFireRateButton = CreateUpgradeButton("UpgradeFireRateButton", "Fire", new Vector2(164f, -108f));
        ConfigureUpgradeButton(upgradeDamageButton, UpgradeDamage);
        ConfigureUpgradeButton(upgradeRangeButton, UpgradeRange);
        ConfigureUpgradeButton(upgradeFireRateButton, UpgradeFireRate);
        SetUpgradeButtonsActive(false);
    }

    Button CreateUpgradeButton(string name, string label, Vector2 pos)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(towerInfoRect, false); rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1); rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(72f, 38f);
        Image img = obj.AddComponent<Image>(); img.color = new Color(0.14f, 0.22f, 0.34f, 1f);
        Button b = obj.AddComponent<Button>(); b.targetGraphic = img;
        GameObject t = new GameObject("Text");
        RectTransform tr = t.AddComponent<RectTransform>();
        tr.SetParent(rect, false); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = new Vector2(4f, 4f); tr.offsetMax = new Vector2(-4f, -4f);
        Text tx = t.AddComponent<Text>(); tx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); tx.text = label; tx.fontSize = 14; tx.alignment = TextAnchor.MiddleCenter; tx.color = Color.white;
        obj.SetActive(false); return b;
    }

    void ConfigureUpgradeButton(Button b, UnityEngine.Events.UnityAction a) { if (b == null) return; b.onClick.RemoveListener(a); b.onClick.AddListener(a); }
    void UpdateUpgradeButtonLabels()
    {
        if (selectedTower == null) return;
        int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        SetUpgradeLabel(upgradeDamageButton, "Damage", selectedTower.upgradeDamageLevel, selectedTower.maxUpgradeLevel, selectedTower.upgradeDamageCost, gold >= selectedTower.upgradeDamageCost);
        SetUpgradeLabel(upgradeRangeButton, "Range", selectedTower.upgradeRangeLevel, selectedTower.maxUpgradeLevel, selectedTower.upgradeRangeCost, gold >= selectedTower.upgradeRangeCost);
        SetUpgradeLabel(upgradeFireRateButton, "Fire", selectedTower.upgradeFireRateLevel, selectedTower.maxUpgradeLevel, selectedTower.upgradeFireRateCost, gold >= selectedTower.upgradeFireRateCost);
    }

    void SetUpgradeLabel(Button b, string label, int level, int max, int cost, bool canPay) { if (b == null) return; Text t = b.GetComponentInChildren<Text>(); if (t != null) t.text = level >= max ? label + "\nMAX" : label + "\n" + cost + "g"; b.interactable = level < max && canPay; }
    void UpgradeDamage() { if (selectedTower != null && selectedTower.UpgradeDamage()) { selectedTower.PlayUpgradeFeedback(new Color(1f, 0.35f, 0.2f)); UpdateTowerInfo(); } }
    void UpgradeRange() { if (selectedTower != null && selectedTower.UpgradeRange()) { selectedTower.PlayUpgradeFeedback(new Color(0.25f, 0.75f, 1f)); UpdateTowerInfo(); } }
    void UpgradeFireRate() { if (selectedTower != null && selectedTower.UpgradeFireRate()) { selectedTower.PlayUpgradeFeedback(new Color(1f, 0.95f, 0.35f)); UpdateTowerInfo(); } }
    void SetUpgradeButtonsActive(bool a) { if (upgradeDamageButton != null) upgradeDamageButton.gameObject.SetActive(a); if (upgradeRangeButton != null) upgradeRangeButton.gameObject.SetActive(a); if (upgradeFireRateButton != null) upgradeFireRateButton.gameObject.SetActive(a); }
    void ShowMessage(string m) { if (messageText != null) messageText.text = m; }
    void FindMissingReferences() { if (goldText == null) goldText = FindTextByName("GoldText"); if (castleHealthText == null) castleHealthText = FindTextByName("CastleHPText"); if (waveText == null) waveText = FindTextByName("WaveText"); if (messageText == null) messageText = FindTextByName("MessageText"); if (autoWaveText == null) autoWaveText = FindTextByName("AutoWaveText"); if (startWaveButton == null) startWaveButton = FindButtonByName("StartWaveButton"); }
    void HideLegacyBuildButton() { GameObject old = GameObject.Find("BuildTowerButton"); if (old != null) old.SetActive(false); }
    Text FindTextByName(string n) { GameObject o = GameObject.Find(n); return o != null ? o.GetComponent<Text>() : null; }
    Button FindButtonByName(string n) { GameObject o = GameObject.Find(n); return o != null ? o.GetComponent<Button>() : null; }
    void ApplySimpleLayout() { PlaceText(goldText, new Vector2(16f, -16f)); PlaceText(castleHealthText, new Vector2(16f, -48f)); PlaceText(waveText, new Vector2(16f, -80f)); PlaceText(messageText, new Vector2(0f, -16f), TextAnchor.UpperCenter); PlaceButton(startWaveButton, new Vector2(-20f, -20f)); }
    void PlaceText(Text t, Vector2 p, TextAnchor a = TextAnchor.UpperLeft) { if (t == null) return; RectTransform r = t.GetComponent<RectTransform>(); r.anchorMin = a == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f); r.anchorMax = r.anchorMin; r.pivot = r.anchorMin; r.anchoredPosition = p; r.sizeDelta = a == TextAnchor.UpperCenter ? new Vector2(320f, 34f) : new Vector2(270f, 32f); t.alignment = a; t.fontSize = 20; t.fontStyle = FontStyle.Bold; t.color = new Color(0.92f, 0.9f, 0.75f); }
    void PlaceButton(Button b, Vector2 p) { if (b == null) return; RectTransform r = b.GetComponent<RectTransform>(); r.anchorMin = new Vector2(1f, 1f); r.anchorMax = new Vector2(1f, 1f); r.pivot = new Vector2(1f, 1f); r.anchoredPosition = p; r.sizeDelta = new Vector2(160f, 44f); }

    void ApplyFantasySkin()
    {
        StyleLabel(goldText);
        StyleLabel(castleHealthText);
        StyleLabel(waveText);
        StyleLabel(autoWaveText);
        StyleLabel(messageText);
        StyleButton(startWaveButton, new Color(0.2f, 0.13f, 0.08f, 0.95f));
        StyleButton(upgradeDamageButton, new Color(0.25f, 0.17f, 0.1f, 0.95f));
        StyleButton(upgradeRangeButton, new Color(0.25f, 0.17f, 0.1f, 0.95f));
        StyleButton(upgradeFireRateButton, new Color(0.25f, 0.17f, 0.1f, 0.95f));
    }

    void StyleLabel(Text t)
    {
        if (t == null) return;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.95f, 0.9f, 0.75f);
        if (t.fontSize < 20) t.fontSize = 20;
    }

    void CreateHudPanel()
    {
        if (canvas == null || goldText == null) return;
        GameObject panelObj = new GameObject("HUDTopLeftPanel");
        RectTransform rect = panelObj.AddComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(8f, -8f);
        rect.sizeDelta = new Vector2(360f, 150f);
        hudTopLeftPanel = panelObj.AddComponent<Image>();
        if (panelSprite != null)
        {
            hudTopLeftPanel.sprite = panelSprite;
            hudTopLeftPanel.type = Image.Type.Sliced;
            hudTopLeftPanel.color = new Color(1f, 1f, 1f, 0.92f);
        }
        else
        {
            hudTopLeftPanel.color = new Color(0f, 0f, 0f, 0.35f);
        }
    }

    void CreateCastleHpBar()
    {
        if (hudTopLeftPanel == null) return;
        GameObject hpBaseObj = new GameObject("CastleHPBarBase");
        RectTransform baseRect = hpBaseObj.AddComponent<RectTransform>();
        baseRect.SetParent(hudTopLeftPanel.transform, false);
        baseRect.anchorMin = new Vector2(0f, 1f);
        baseRect.anchorMax = new Vector2(0f, 1f);
        baseRect.pivot = new Vector2(0f, 1f);
        baseRect.anchoredPosition = new Vector2(16f, -112f);
        baseRect.sizeDelta = new Vector2(230f, 20f);
        castleHpBarBase = hpBaseObj.AddComponent<Image>();
        if (smallBarBaseSprite != null) { castleHpBarBase.sprite = smallBarBaseSprite; castleHpBarBase.type = Image.Type.Sliced; castleHpBarBase.color = Color.white; } else castleHpBarBase.color = new Color(0f, 0f, 0f, 0.5f);

        GameObject hpFillObj = new GameObject("CastleHPBarFill");
        RectTransform fillRect = hpFillObj.AddComponent<RectTransform>();
        fillRect.SetParent(hpBaseObj.transform, false);
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(3f, 0f);
        fillRect.sizeDelta = new Vector2(224f, 14f);
        castleHpBarFill = hpFillObj.AddComponent<Image>();
        if (smallBarFillSprite != null) { castleHpBarFill.sprite = smallBarFillSprite; castleHpBarFill.type = Image.Type.Sliced; castleHpBarFill.color = new Color(1f, 0.3f, 0.3f, 1f); } else castleHpBarFill.color = new Color(0.95f, 0.2f, 0.2f, 0.9f);
        UpdateCastleHpBarVisual();
    }

    void CreateWaveBanner()
    {
        if (canvas == null) return;
        GameObject bannerObj = new GameObject("WaveBanner");
        RectTransform rect = bannerObj.AddComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -10f);
        rect.sizeDelta = new Vector2(360f, 70f);
        Image bg = bannerObj.AddComponent<Image>();
        if (bannerSprite != null) { bg.sprite = bannerSprite; bg.type = Image.Type.Sliced; bg.color = Color.white; } else bg.color = new Color(0f, 0f, 0f, 0.4f);

        GameObject txtObj = new GameObject("WaveBannerText");
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.SetParent(rect, false);
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(12f, 8f); txtRect.offsetMax = new Vector2(-12f, -8f);
        waveBannerText = txtObj.AddComponent<Text>();
        waveBannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        waveBannerText.fontSize = 24;
        waveBannerText.fontStyle = FontStyle.Bold;
        waveBannerText.alignment = TextAnchor.MiddleCenter;
        waveBannerText.color = new Color(0.97f, 0.93f, 0.82f);
        waveBannerText.text = "Stage 1 - Wave 1";
    }

    void UpdateCastleHpBarVisual()
    {
        if (castleHpBarFill == null) return;
        float pct = Mathf.Clamp01((float)lastCastleHp / lastCastleMaxHp);
        RectTransform r = castleHpBarFill.GetComponent<RectTransform>();
        if (r != null) r.sizeDelta = new Vector2(Mathf.Lerp(0f, 224f, pct), 14f);
        castleHpBarFill.color = Color.Lerp(new Color(0.65f, 0.08f, 0.08f, 1f), new Color(1f, 0.3f, 0.3f, 1f), pct);
    }

    void StyleButton(Button b, Color color)
    {
        if (b == null) return;
        Image img = b.GetComponent<Image>();
        if (img != null)
        {
            img.color = buttonSprite != null ? Color.white : color;
            if (buttonSprite != null) { img.sprite = buttonSprite; img.type = Image.Type.Sliced; }
        }
        Text txt = b.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.color = new Color(0.98f, 0.94f, 0.84f);
            txt.fontStyle = FontStyle.Bold;
            txt.fontSize = 16;
        }
    }

    void AutoAssignUISpritesInEditor()
    {
#if UNITY_EDITOR
        if (panelSprite == null) panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/UI Elements/Papers/RegularPaper.png");
        if (buttonSprite == null) buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/UI Elements/Buttons/BigBlueButton_Regular.png");
        if (smallBarBaseSprite == null) smallBarBaseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/UI Elements/Bars/SmallBar_Base.png");
        if (smallBarFillSprite == null) smallBarFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/UI Elements/Bars/SmallBar_Fill.png");
        if (bannerSprite == null) bannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/UI Elements/Banners/Banner.png");
#endif
    }
}
