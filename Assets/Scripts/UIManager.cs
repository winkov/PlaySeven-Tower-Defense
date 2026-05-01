using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public Text goldText;
    public Text castleHealthText;
    public Text waveText;
    public Text messageText;
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
        if (startWaveButton != null) startWaveButton.onClick.AddListener(StartWave);
        if (autoLayout) ApplySimpleLayout();
    }

    void Update()
    {
        HandleClickOutside();
    }

    public void UpdateGold(int gold) { if (goldText != null) goldText.text = "Gold: " + gold; if (selectedTower != null) UpdateUpgradeButtonLabels(); }
    public void UpdateCastleHealth(int health, int maxHealth) { if (castleHealthText != null) castleHealthText.text = "Castle HP: " + health + "/" + maxHealth; }
    public void UpdateWave(int wave) { waveDisplay = wave; if (waveText != null) waveText.text = "Stage: " + stageDisplay + "  Wave: " + waveDisplay; }
    public void UpdateStage(int stage) { stageDisplay = stage; if (waveText != null) waveText.text = "Stage: " + stageDisplay + "  Wave: " + waveDisplay; }
    public void UpdateBuildMode(bool active) { }
    public void UpdateGameOver(bool gameOver) { if (gameOver) ShowMessage("Game Over"); }
    public void UpdateWaveMessage(bool waveRunning, int aliveEnemies) { if (!bonusChoiceOpen) ShowMessage(waveRunning ? "Enemies: " + aliveEnemies : ""); }

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
        pendingSpot = spot;
        buildChoicePanel.SetActive(true);
        buildChoiceReadyTime = Time.time + 0.15f;
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
    void ConfirmBuildTower(TowerType type) { if (Time.time < buildChoiceReadyTime) return; if (pendingSpot != null) pendingSpot.BuildTower(type); pendingSpot = null; if (buildChoicePanel != null) buildChoicePanel.SetActive(false); }
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
        if (selectedTower != null) DeselectTower();
    }

    void CreateUIIfMissing()
    {
        if (canvas == null) return;
        if (towerInfoText != null) { towerInfoPanel = towerInfoText.transform.parent.gameObject; towerInfoRect = towerInfoPanel.GetComponent<RectTransform>(); return; }
        towerInfoPanel = new GameObject("TowerInfoPanel");
        towerInfoRect = towerInfoPanel.AddComponent<RectTransform>();
        towerInfoRect.SetParent(canvas.transform, false);
        towerInfoRect.anchorMin = new Vector2(0.5f, 0f); towerInfoRect.anchorMax = new Vector2(0.5f, 0f); towerInfoRect.pivot = new Vector2(0.5f, 0f);
        towerInfoRect.anchoredPosition = new Vector2(0f, 20f); towerInfoRect.sizeDelta = new Vector2(320f, 200f);
        Image bg = towerInfoPanel.AddComponent<Image>(); bg.color = new Color(0.08f, 0.10f, 0.15f, 0.94f);
        GameObject textObj = new GameObject("InfoText");
        towerInfoText = textObj.AddComponent<Text>(); towerInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); towerInfoText.fontSize = 13; towerInfoText.alignment = TextAnchor.UpperLeft; towerInfoText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.SetParent(towerInfoRect, false); textRect.anchorMin = new Vector2(0f, 0f); textRect.anchorMax = new Vector2(1f, 1f); textRect.offsetMin = new Vector2(12f, 72f); textRect.offsetMax = new Vector2(-12f, -30f);
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
        Image bg = panel.AddComponent<Image>(); bg.color = new Color(0.08f, 0.1f, 0.15f, 0.96f);
        return panel;
    }

    void CreateChoiceButton(Transform parent, string label, Vector2 offset, UnityEngine.Events.UnityAction action, string name = null)
    {
        GameObject buttonObj = new GameObject(string.IsNullOrEmpty(name) ? label + "Button" : name);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = offset; rect.sizeDelta = new Vector2(120f, 72f);
        Image img = buttonObj.AddComponent<Image>(); img.color = new Color(0.17f, 0.25f, 0.35f, 1f);
        Button btn = buttonObj.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(action);
        GameObject txtObj = new GameObject("Text");
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.SetParent(rect, false); txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.offsetMin = new Vector2(4f, 4f); txtRect.offsetMax = new Vector2(-4f, -4f);
        Text txt = txtObj.AddComponent<Text>(); txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); txt.text = label; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white; txt.fontSize = 12;
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
        if (upgradeDamageButton == null) upgradeDamageButton = CreateUpgradeButton("UpgradeDamageButton", "Damage", new Vector2(12f, -132f));
        if (upgradeRangeButton == null) upgradeRangeButton = CreateUpgradeButton("UpgradeRangeButton", "Range", new Vector2(112f, -132f));
        if (upgradeFireRateButton == null) upgradeFireRateButton = CreateUpgradeButton("UpgradeFireRateButton", "Fire", new Vector2(212f, -132f));
        ConfigureUpgradeButton(upgradeDamageButton, UpgradeDamage);
        ConfigureUpgradeButton(upgradeRangeButton, UpgradeRange);
        ConfigureUpgradeButton(upgradeFireRateButton, UpgradeFireRate);
        SetUpgradeButtonsActive(false);
    }

    Button CreateUpgradeButton(string name, string label, Vector2 pos)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(towerInfoRect, false); rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1); rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(90f, 42f);
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
    void UpgradeDamage() { if (selectedTower != null && selectedTower.UpgradeDamage()) UpdateTowerInfo(); }
    void UpgradeRange() { if (selectedTower != null && selectedTower.UpgradeRange()) UpdateTowerInfo(); }
    void UpgradeFireRate() { if (selectedTower != null && selectedTower.UpgradeFireRate()) UpdateTowerInfo(); }
    void SetUpgradeButtonsActive(bool a) { if (upgradeDamageButton != null) upgradeDamageButton.gameObject.SetActive(a); if (upgradeRangeButton != null) upgradeRangeButton.gameObject.SetActive(a); if (upgradeFireRateButton != null) upgradeFireRateButton.gameObject.SetActive(a); }
    void ShowMessage(string m) { if (messageText != null) messageText.text = m; }
    void FindMissingReferences() { if (goldText == null) goldText = FindTextByName("GoldText"); if (castleHealthText == null) castleHealthText = FindTextByName("CastleHPText"); if (waveText == null) waveText = FindTextByName("WaveText"); if (messageText == null) messageText = FindTextByName("MessageText"); if (startWaveButton == null) startWaveButton = FindButtonByName("StartWaveButton"); }
    void HideLegacyBuildButton() { GameObject old = GameObject.Find("BuildTowerButton"); if (old != null) old.SetActive(false); }
    Text FindTextByName(string n) { GameObject o = GameObject.Find(n); return o != null ? o.GetComponent<Text>() : null; }
    Button FindButtonByName(string n) { GameObject o = GameObject.Find(n); return o != null ? o.GetComponent<Button>() : null; }
    void ApplySimpleLayout() { PlaceText(goldText, new Vector2(16f, -16f)); PlaceText(castleHealthText, new Vector2(16f, -48f)); PlaceText(waveText, new Vector2(16f, -80f)); PlaceText(messageText, new Vector2(0f, -16f), TextAnchor.UpperCenter); PlaceButton(startWaveButton, new Vector2(-20f, -20f)); }
    void PlaceText(Text t, Vector2 p, TextAnchor a = TextAnchor.UpperLeft) { if (t == null) return; RectTransform r = t.GetComponent<RectTransform>(); r.anchorMin = a == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f); r.anchorMax = r.anchorMin; r.pivot = r.anchorMin; r.anchoredPosition = p; r.sizeDelta = a == TextAnchor.UpperCenter ? new Vector2(300f, 30f) : new Vector2(250f, 28f); t.alignment = a; t.fontSize = 18; t.color = Color.black; }
    void PlaceButton(Button b, Vector2 p) { if (b == null) return; RectTransform r = b.GetComponent<RectTransform>(); r.anchorMin = new Vector2(1f, 1f); r.anchorMax = new Vector2(1f, 1f); r.pivot = new Vector2(1f, 1f); r.anchoredPosition = p; r.sizeDelta = new Vector2(160f, 44f); }
}
