using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public static bool isGameStarted = false;

    public static int gameSpeedLevel = 2;

    private Canvas canvas;
    private GameObject canvasRoot;
    private GameObject createdEventSystem;
    private GameObject startMenuPanel;
    private GameObject hudPanel;
    private GameObject gameOverPanel;

    private Image lifeBar1;
    private Image lifeBar2;
    private readonly Color lifeActiveColor = new Color(0.18f, 0.85f, 0.45f, 1f);
    private readonly Color lifeInactiveColor = new Color(0.25f, 0.28f, 0.35f, 0.7f);

    private TextMeshProUGUI startHighScoreText;
    private TextMeshProUGUI speedValueText;
    private TextMeshProUGUI finalScoreText;
    private TextMeshProUGUI finalHighScoreText;

    private const string HIGH_SCORE_KEY = "HighScore";
    private const string SPEED_KEY = "GameSpeedLevel";

    public bool isAddActive = true;
    public bool isSubActive = true;
    public bool isMulActive = true;
    public bool isDivActive = true;

    private Image[] opButtonImages = new Image[4];
    private readonly Button[] operationButtons = new Button[4];
    private readonly UnityAction[] operationActions = new UnityAction[4];
    private Button startButton;
    private Button retryButton;
    private Button reviveButton;
    private TextMeshProUGUI reviveLabel;
    private RectTransform gameOverContent;
    private Slider speedSlider;
    private bool reviveUsedThisRun;
    private bool adInProgress;
    private const string ReviveLabelText = "REKLAM İZLE VE DEVAM ET";
    private readonly Color activeColor = new Color(0.18f, 0.55f, 0.95f, 1f);
    private readonly Color inactiveColor = new Color(0.2f, 0.23f, 0.3f, 1f);

    private Coroutine gameOverRoutine;
    private Coroutine popRoutine;
    private bool gameOverSequenceRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadSettings();
        CreateModernUI();
    }

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        isGameStarted = false;
        Time.timeScale = 0f;
        ShowStartMenu();
    }

    private void OnDestroy()
    {
        StopUiRoutines();
        ReleaseUiHooks();

        if (createdEventSystem != null)
        {
            Destroy(createdEventSystem);
        }

        if (canvasRoot != null)
        {
            Destroy(canvasRoot);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LoadSettings()
    {
        isAddActive = SafePrefs.GetInt("Op_Add", 1) == 1;
        isSubActive = SafePrefs.GetInt("Op_Sub", 1) == 1;
        isMulActive = SafePrefs.GetInt("Op_Mul", 1) == 1;
        isDivActive = SafePrefs.GetInt("Op_Div", 1) == 1;

        if (!isAddActive && !isSubActive && !isMulActive && !isDivActive)
        {
            isAddActive = true;
        }

        gameSpeedLevel = SafePrefs.GetInt(SPEED_KEY, 2);
        if (gameSpeedLevel < 1 || gameSpeedLevel > 3)
        {
            gameSpeedLevel = 2;
        }
    }

    public static float GetSpeed()
    {
        switch (gameSpeedLevel)
        {
            case 1: return 12f;
            case 3: return 24f;
            default: return 18f;
        }
    }

    private void CreateModernUI()
    {
        canvasRoot = new GameObject("Dynamic_Modern_Canvas");
        canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasRoot.AddComponent<GraphicRaycaster>();

        if (SceneQuery.FindFirst<UnityEngine.EventSystems.EventSystem>() == null)
        {
            createdEventSystem = new GameObject("EventSystem");
            createdEventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            createdEventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        BuildStartMenu(canvasRoot.transform);
        BuildHUD(canvasRoot.transform);
        BuildGameOverMenu(canvasRoot.transform);
    }

    private void BuildStartMenu(Transform parent)
    {
        startMenuPanel = CreatePanel(parent, "StartMenuPanel", new Color(0.05f, 0.08f, 0.15f, 0.92f));

        CreateText(startMenuPanel.transform, "BrainRun", 95, new Vector2(0, 520), Color.white, FontStyles.Bold);

        int highScore = SafePrefs.GetInt(HIGH_SCORE_KEY, 0);
        startHighScoreText = CreateText(
            startMenuPanel.transform,
            "EN İYİ SKOR: " + highScore,
            46,
            new Vector2(0, 380),
            new Color(1f, 0.84f, 0.0f),
            FontStyles.Bold
        );

        CreateText(startMenuPanel.transform, "İŞLEMLERİ SEÇİN", 32, new Vector2(0, 220), new Color(0.7f, 0.8f, 0.9f), FontStyles.Normal);
        BuildOperationButtons(startMenuPanel.transform, new Vector2(0, 110));

        speedValueText = CreateText(startMenuPanel.transform, GetSpeedLabel(gameSpeedLevel), 32, new Vector2(0, -60), new Color(0.7f, 0.8f, 0.9f), FontStyles.Normal);
        BuildSpeedSlider(startMenuPanel.transform, new Vector2(0, -140));

        GameObject tapBtn = CreateButton(
            startMenuPanel.transform,
            "TapButton",
            "BAŞLAMAK İÇİN DOKUN",
            new Vector2(0, -370),
            new Vector2(620, 130),
            new Color(0.18f, 0.8f, 0.44f, 1f)
        );
        startButton = tapBtn.GetComponent<Button>();
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnGameStart);
        }
    }

    private void BuildOperationButtons(Transform parent, Vector2 centerPos)
    {
        string[] symbols = { "+", "-", "×", "÷" };
        float spacing = 160f;
        float startX = -((symbols.Length - 1) * spacing) / 2f;

        for (int i = 0; i < symbols.Length; i++)
        {
            int index = i;
            Vector2 pos = centerPos + new Vector2(startX + (i * spacing), 0);
            GameObject btn = CreateButton(parent, "OpBtn_" + symbols[i], symbols[i], pos, new Vector2(130, 130), inactiveColor);

            opButtonImages[index] = btn.GetComponent<Image>();
            operationButtons[index] = btn.GetComponent<Button>();
            operationActions[index] = () => ToggleOperation(index);
            if (operationButtons[index] != null)
            {
                operationButtons[index].onClick.AddListener(operationActions[index]);
            }
        }

        UpdateOperationButtonVisuals();
    }

    private void BuildSpeedSlider(Transform parent, Vector2 pos)
    {
        GameObject sliderObj = new GameObject("SpeedSlider");
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRt = sliderObj.AddComponent<RectTransform>();
        sliderRt.anchoredPosition = pos;
        sliderRt.sizeDelta = new Vector2(560, 40);

        Slider slider = sliderObj.AddComponent<Slider>();

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.2f, 0.3f, 1f);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.2f);
        fillAreaRt.anchorMax = new Vector2(1, 0.8f);
        fillAreaRt.sizeDelta = new Vector2(-20, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.18f, 0.55f, 0.95f, 1f);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(55, 55);

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 1;
        slider.maxValue = 3;
        slider.wholeNumbers = true;
        slider.value = gameSpeedLevel;

        speedSlider = slider;
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
    }

    private void OnSpeedChanged(float val)
    {
        gameSpeedLevel = Mathf.Clamp(Mathf.RoundToInt(val), 1, 3);
        SafePrefs.SetInt(SPEED_KEY, gameSpeedLevel);
        SafePrefs.Save();

        if (speedValueText != null)
        {
            speedValueText.text = GetSpeedLabel(gameSpeedLevel);
        }
    }

    private string GetSpeedLabel(int level)
    {
        if (level == 1) return "OYUN HIZI: 1 (Yavaş)";
        if (level == 3) return "OYUN HIZI: 3 (Hızlı)";
        return "OYUN HIZI: 2 (Normal)";
    }

    public void ToggleOperation(int index)
    {
        int activeCount = (isAddActive ? 1 : 0) + (isSubActive ? 1 : 0) + (isMulActive ? 1 : 0) + (isDivActive ? 1 : 0);

        if (index == 0) { if (isAddActive && activeCount == 1) return; isAddActive = !isAddActive; SafePrefs.SetInt("Op_Add", isAddActive ? 1 : 0); }
        if (index == 1) { if (isSubActive && activeCount == 1) return; isSubActive = !isSubActive; SafePrefs.SetInt("Op_Sub", isSubActive ? 1 : 0); }
        if (index == 2) { if (isMulActive && activeCount == 1) return; isMulActive = !isMulActive; SafePrefs.SetInt("Op_Mul", isMulActive ? 1 : 0); }
        if (index == 3) { if (isDivActive && activeCount == 1) return; isDivActive = !isDivActive; SafePrefs.SetInt("Op_Div", isDivActive ? 1 : 0); }

        SafePrefs.Save();
        UpdateOperationButtonVisuals();
    }

    private void UpdateOperationButtonVisuals()
    {
        if (opButtonImages[0] != null) opButtonImages[0].color = isAddActive ? activeColor : inactiveColor;
        if (opButtonImages[1] != null) opButtonImages[1].color = isSubActive ? activeColor : inactiveColor;
        if (opButtonImages[2] != null) opButtonImages[2].color = isMulActive ? activeColor : inactiveColor;
        if (opButtonImages[3] != null) opButtonImages[3].color = isDivActive ? activeColor : inactiveColor;
    }

    public void CopyActiveOperations(List<int> destination)
    {
        if (destination == null)
        {
            return;
        }

        destination.Clear();
        if (isAddActive) destination.Add(0);
        if (isSubActive) destination.Add(1);
        if (isMulActive) destination.Add(2);
        if (isDivActive) destination.Add(3);
        if (destination.Count == 0) destination.Add(0);
    }

    public List<int> GetActiveOperations()
    {
        List<int> ops = new List<int>(4);
        CopyActiveOperations(ops);
        return ops;
    }

    private void BuildHUD(Transform parent)
    {
        hudPanel = CreatePanel(parent, "HUDPanel", Color.clear);

        GameObject lifeContainer = new GameObject("LifeBarsContainer");
        lifeContainer.transform.SetParent(hudPanel.transform, false);
        RectTransform containerRt = lifeContainer.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 1f);
        containerRt.anchorMax = new Vector2(0.5f, 1f);
        containerRt.pivot = new Vector2(0.5f, 1f);
        containerRt.anchoredPosition = new Vector2(0, -90);
        containerRt.sizeDelta = new Vector2(260, 20);

        GameObject bar1 = new GameObject("LifeBar_1");
        bar1.transform.SetParent(lifeContainer.transform, false);
        lifeBar1 = bar1.AddComponent<Image>();
        lifeBar1.color = lifeActiveColor;
        lifeBar1.raycastTarget = false;
        RectTransform bar1Rt = bar1.GetComponent<RectTransform>();
        bar1Rt.anchoredPosition = new Vector2(-65, 0);
        bar1Rt.sizeDelta = new Vector2(110, 16);

        GameObject bar2 = new GameObject("LifeBar_2");
        bar2.transform.SetParent(lifeContainer.transform, false);
        lifeBar2 = bar2.AddComponent<Image>();
        lifeBar2.color = lifeActiveColor;
        lifeBar2.raycastTarget = false;
        RectTransform bar2Rt = bar2.GetComponent<RectTransform>();
        bar2Rt.anchoredPosition = new Vector2(65, 0);
        bar2Rt.sizeDelta = new Vector2(110, 16);
    }

    public void UpdateLivesUI(int wrongCount, int maxWrong = 2)
    {
        if (lifeBar1 == null || lifeBar2 == null)
        {
            return;
        }

        int lostLives = Mathf.Clamp(wrongCount, 0, Mathf.Max(1, maxWrong));
        if (lostLives <= 0)
        {
            lifeBar1.color = lifeActiveColor;
            lifeBar2.color = lifeActiveColor;
        }
        else if (lostLives < maxWrong)
        {
            lifeBar1.color = lifeActiveColor;
            lifeBar2.color = lifeInactiveColor;
        }
        else
        {
            lifeBar1.color = lifeInactiveColor;
            lifeBar2.color = lifeInactiveColor;
        }
    }

    private void BuildGameOverMenu(Transform parent)
    {
        gameOverPanel = CreatePanel(parent, "GameOverPanel", new Color(0f, 0f, 0f, 0.45f));

        GameObject card = CreatePanel(gameOverPanel.transform, "Card", new Color(0.1f, 0.13f, 0.22f, 0.95f));
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(860, 980);

        GameObject content = new GameObject("Content");
        content.transform.SetParent(card.transform, false);
        gameOverContent = content.AddComponent<RectTransform>();
        gameOverContent.anchorMin = Vector2.zero;
        gameOverContent.anchorMax = Vector2.one;
        gameOverContent.offsetMin = new Vector2(48f, 48f);
        gameOverContent.offsetMax = new Vector2(-48f, -48f);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 12, 12);
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        SetLayoutHeight(CreateText(content.transform, "OYUN BİTTİ", 75, Vector2.zero, new Color(1f, 0.35f, 0.35f), FontStyles.Bold).gameObject, 110f);
        finalScoreText = CreateText(content.transform, "Skor: 0", 54, Vector2.zero, Color.white, FontStyles.Normal);
        SetLayoutHeight(finalScoreText.gameObject, 78f);
        finalHighScoreText = CreateText(content.transform, "Rekor: 0", 46, Vector2.zero, new Color(1f, 0.84f, 0.0f), FontStyles.Bold);
        SetLayoutHeight(finalHighScoreText.gameObject, 78f);

        GameObject reviveBtn = CreateButton(
            content.transform,
            "ReviveButton",
            ReviveLabelText,
            Vector2.zero,
            new Vector2(740, 124),
            new Color(0.18f, 0.75f, 0.45f, 1f)
        );
        SetLayoutHeight(reviveBtn, 124f);
        reviveButton = reviveBtn.GetComponent<Button>();
        reviveLabel = reviveBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (reviveLabel != null)
        {
            reviveLabel.enableAutoSizing = true;
            reviveLabel.fontSizeMin = 24f;
            reviveLabel.fontSizeMax = 34f;
        }
        if (reviveButton != null)
        {
            reviveButton.onClick.AddListener(OnReviveClicked);
        }

        GameObject retryBtn = CreateButton(
            content.transform,
            "RetryButton",
            "YENİDEN BAŞLA",
            Vector2.zero,
            new Vector2(740, 124),
            new Color(0.2f, 0.5f, 0.85f, 1f)
        );
        SetLayoutHeight(retryBtn, 124f);
        retryButton = retryBtn.GetComponent<Button>();
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }
    }

    private static void SetLayoutHeight(GameObject target, float height)
    {
        if (target == null)
        {
            return;
        }

        LayoutElement element = target.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = target.AddComponent<LayoutElement>();
        }

        element.minHeight = height;
        element.preferredHeight = height;
        element.flexibleHeight = 0f;
        element.flexibleWidth = 1f;
    }

    public void ShowStartMenu()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (ScoreManager.instance != null) ScoreManager.instance.SetScoreUIVisible(false);

        int currentBest = SafePrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (startHighScoreText != null)
        {
            startHighScoreText.text = "EN İYİ SKOR: " + currentBest;
        }
        UpdateOperationButtonVisuals();
    }

    public void OnGameStart()
    {
        reviveUsedThisRun = false;
        adInProgress = false;
        gameOverSequenceRunning = false;
        isGameStarted = true;
        Time.timeScale = 1f;
        RefreshReviveButton();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);

        UpdateLivesUI(0, 2);

        if (ScoreManager.instance != null) ScoreManager.instance.SetScoreUIVisible(true);

        if (GateSpawner.Instance != null)
        {
            GateSpawner.Instance.ResetAndSpawnFirstGates();
        }
    }

    public void TriggerGameOverWithSlowMotion(int score)
    {
        if (gameOverSequenceRunning)
        {
            return;
        }

        gameOverSequenceRunning = true;
        StopUiRoutines();
        gameOverRoutine = StartCoroutine(SlowDownAndShowGameOver(score));
    }

    public void ShowGameOver(int score)
    {
        TriggerGameOverWithSlowMotion(score);
    }

    private IEnumerator SlowDownAndShowGameOver(int score)
    {
        isGameStarted = false;

        float duration = 0.8f;
        float elapsed = 0f;
        float startScale = Time.timeScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(startScale, 0f, elapsed / duration);
            yield return null;
        }

        Time.timeScale = 0f;

        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (ScoreManager.instance != null) ScoreManager.instance.SetScoreUIVisible(false);

        int currentBest = SafePrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (score > currentBest)
        {
            currentBest = score;
            SafePrefs.SetInt(HIGH_SCORE_KEY, currentBest);
            SafePrefs.Save();
        }

        if (finalScoreText != null) finalScoreText.text = "Skor: " + score;
        if (finalHighScoreText != null) finalHighScoreText.text = "Rekor: " + currentBest;

        RefreshReviveButton();
        if (gameOverContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameOverContent);
        }

        if (gameOverPanel != null)
        {
            Transform card = gameOverPanel.transform.Find("Card");
            if (card != null)
            {
                popRoutine = StartCoroutine(PopAnimation(card));
            }
        }

        gameOverRoutine = null;
    }

    public void OnReviveClicked()
    {
        if (reviveUsedThisRun || adInProgress)
        {
            return;
        }

        adInProgress = true;
        if (reviveButton != null)
        {
            reviveButton.interactable = false;
        }
        if (reviveLabel != null)
        {
            reviveLabel.text = "REKLAM YÜKLENİYOR...";
        }

        AdManager ads = AdManager.Instance != null ? AdManager.Instance : SceneQuery.FindFirst<AdManager>();
        if (ads == null)
        {
            StartCoroutine(FallbackMockRevive());
            return;
        }

        ads.ShowRewardedAd(OnRewardedAdFinished);
    }

    private IEnumerator FallbackMockRevive()
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        OnRewardedAdFinished(true);
    }

    private void OnRewardedAdFinished(bool earned)
    {
        if (this == null)
        {
            return;
        }

        adInProgress = false;
        if (!earned)
        {
            if (reviveLabel != null)
            {
                reviveLabel.text = ReviveLabelText;
            }
            RefreshReviveButton();
            return;
        }

        CompleteRevive();
    }

    private void CompleteRevive()
    {
        reviveUsedThisRun = true;
        adInProgress = false;
        gameOverSequenceRunning = false;
        isGameStarted = true;
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        if (ScoreManager.instance != null) ScoreManager.instance.SetScoreUIVisible(true);

        NumberPlayer player = SceneQuery.FindFirst<NumberPlayer>();
        if (player != null)
        {
            player.Revive();
        }

        RefreshReviveButton();
    }

    private void RefreshReviveButton()
    {
        bool available = !reviveUsedThisRun;
        if (reviveButton != null)
        {
            reviveButton.gameObject.SetActive(available);
            reviveButton.interactable = available && !adInProgress;
        }

        if (reviveLabel != null && available && !adInProgress)
        {
            reviveLabel.text = ReviveLabelText;
        }

        if (gameOverContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameOverContent);
        }
    }

    private void OnRetryClicked()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex < 0)
        {
            ReportSceneFailure(activeScene.name);
            return;
        }

        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadScene(activeScene.buildIndex);
        }
        catch (System.Exception exception)
        {
            Time.timeScale = 0f;
            ReportSceneFailure(exception.Message);
        }
    }

    private IEnumerator PopAnimation(Transform target)
    {
        if (target == null)
        {
            yield break;
        }

        target.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1f)
        {
            if (target == null)
            {
                popRoutine = null;
                yield break;
            }

            t += Time.unscaledDeltaTime * 4.5f;
            target.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        if (target != null)
        {
            target.localScale = Vector3.one;
        }

        popRoutine = null;
    }

    private void StopUiRoutines()
    {
        if (gameOverRoutine != null)
        {
            StopCoroutine(gameOverRoutine);
            gameOverRoutine = null;
        }

        if (popRoutine != null)
        {
            StopCoroutine(popRoutine);
            popRoutine = null;
        }
    }

    private void ReleaseUiHooks()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnGameStart);
        }

        for (int i = 0; i < operationButtons.Length; i++)
        {
            if (operationButtons[i] != null && operationActions[i] != null)
            {
                operationButtons[i].onClick.RemoveListener(operationActions[i]);
                operationActions[i] = null;
            }
        }

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveListener(OnSpeedChanged);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryClicked);
        }

        if (reviveButton != null)
        {
            reviveButton.onClick.RemoveListener(OnReviveClicked);
        }
    }

    private static void ReportSceneFailure(string detail)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError("Scene could not be reloaded: " + detail);
#endif
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return panel;
    }

    private TextMeshProUGUI CreateText(Transform parent, string content, float size, Vector2 anchoredPos, Color color, FontStyles style)
    {
        GameObject txtObj = new GameObject("Text_" + content);
        txtObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(850, 160);
        return tmp;
    }

    private GameObject CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color btnColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        GameObject txtObj = new GameObject("BtnText");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 38;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        return btnObj;
    }
}
