using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComputerDisplay : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TypingInput typingInput;

    private string _corpus;
    private int _charIndex;
    private string _buffer = "";
    private int _lineNumber = 1;
    private bool _isTypingMode;
    private bool _dashboardBuilt;

    private GameObject _dashboardRoot;
    private readonly List<Button> _cardButtons = new List<Button>();
    private Button _defaultButton;
    private GameObject _purchasePopupRoot;
    private TextMeshProUGUI _purchasePopupText;
    private Button _purchaseYesButton;
    private Button _purchaseBackButton;
    private string _pendingHustleName;
    private int _pendingHustlePrice;

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

        panel.SetActive(false);
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
        SetTypingEnabled(false);
        _isTypingMode = false;
        _dashboardRoot.SetActive(true);
        scrollRect.gameObject.SetActive(false);
        panel.SetActive(true);
        FocusDefaultCard();
    }

    public void Deactivate()
    {
        SetTypingEnabled(false);
        _isTypingMode = false;
        panel.SetActive(false);
    }

    public void ShowDashboard()
    {
        if (!panel.activeSelf) return;
        EnsureDashboardBuilt();
        SetTypingEnabled(false);
        _isTypingMode = false;
        _dashboardRoot.SetActive(true);
        scrollRect.gameObject.SetActive(false);
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
        scrollRect.gameObject.SetActive(true);
        SetTypingEnabled(true);
        StartCoroutine(ScrollToTop());
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
        CreateCard("Bug Bounty", "Active (placeholder in this phase)", false, () => OnPlaceholderCardPressed("Bug Bounty"));
        CreateCard("OpenClaw", "Purchase Hustle - $200", true, () => ShowPurchasePopup("OpenClaw", 200));
        CreateCard("Airbed", "Purchase Hustle - $100", true, () => ShowPurchasePopup("Airbed", 100));
        ConfigureNavigation();
        CreatePurchasePopup();

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
        colors.selectedColor = new Color(0.48f, 0.42f, 0.18f, 1f);
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
}
