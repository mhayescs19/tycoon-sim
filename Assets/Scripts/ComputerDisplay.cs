using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ComputerDisplay : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TypingInput typingInput;
    [SerializeField] private AudioClip karpathyPodcast;

    private string _corpus;
    private int _charIndex;
    private string _buffer = "";
    private int _lineNumber = 1;
    private bool _isTypingMode;
    private bool _dashboardBuilt;
    private AudioSource _vibePodcastSource;
    private float _vibePodcastTimestamp;

    private GameObject _dashboardRoot;
    private readonly List<Button> _cardButtons = new List<Button>();
    private Button _defaultButton;
    private GameObject _purchasePopupRoot;
    private TextMeshProUGUI _purchasePopupText;
    private Button _purchaseYesButton;
    private Button _purchaseBackButton;
    private string _pendingHustleName;
    private int _pendingHustlePrice;

    private BugBountyManager _bugBountyManager;
    private GameObject _bugBountyPanelRoot;
    private TextMeshProUGUI _activeBountiesText;
    private TextMeshProUGUI _successfulBountiesText;
    private TextMeshProUGUI _failedBountiesText;
    private TextMeshProUGUI _spawnInfoText;
    private Button _bugBountyBackButton;
    private TextMeshProUGUI _bugBountyCardLabel;
    private ScrollRect _activeBountiesScroll;
    private ScrollRect _successfulBountiesScroll;
    private ScrollRect _failedBountiesScroll;
    private float _bugBountyCardRefreshTimer;
    private int _ignoreEscapeCloseUntilFrame = -1;

    private OpenclawManager _openclawManager;
    private GameObject _openclawPanelRoot;
    private TextMeshProUGUI _openclawStatusText;

    private GameObject _airbedPanelRoot;
    private TextMeshProUGUI _airbedStatusText;
    private Button _airbedBuyButton;
    private TextMeshProUGUI _airbedBuyButtonLabel;
    private TextMeshProUGUI _airbedCardLabel;

    void Awake()
    {
        TextAsset asset = Resources.Load<TextAsset>("GStackSourceCode");
        if (asset != null)
        {
            _corpus = asset.text;
        }
        else
        {
            _corpus = "// loading...\n";
            Debug.LogError("GStackSourceCode.txt not found in Resources/");
        }

        if (typingInput == null)
            typingInput = FindFirstObjectByType<TypingInput>();

        EnsureVibePodcastSource();

        _bugBountyManager = FindFirstObjectByType<BugBountyManager>();
        if (_bugBountyManager == null)
        {
            var managerObj = new GameObject("BugBountyManager");
            _bugBountyManager = managerObj.AddComponent<BugBountyManager>();
        }

        _openclawManager = FindFirstObjectByType<OpenclawManager>();
        if (_openclawManager == null)
        {
            Debug.LogError("FATAL: OpenclawManager not found in scene. Please add it to a persistent GameObject.");
        }

        _bugBountyManager.OnBountyDataChanged += RefreshBugBountyPanel;
        _bugBountyManager.OnBountySpawned += OnBountySpawned;

        _openclawManager.OnStateChanged += RefreshOpenClawPanel;

        if (AirbedManager.Instance == null)
        {
            var airbedObj = new GameObject("AirbedManager");
            airbedObj.AddComponent<AirbedManager>();
            airbedObj.AddComponent<AirbedDisplay>();
        }
        AirbedManager.Instance.OnStateChanged += RefreshAirbedPanel;
        AirbedManager.Instance.OnStateChanged += RefreshAirbedCardSubtitle;

        panel.SetActive(false);
    }

    void OnDestroy()
    {
        PauseVibePodcast();

        if (_bugBountyManager != null)
        {
            _bugBountyManager.OnBountyDataChanged -= RefreshBugBountyPanel;
            _bugBountyManager.OnBountySpawned -= OnBountySpawned;
        }
        if (_openclawManager != null)
        {
            _openclawManager.OnStateChanged -= RefreshOpenClawPanel;
        }
        if (AirbedManager.Instance != null)
        {
            AirbedManager.Instance.OnStateChanged -= RefreshAirbedPanel;
            AirbedManager.Instance.OnStateChanged -= RefreshAirbedCardSubtitle;
        }
    }

    void Update()
    {
        if (panel.activeSelf && _dashboardRoot != null && _dashboardRoot.activeSelf)
        {
            _bugBountyCardRefreshTimer += Time.deltaTime;
            if (_bugBountyCardRefreshTimer >= 0.25f)
            {
                _bugBountyCardRefreshTimer = 0f;
                RefreshBugBountyCardSubtitle();
            }
        }

        if (Keyboard.current == null || !panel.activeSelf) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (Time.frameCount <= _ignoreEscapeCloseUntilFrame) return;

        bool popupActive = _purchasePopupRoot != null && _purchasePopupRoot.activeSelf;
        bool bugBountyActive = _bugBountyPanelRoot != null && _bugBountyPanelRoot.activeSelf;
        bool airbedActive = _airbedPanelRoot != null && _airbedPanelRoot.activeSelf;
        bool openclawActive = _openclawPanelRoot != null && _openclawPanelRoot.activeSelf;

        if (popupActive || bugBountyActive || airbedActive || openclawActive || _isTypingMode)
        {
            ShowDashboard();
        }
        else if (_dashboardRoot != null && _dashboardRoot.activeSelf)
        {
            Deactivate();
        }
    }

    public int TypeCharacters(int count)
    {
        if (!_isTypingMode) return 0;

        int newlines = 0;
        for (int i = 0; i < count; i++)
        {
            char c = _corpus[_charIndex];
            _buffer += c;
            if (c == '\n')
            {
                newlines++;
                _lineNumber++;
                _buffer += $"{_lineNumber,4} ";
            }
            _charIndex = (_charIndex + 1) % _corpus.Length;
        }

        codeText.text = _buffer;
        StartCoroutine(ScrollToBottom());
        return newlines;
    }

    public void Activate()
    {
        EnsureDashboardBuilt();
        PauseVibePodcast();
        SetTypingEnabled(false);
        _isTypingMode = false;
        _dashboardRoot.SetActive(true);
        if (_bugBountyPanelRoot != null) _bugBountyPanelRoot.SetActive(false);
        scrollRect.gameObject.SetActive(false);
        panel.SetActive(true);
        FocusDefaultCard();
    }

    public void Deactivate()
    {
        PauseVibePodcast();
        SetTypingEnabled(false);
        _isTypingMode = false;
        panel.SetActive(false);
    }

    public void ShowDashboard()
    {
        if (!panel.activeSelf) return;
        EnsureDashboardBuilt();
        PauseVibePodcast();
        SetTypingEnabled(false);
        _isTypingMode = false;
        _dashboardRoot.SetActive(true);
        if (_bugBountyPanelRoot != null) _bugBountyPanelRoot.SetActive(false);
        if (_airbedPanelRoot != null) _airbedPanelRoot.SetActive(false);
        if (_openclawPanelRoot != null) _openclawPanelRoot.SetActive(false);
        if (_purchasePopupRoot != null) _purchasePopupRoot.SetActive(false);
        scrollRect.gameObject.SetActive(false);
        // TypingInput also uses Esc to return here; ignore close-on-Esc for this frame.
        _ignoreEscapeCloseUntilFrame = Time.frameCount + 1;
        FocusDefaultCard();
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private IEnumerator ScrollToTop()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void EnterVibeCodeMode()
    {
        _buffer = $"{_lineNumber,4} ";
        codeText.text = _buffer;
        _isTypingMode = true;
        _dashboardRoot.SetActive(false);
        if (_bugBountyPanelRoot != null) _bugBountyPanelRoot.SetActive(false);
        scrollRect.gameObject.SetActive(true);
        SetTypingEnabled(true);
        PlayVibePodcast();
        StartCoroutine(ScrollToTop());
    }

    private void EnsureVibePodcastSource()
    {
        if (_vibePodcastSource != null) return;

        _vibePodcastSource = gameObject.AddComponent<AudioSource>();
        _vibePodcastSource.playOnAwake = false;
        _vibePodcastSource.loop = false;
    }

    private void PlayVibePodcast()
    {
        if (karpathyPodcast == null) return;

        EnsureVibePodcastSource();
        _vibePodcastSource.clip = karpathyPodcast;

        if (_vibePodcastTimestamp >= karpathyPodcast.length - 0.05f)
            _vibePodcastTimestamp = 0f;

        _vibePodcastSource.time = Mathf.Clamp(_vibePodcastTimestamp, 0f, Mathf.Max(0f, karpathyPodcast.length - 0.01f));
        _vibePodcastSource.Play();
    }

    private void PauseVibePodcast()
    {
        if (_vibePodcastSource == null || !_vibePodcastSource.isPlaying) return;

        _vibePodcastTimestamp = _vibePodcastSource.time;
        _vibePodcastSource.Pause();
    }

    private void OnPlaceholderCardPressed(string name)
    {
        Debug.Log($"[ComputerDisplay] {name} is dashboard-only in this phase.");
        FocusDefaultCard();
    }

    private void EnsureDashboardBuilt()
    {
        if (_dashboardBuilt) return;

        _dashboardRoot = new GameObject("DashboardRoot", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        _dashboardRoot.transform.SetParent(panel.transform, false);

        var rootRect = (RectTransform)_dashboardRoot.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(620f, 420f);

        var rootImage = _dashboardRoot.GetComponent<Image>();
        rootImage.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);

        var layout = _dashboardRoot.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var fitter = _dashboardRoot.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        CreateTitle();
        CreateCard("Vibe Code", "Launch coding interface", false, EnterVibeCodeMode);
        CreateCard("Bug Bounty", "View active, successful, and failed bounties", false, ShowBugBountyPanel);
        CreateCard("OpenClaw", "Purchase Agents", false, ShowOpenClawPanel);
        CreateCard("Airbed", "Sublet floor space for passive income", false, ShowAirbedPanel);
        ConfigureNavigation();
        CreatePurchasePopup();
        CreateBugBountyPanel();
        CreateAirbedPanel();
        CreateOpenClawPanel();

        scrollRect.gameObject.SetActive(false);
        _dashboardBuilt = true;
    }

    private void CreateTitle()
    {
        var titleObj = new GameObject("DashboardTitle", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        titleObj.transform.SetParent(_dashboardRoot.transform, false);

        var titleLayout = titleObj.GetComponent<LayoutElement>();
        titleLayout.preferredHeight = 44f;

        var title = titleObj.GetComponent<TextMeshProUGUI>();
        title.text = "Computer Dashboard";
        title.font = codeText.font;
        title.fontSize = 30;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
    }

    private void CreateCard(string title, string subtitle, bool isLocked, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObj = new GameObject($"{title}Card", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObj.transform.SetParent(_dashboardRoot.transform, false);

        var layout = buttonObj.GetComponent<LayoutElement>();
        layout.preferredHeight = 74f;

        var image = buttonObj.GetComponent<Image>();
        image.color = isLocked ? new Color(0.13f, 0.13f, 0.13f, 1f) : new Color(0.18f, 0.24f, 0.34f, 1f);

        var button = buttonObj.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = isLocked ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(0.25f, 0.34f, 0.5f, 1f);
        colors.selectedColor = new Color(0.48f, 0.42f, 0.18f, 1f);
        colors.pressedColor = new Color(0.52f, 0.52f, 0.52f, 1f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        var labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(buttonObj.transform, false);

        var labelRect = (RectTransform)labelObj.transform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(16f, 10f);
        labelRect.offsetMax = new Vector2(-16f, -10f);

        var label = labelObj.GetComponent<TextMeshProUGUI>();
        label.font = codeText.font;
        label.alignment = TextAlignmentOptions.Left;
        label.enableWordWrapping = true;
        label.fontSize = 20;
        label.text = $"{title}\n<size=65%>{subtitle}</size>";
        label.color = isLocked ? new Color(0.84f, 0.84f, 0.84f, 1f) : Color.white;

        if (isLocked)
            CreateLockedBadge(buttonObj.transform);

        if (title == "Bug Bounty")
        {
            _bugBountyCardLabel = label;
            RefreshBugBountyCardSubtitle();
        }

        if (title == "Airbed")
        {
            _airbedCardLabel = label;
            RefreshAirbedCardSubtitle();
        }

        _cardButtons.Add(button);
        if (_defaultButton == null) _defaultButton = button;
    }

    private void CreateLockedBadge(Transform parent)
    {
        var badgeObj = new GameObject("LockedBadge", typeof(RectTransform), typeof(Image));
        badgeObj.transform.SetParent(parent, false);

        var badgeRect = (RectTransform)badgeObj.transform;
        badgeRect.anchorMin = new Vector2(1f, 1f);
        badgeRect.anchorMax = new Vector2(1f, 1f);
        badgeRect.pivot = new Vector2(1f, 1f);
        badgeRect.anchoredPosition = new Vector2(-12f, -10f);
        badgeRect.sizeDelta = new Vector2(96f, 24f);

        var badgeImage = badgeObj.GetComponent<Image>();
        badgeImage.color = new Color(0.42f, 0.2f, 0.2f, 1f);

        var badgeTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        badgeTextObj.transform.SetParent(badgeObj.transform, false);

        var badgeTextRect = (RectTransform)badgeTextObj.transform;
        badgeTextRect.anchorMin = new Vector2(0f, 0f);
        badgeTextRect.anchorMax = new Vector2(1f, 1f);
        badgeTextRect.offsetMin = Vector2.zero;
        badgeTextRect.offsetMax = Vector2.zero;

        var badgeText = badgeTextObj.GetComponent<TextMeshProUGUI>();
        badgeText.font = codeText.font;
        badgeText.fontSize = 14;
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.text = "LOCKED";
        badgeText.color = Color.white;
    }

    private void CreatePurchasePopup()
    {
        _purchasePopupRoot = new GameObject("PurchasePopup", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        _purchasePopupRoot.transform.SetParent(panel.transform, false);

        var popupRect = (RectTransform)_purchasePopupRoot.transform;
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.sizeDelta = new Vector2(420f, 220f);

        var popupImage = _purchasePopupRoot.GetComponent<Image>();
        popupImage.color = new Color(0.05f, 0.05f, 0.05f, 0.98f);

        var popupLayout = _purchasePopupRoot.GetComponent<VerticalLayoutGroup>();
        popupLayout.padding = new RectOffset(20, 20, 20, 20);
        popupLayout.spacing = 12;
        popupLayout.childAlignment = TextAnchor.UpperCenter;
        popupLayout.childControlHeight = true;
        popupLayout.childControlWidth = true;
        popupLayout.childForceExpandHeight = false;
        popupLayout.childForceExpandWidth = true;

        var textObj = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObj.transform.SetParent(_purchasePopupRoot.transform, false);
        var textLayout = textObj.GetComponent<LayoutElement>();
        textLayout.preferredHeight = 92f;

        _purchasePopupText = textObj.GetComponent<TextMeshProUGUI>();
        _purchasePopupText.font = codeText.font;
        _purchasePopupText.fontSize = 23;
        _purchasePopupText.alignment = TextAlignmentOptions.Center;
        _purchasePopupText.color = Color.white;
        _purchasePopupText.enableWordWrapping = true;

        _purchaseYesButton = CreatePopupButton(_purchasePopupRoot.transform, "YesButton", "Yes", OnPurchaseYesPressed);
        _purchaseBackButton = CreatePopupButton(_purchasePopupRoot.transform, "BackButton", "Back", OnPurchaseBackPressed);

        Navigation yesNav = new Navigation { mode = Navigation.Mode.Explicit };
        yesNav.selectOnUp = _purchaseBackButton;
        yesNav.selectOnDown = _purchaseBackButton;
        yesNav.selectOnLeft = _purchaseYesButton;
        yesNav.selectOnRight = _purchaseYesButton;
        _purchaseYesButton.navigation = yesNav;

        Navigation backNav = new Navigation { mode = Navigation.Mode.Explicit };
        backNav.selectOnUp = _purchaseYesButton;
        backNav.selectOnDown = _purchaseYesButton;
        backNav.selectOnLeft = _purchaseBackButton;
        backNav.selectOnRight = _purchaseBackButton;
        _purchaseBackButton.navigation = backNav;

        _purchasePopupRoot.SetActive(false);
    }

    private Button CreatePopupButton(Transform parent, string name, string text, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObj.transform.SetParent(parent, false);

        var layout = buttonObj.GetComponent<LayoutElement>();
        layout.preferredHeight = 38f;

        var image = buttonObj.GetComponent<Image>();
        image.color = new Color(0.22f, 0.29f, 0.44f, 1f);

        var button = buttonObj.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.onClick.AddListener(onClick);
        var colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.31f, 0.42f, 0.61f, 1f);
        colors.selectedColor = image.color; // Removes the sticky green highlight
        colors.pressedColor = new Color(0.18f, 0.24f, 0.35f, 1f);
        button.colors = colors;

        var labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(buttonObj.transform, false);
        var labelRect = (RectTransform)labelObj.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var label = labelObj.GetComponent<TextMeshProUGUI>();
        label.font = codeText.font;
        label.fontSize = 20;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.text = text;

        return button;
    }

    private void ShowPurchasePopup(string hustleName, int price)
    {
        _pendingHustleName = hustleName;
        _pendingHustlePrice = price;
        _purchasePopupText.text = $"Do you want to purchase {hustleName} for ${price}?";
        _purchasePopupRoot.SetActive(true);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_purchaseYesButton.gameObject);
    }

    private void OnPurchaseYesPressed()
    {
        // Future implementation: deduct money and unlock/purchase selected hustle.
        Debug.Log($"[ComputerDisplay] TODO purchase flow for {_pendingHustleName} (${_pendingHustlePrice}).");
        _purchasePopupRoot.SetActive(false);
        FocusDefaultCard();
    }

    private void OnPurchaseBackPressed()
    {
        _purchasePopupRoot.SetActive(false);
        FocusDefaultCard();
    }

    private void ShowBugBountyPanel()
    {
        _dashboardRoot.SetActive(false);
        scrollRect.gameObject.SetActive(false);
        if (_purchasePopupRoot != null) _purchasePopupRoot.SetActive(false);
        _bugBountyPanelRoot.SetActive(true);
        RefreshBugBountyPanel();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_bugBountyBackButton.gameObject);
    }

    private void CreateBugBountyPanel()
    {
        _bugBountyPanelRoot = new GameObject("BugBountyPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        _bugBountyPanelRoot.transform.SetParent(panel.transform, false);

        var rootRect = (RectTransform)_bugBountyPanelRoot.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(720f, 600f);

        var image = _bugBountyPanelRoot.GetComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        var layout = _bugBountyPanelRoot.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 18, 18);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;

        var title = CreatePanelText(_bugBountyPanelRoot.transform, "Bug Bounties", 30, 42f, TextAlignmentOptions.Left);
        title.color = Color.white;

        _spawnInfoText = CreatePanelText(_bugBountyPanelRoot.transform, "Spawning every 15-45 seconds", 18, 26f, TextAlignmentOptions.Left);
        _spawnInfoText.color = new Color(0.75f, 0.92f, 0.75f, 1f);

        const float bountyCardViewportHeight = 118f;
        _activeBountiesText = CreateBountySectionScrollCard(_bugBountyPanelRoot.transform, bountyCardViewportHeight, out _activeBountiesScroll);
        _successfulBountiesText = CreateBountySectionScrollCard(_bugBountyPanelRoot.transform, bountyCardViewportHeight, out _successfulBountiesScroll);
        _failedBountiesText = CreateBountySectionScrollCard(_bugBountyPanelRoot.transform, bountyCardViewportHeight, out _failedBountiesScroll);

        _bugBountyBackButton = CreatePopupButton(_bugBountyPanelRoot.transform, "BackToDashboardButton", "Back to Dashboard", ShowDashboard);

        Navigation backNav = new Navigation { mode = Navigation.Mode.Explicit };
        backNav.selectOnUp = _bugBountyBackButton;
        backNav.selectOnDown = _bugBountyBackButton;
        backNav.selectOnLeft = _bugBountyBackButton;
        backNav.selectOnRight = _bugBountyBackButton;
        _bugBountyBackButton.navigation = backNav;

        _bugBountyPanelRoot.SetActive(false);
        RefreshBugBountyPanel();
    }

    private TextMeshProUGUI CreatePanelText(Transform parent, string defaultText, int fontSize, float preferredHeight, TextAlignmentOptions alignment)
    {
        var obj = new GameObject("TextBlock", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        var layout = obj.GetComponent<LayoutElement>();
        layout.preferredHeight = preferredHeight;

        var text = obj.GetComponent<TextMeshProUGUI>();
        text.font = codeText.font;
        text.fontSize = fontSize;
        text.text = defaultText;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = true;
        return text;
    }

    private void RefreshBugBountyPanel()
    {
        if (_bugBountyPanelRoot == null || _bugBountyManager == null) return;

        var activeBuilder = new StringBuilder();
        activeBuilder.AppendLine("ACTIVE BOUNTIES (claim on wall notes):");
        if (_bugBountyManager.ActiveBounties.Count == 0)
        {
            activeBuilder.AppendLine("- No active notes yet.");
        }
        else
        {
            foreach (var bounty in _bugBountyManager.ActiveBounties)
            {
                if (bounty == null)
                {
                    activeBuilder.AppendLine("- Unknown bounty ($0) | Tier ?");
                    continue;
                }

                string title = "Unknown bounty";
                if (bounty != null && bounty.Definition != null && !string.IsNullOrWhiteSpace(bounty.Definition.Title))
                    title = bounty.Definition.Title;
                int reward = (bounty != null && bounty.Definition != null) ? bounty.Definition.Reward : 0;
                activeBuilder.AppendLine($"- {title} (${reward}) | Tier {bounty.DifficultyTier}");
            }
        }
        _activeBountiesText.text = activeBuilder.ToString();

        var successBuilder = new StringBuilder();
        successBuilder.AppendLine($"SUCCESSFUL BOUNTIES (Total Earned: ${_bugBountyManager.TotalEarned:F0})");
        if (_bugBountyManager.SuccessfulBounties.Count == 0)
        {
            successBuilder.AppendLine("- None yet.");
        }
        else
        {
            int shown = Mathf.Min(5, _bugBountyManager.SuccessfulBounties.Count);
            for (int i = 0; i < shown; i++)
            {
                var item = _bugBountyManager.SuccessfulBounties[i];
                successBuilder.AppendLine($"- {item.Title}: +${item.Amount} ({item.Timestamp})");
            }
        }
        _successfulBountiesText.text = successBuilder.ToString();

        var failedBuilder = new StringBuilder();
        failedBuilder.AppendLine($"FAILED BOUNTIES (Total Penalties: ${_bugBountyManager.TotalPenalties:F0})");
        if (_bugBountyManager.FailedBounties.Count == 0)
        {
            failedBuilder.AppendLine("- None yet.");
        }
        else
        {
            int shown = Mathf.Min(5, _bugBountyManager.FailedBounties.Count);
            for (int i = 0; i < shown; i++)
            {
                var item = _bugBountyManager.FailedBounties[i];
                failedBuilder.AppendLine($"- {item.Title}: -${item.Amount} ({item.Timestamp})");
            }
        }
        _failedBountiesText.text = failedBuilder.ToString();

        _spawnInfoText.text = $"Next bounty in ~{_bugBountyManager.SpawnTimerRemaining:F0}s (interval rolls 15-45s)";

        RebuildBountyScrollLayout(_activeBountiesScroll);
        RebuildBountyScrollLayout(_successfulBountiesScroll);
        RebuildBountyScrollLayout(_failedBountiesScroll);
    }

    private static void RebuildBountyScrollLayout(ScrollRect scroll)
    {
        if (scroll == null || scroll.content == null || scroll.viewport == null) return;

        var content = scroll.content;
        var tmp = content.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null) return;

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        float padH = vlg != null ? vlg.padding.horizontal : 0f;
        float vpW = ((RectTransform)scroll.viewport).rect.width;
        // First layout frame viewport can be 0; use panel-ish width so wrapping height is sane.
        float innerWidth = Mathf.Max(120f, vpW > 1f ? vpW - padH : 640f - padH);

        tmp.ForceMeshUpdate(true);
        float textHeight = tmp.GetPreferredValues(innerWidth, 0).y;
        // Prefer rendered bounds when they exceed preferred (fixes short content + clipped last line).
        float boundsH = tmp.textBounds.size.y;
        if (boundsH > 0.5f)
            textHeight = Mathf.Max(textHeight, boundsH + Mathf.Max(0f, -tmp.textBounds.min.y));
        textHeight += 10f;

        var textLe = tmp.GetComponent<LayoutElement>();
        if (textLe != null)
            textLe.preferredHeight = textHeight;

        float top = vlg != null ? vlg.padding.top : 0f;
        float bottom = vlg != null ? vlg.padding.bottom : 0f;
        float contentH = top + textHeight + bottom;

        var contentRt = (RectTransform)content;
        contentRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentH);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
        contentRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentH);
        scroll.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// Card frame + scroll viewport so long bounty lists never overlap other sections.
    /// </summary>
    private TextMeshProUGUI CreateBountySectionScrollCard(Transform parent, float viewportHeight, out ScrollRect scrollRectOut)
    {
        scrollRectOut = null;

        var card = new GameObject("BountySectionCard", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        card.transform.SetParent(parent, false);

        var cardLe = card.GetComponent<LayoutElement>();
        cardLe.preferredHeight = viewportHeight + 16f;

        var cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.11f, 0.11f, 0.11f, 1f);

        var cardLayout = card.GetComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(10, 10, 8, 8);
        cardLayout.childAlignment = TextAnchor.UpperLeft;
        cardLayout.childControlHeight = true;
        cardLayout.childControlWidth = true;
        cardLayout.childForceExpandHeight = true;
        cardLayout.childForceExpandWidth = true;

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGo.transform.SetParent(card.transform, false);

        var scrollLe = scrollGo.AddComponent<LayoutElement>();
        scrollLe.preferredHeight = viewportHeight;
        scrollLe.minHeight = viewportHeight;
        scrollLe.flexibleHeight = 0f;

        var scrollBg = scrollGo.GetComponent<Image>();
        scrollBg.color = new Color(0.06f, 0.06f, 0.06f, 1f);
        scrollBg.raycastTarget = true;

        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;
        scrollRect.inertia = true;
        scrollRectOut = scrollRect;

        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var vpImg = viewportGo.GetComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.02f);
        vpImg.raycastTarget = true;

        var vpRt = viewportGo.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        vpRt.anchoredPosition = Vector2.zero;

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        contentGo.transform.SetParent(viewportGo.transform, false);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);

        var contentVlg = contentGo.GetComponent<VerticalLayoutGroup>();
        contentVlg.padding = new RectOffset(6, 6, 2, 10);
        contentVlg.childAlignment = TextAnchor.UpperLeft;
        contentVlg.childControlHeight = true;
        contentVlg.childControlWidth = true;
        contentVlg.childForceExpandWidth = true;
        contentVlg.childForceExpandHeight = false;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textGo.transform.SetParent(contentGo.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0f, 1f);
        textRt.anchorMax = new Vector2(1f, 1f);
        textRt.pivot = new Vector2(0.5f, 1f);
        textRt.sizeDelta = new Vector2(0f, 0f);
        textRt.anchoredPosition = Vector2.zero;

        var textLe = textGo.GetComponent<LayoutElement>();
        textLe.flexibleWidth = 1f;
        textLe.minHeight = 2f;

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.font = codeText.font;
        tmp.fontSize = 18;
        tmp.text = "";
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        scrollRect.viewport = vpRt;
        scrollRect.content = contentRt;

        return tmp;
    }

    private void OnBountySpawned(string message)
    {
        RefreshBugBountyCardSubtitle();
        RefreshBugBountyPanel();
        Debug.Log($"[ComputerDisplay] {message}");
    }

    private void RefreshBugBountyCardSubtitle()
    {
        if (_bugBountyCardLabel == null || _bugBountyManager == null) return;

        _bugBountyCardLabel.text =
            $"Bug Bounty\n<size=65%>Active: {_bugBountyManager.ActiveBounties.Count} | Next: {_bugBountyManager.SpawnTimerRemaining:F0}s</size>";
    }

    private void ConfigureNavigation()
    {
        for (int i = 0; i < _cardButtons.Count; i++)
        {
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = _cardButtons[(i - 1 + _cardButtons.Count) % _cardButtons.Count];
            nav.selectOnDown = _cardButtons[(i + 1) % _cardButtons.Count];
            nav.selectOnLeft = _cardButtons[i];
            nav.selectOnRight = _cardButtons[i];
            _cardButtons[i].navigation = nav;
        }
    }

    private void FocusDefaultCard()
    {
        if (_defaultButton == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(_defaultButton.gameObject);
    }

    private void SetTypingEnabled(bool enabled)
    {
        if (typingInput != null)
            typingInput.IsActive = enabled;
    }

    private void CreateOpenClawPanel()
    {
        _openclawPanelRoot = new GameObject("OpenclawBountyPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        _openclawPanelRoot.transform.SetParent(panel.transform, false);

        var rootRect = (RectTransform)_openclawPanelRoot.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(860f, 660f);

        var rootImage = _openclawPanelRoot.GetComponent<Image>();
        rootImage.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        var rootLayout = _openclawPanelRoot.GetComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(24, 24, 24, 24);
        rootLayout.spacing = 10f;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = false;
        rootLayout.childAlignment = TextAnchor.UpperLeft;

        var title = CreatePanelText(_openclawPanelRoot.transform, "Openclaw Agents", 30, 42f, TextAlignmentOptions.Left);
        title.color = Color.white;

        _openclawStatusText = CreatePanelText(_openclawPanelRoot.transform, "", 19, 56f, TextAlignmentOptions.TopLeft);
        _openclawStatusText.color = new Color(0.88f, 0.95f, 0.88f, 1f);

        var gridRoot = new GameObject("OpenClawGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        gridRoot.transform.SetParent(_openclawPanelRoot.transform, false);
        var gridRect = (RectTransform)gridRoot.transform;
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.pivot = new Vector2(0.5f, 0.5f);

        var grid = gridRoot.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.cellSize = new Vector2(340f, 200f);
        grid.spacing = new Vector2(12f, 12f);
        grid.childAlignment = TextAnchor.MiddleCenter;

        var gridLe = gridRoot.GetComponent<LayoutElement>();
        gridLe.preferredHeight = (grid.cellSize.y * 2f) + grid.spacing.y;
        gridLe.preferredWidth = (grid.cellSize.x * 2f) + grid.spacing.x;
        gridRect.sizeDelta = new Vector2(gridLe.preferredWidth, gridLe.preferredHeight);

        for (int i = 0; i < 4; i++)
        {
            var agentPanel = new GameObject($"AgentPanel{i+1}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            agentPanel.transform.SetParent(gridRoot.transform, false);

            var agentImg = agentPanel.GetComponent<Image>();
            agentImg.color = new Color(0.13f, 0.13f, 0.13f, 1f);

            var agentLayout = agentPanel.GetComponent<VerticalLayoutGroup>();
            agentLayout.padding = new RectOffset(8, 8, 6, 6);
            agentLayout.spacing = 3f;
            agentLayout.childAlignment = TextAnchor.UpperLeft;
            agentLayout.childControlWidth = true;
            agentLayout.childControlHeight = true;
            agentLayout.childForceExpandWidth = true;
            agentLayout.childForceExpandHeight = false;

            string agentName = $"OpenClaw Agent {i + 1}";
            int localAgentIdx = i;
            var agentButton = CreatePopupButton(agentPanel.transform, $"Agent{i+1}BuyButton",
                $"Buy {agentName}",
                () => { _openclawManager.TryPurchaseAgent(localAgentIdx); });
            var agentLe = agentButton.GetComponent<LayoutElement>();
            if (agentLe != null) agentLe.preferredHeight = 44f;
            var agentLabel = agentButton.GetComponentInChildren<TextMeshProUGUI>();
            if (agentLabel != null) agentLabel.fontSize = 14;

            for (int skillIdx = 0; skillIdx < 4; skillIdx++)
            {
                int localSkillIdx = skillIdx;
                var skillButton = CreatePopupButton(agentPanel.transform, $"Agent{i+1}Skill{skillIdx+1}Button",
                    $"Skill {skillIdx + 1}",
                    () => { _openclawManager.TryPurchaseSkill(localAgentIdx, localSkillIdx); });
                var skillLe = skillButton.GetComponent<LayoutElement>();
                if (skillLe != null) skillLe.preferredHeight = 30f;
                var skillLabel = skillButton.GetComponentInChildren<TextMeshProUGUI>();
                if (skillLabel != null) skillLabel.fontSize = 11;
            }
        }

        var backButton = CreatePopupButton(_openclawPanelRoot.transform, "OpenClawBackButton", "Back to Dashboard", ShowDashboard);
        var backLe = backButton.GetComponent<LayoutElement>();
        if (backLe != null)
        {
            backLe.preferredHeight = 40f;
        }
        var backLabel = backButton.GetComponentInChildren<TextMeshProUGUI>();
        if (backLabel != null) backLabel.fontSize = 18;

        _openclawPanelRoot.SetActive(false);

    }

    private void ShowOpenClawPanel()
    {
        _dashboardRoot.SetActive(false);
        scrollRect.gameObject.SetActive(false);
        if (_purchasePopupRoot != null) _purchasePopupRoot.SetActive(false);
        if (_bugBountyPanelRoot != null) _bugBountyPanelRoot.SetActive(false);
        if (_airbedPanelRoot != null) _airbedPanelRoot.SetActive(false);
        _openclawPanelRoot.SetActive(true);
        RefreshOpenClawPanel();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void RefreshOpenClawPanel()
    {
        if (_openclawPanelRoot == null || _openclawManager == null) return;
        
        int activeAgents = 0;
        int activeSkills = 0;

        // OpenClawGrid is the third child now
        var gridRoot = _openclawPanelRoot.transform.Find("OpenClawGrid");
        if (gridRoot == null) return;

        for (int i = 0; i < 4; i++)
        {
            var agentPanel = gridRoot.Find($"AgentPanel{i+1}");
            if (agentPanel == null) continue;

            var agentButton = agentPanel.Find($"Agent{i+1}BuyButton")?.GetComponent<Button>();
            var agentLabel = agentButton?.GetComponentInChildren<TextMeshProUGUI>();
            
            bool agentPurchased = _openclawManager.Agents[i].IsPurchased;
            if (agentPurchased) activeAgents++;

            int agentCost = _openclawManager.GetAgentCost(i);

            if (agentLabel != null)
            {
                agentLabel.text = agentPurchased ? $"Agent {i + 1} (Owned)" : $"Buy Agent {i + 1} (${agentCost})";
            }
            if (agentButton != null)
            {
                agentButton.interactable = !agentPurchased;
            }

            for (int skillIdx = 0; skillIdx < 4; skillIdx++)
            {
                var skillButton = agentPanel.Find($"Agent{i+1}Skill{skillIdx+1}Button")?.GetComponent<Button>();
                var skillLabel = skillButton?.GetComponentInChildren<TextMeshProUGUI>();
                
                bool skillPurchased = _openclawManager.Agents[i].SkillsPurchased[skillIdx];
                if (skillPurchased) activeSkills++;

                int skillCost = _openclawManager.GetSkillCost(i, skillIdx);

                if (skillLabel != null)
                {
                    if (!agentPurchased)
                    {
                        skillLabel.text = $"Skill {skillIdx + 1} (Locked)";
                    }
                    else
                    {
                        skillLabel.text = skillPurchased ? $"Skill {skillIdx + 1} (Owned)" : $"Buy Skill {skillIdx + 1} (${skillCost})";
                    }
                }
                
                if (skillButton != null)
                {
                    skillButton.interactable = agentPurchased && !skillPurchased;
                }
            }
        }

        if (_openclawStatusText != null)
        {
            _openclawStatusText.text = $"Active Agents: {activeAgents} / 4\nActive Skills: {activeSkills} / 16";
        }
    }

    private void ShowAirbedPanel()
    {
        _dashboardRoot.SetActive(false);
        scrollRect.gameObject.SetActive(false);
        if (_purchasePopupRoot != null) _purchasePopupRoot.SetActive(false);
        _airbedPanelRoot.SetActive(true);
        RefreshAirbedPanel();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_airbedBuyButton.gameObject);
    }

    private void CreateAirbedPanel()
    {
        _airbedPanelRoot = new GameObject("AirbedPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        _airbedPanelRoot.transform.SetParent(panel.transform, false);

        var rootRect = (RectTransform)_airbedPanelRoot.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot     = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(560f, 360f);

        var image = _airbedPanelRoot.GetComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        var layout = _airbedPanelRoot.GetComponent<VerticalLayoutGroup>();
        layout.padding            = new RectOffset(24, 24, 24, 24);
        layout.spacing            = 6f;
        layout.childAlignment     = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth  = true;

        var title = CreatePanelText(_airbedPanelRoot.transform, "Airbed Rentals", 30, 42f, TextAlignmentOptions.Left);
        title.color = Color.white;

        _airbedStatusText = CreatePanelText(_airbedPanelRoot.transform, "", 19, 60f, TextAlignmentOptions.TopLeft);
        _airbedStatusText.color = new Color(0.88f, 0.95f, 0.88f, 1f);

        _airbedBuyButton = CreatePopupButton(_airbedPanelRoot.transform, "AirbedBuyButton", "Buy Unit", OnAirbedBuyPressed);

        var backButton = CreatePopupButton(_airbedPanelRoot.transform, "AirbedBackButton", "Back to Dashboard", ShowDashboard);

        _airbedBuyButtonLabel = _airbedBuyButton.GetComponentInChildren<TextMeshProUGUI>();

        Navigation buyNav  = new Navigation { mode = Navigation.Mode.Explicit,
            selectOnUp = backButton, selectOnDown = backButton };
        Navigation backNav = new Navigation { mode = Navigation.Mode.Explicit,
            selectOnUp = _airbedBuyButton, selectOnDown = _airbedBuyButton };
        _airbedBuyButton.navigation = buyNav;
        backButton.navigation       = backNav;

        _airbedPanelRoot.SetActive(false);
        RefreshAirbedPanel();
    }

    private void OnAirbedBuyPressed()
    {
        if (AirbedManager.Instance == null) return;
        bool bought = AirbedManager.Instance.TryPurchaseUnit();
        if (!bought)
        {
            if (_airbedStatusText != null)
                StartCoroutine(FlashText(_airbedStatusText,
                    AirbedManager.Instance.IsMaxed ? "Already at max capacity!" : "Not enough funds!"));
        }
    }

    private void RefreshAirbedPanel()
    {
        if (_airbedPanelRoot == null || AirbedManager.Instance == null) return;

        var mgr = AirbedManager.Instance;
        var sb  = new System.Text.StringBuilder();

        sb.AppendLine($"Units rented:   {mgr.UnitsOwned} / {AirbedManager.MaxUnits}");
        sb.AppendLine($"Passive income: ${mgr.DollarPerSec:F2} / sec");

        if (mgr.IsMaxed)
            sb.AppendLine("All units rented out — fully booked!");

        _airbedStatusText.text = sb.ToString();

        if (_airbedBuyButtonLabel != null)
        {
            _airbedBuyButtonLabel.text = mgr.IsMaxed
                ? "Fully Booked"
                : $"Rent Unit {mgr.UnitsOwned + 1}  (${mgr.NextUnitCost})";
        }

        if (_airbedBuyButton != null)
            _airbedBuyButton.interactable = !mgr.IsMaxed;
    }

    private void RefreshAirbedCardSubtitle()
    {
        if (_airbedCardLabel == null || AirbedManager.Instance == null) return;
        var mgr = AirbedManager.Instance;
        string sub = mgr.UnitsOwned == 0
            ? $"Sublet floor space · first unit ${mgr.NextUnitCost}"
            : $"Units: {mgr.UnitsOwned}/{AirbedManager.MaxUnits} · ${mgr.DollarPerSec:F2}/sec";
        _airbedCardLabel.text = $"Airbed\n<size=65%>{sub}</size>";
    }

    private System.Collections.IEnumerator FlashText(TextMeshProUGUI label, string message)
    {
        string original = label.text;
        label.text  = $"<color=#FF6B6B>{message}</color>";
        yield return new WaitForSeconds(1.8f);
        if (label != null) label.text = original;
        RefreshAirbedPanel();
    }
}
