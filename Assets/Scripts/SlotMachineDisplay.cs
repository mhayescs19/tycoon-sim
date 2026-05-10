using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SlotMachineDisplay : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Sprite[] slotSprites; // Assign sprites in Inspector
    [SerializeField] private TextMeshProUGUI fontSource; // Assign the same TMP as ComputerDisplay.codeText
    [SerializeField, Min(0f)] private float spinCost = 10f;
    [SerializeField, Min(0f)] private float jackpotAmount = 100f;
    [SerializeField, Min(0f)] private float twoMatchAmount = 12f;
    [SerializeField] private string noFundsMessage = "<color=red>Back to your 9-5 brokie</color>";
    [SerializeField] private string noMatchMessage = "<color=red>No match. Try again!</color>";
    [SerializeField] private string twoMatchMessage = "<color=green>Nice! Two match!</color>";
    [SerializeField] private string jackpotMessage = "<color=yellow>JACKPOT! You win!</color>";
    [SerializeField] private string unavailableMessage = "<color=red>Slot machine unavailable</color>";

    private GameObject _root;
    private TextMeshProUGUI _titleText;
    private Image[] _slotImages = new Image[3];
    private Button _spinButton;
    private TextMeshProUGUI _resultText;

    private bool _isActive;

    private enum SpinOutcome
    {
        Unavailable,
        InsufficientFunds,
        NoMatch,
        TwoMatch,
        Jackpot
    }

    void Start()
    {
        BuildUI();
        panel.SetActive(false);
    }

    void Update()
    {
        if (!_isActive) return;
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Deactivate();
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnSpinPressed();
        }
    }

    public void Activate()
    {
        if (_root == null) BuildUI();
        ResetSlots();
        _resultText.text = "";
        panel.SetActive(true);
        _isActive = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_spinButton.gameObject);
    }

    public void Deactivate()
    {
        panel.SetActive(false);
        _isActive = false;
    }

    private void BuildUI()
    {
        if (_root != null) return;

        _root = new GameObject("SlotMachineRoot", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        _root.transform.SetParent(panel.transform, false);

        var rootRect = (RectTransform)_root.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(900f, 600);

        var rootImage = _root.GetComponent<Image>();
        rootImage.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        var layout = _root.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var fitter = _root.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Title
        _titleText = CreateText(_root.transform, "Slot Machine", 64, 100f, TextAlignmentOptions.Center);

        // Slot Row
        var slotRow = new GameObject("SlotRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        slotRow.transform.SetParent(_root.transform, false);
        var slotRowLayout = slotRow.GetComponent<HorizontalLayoutGroup>();
        slotRowLayout.spacing = 16;
        slotRowLayout.childAlignment = TextAnchor.MiddleCenter;
        slotRowLayout.childControlHeight = true;
        slotRowLayout.childControlWidth = true;
        slotRowLayout.childForceExpandHeight = false;
        slotRowLayout.childForceExpandWidth = false;

        for (int i = 0; i < 3; i++)
        {
            _slotImages[i] = CreateSlotImage(slotRow.transform, 220f, 220f);
        }

        // Spin Button
        _spinButton = CreateButton(_root.transform, "Spin $10 (space) ", OnSpinPressed, 96f, 48);

        // Result Text
        _resultText = CreateText(_root.transform, "", 44, 80f, TextAlignmentOptions.Center);
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, float preferredHeight, TextAlignmentOptions alignment, bool isSlot = false)
    {
        var obj = new GameObject(isSlot ? "SlotText" : "TextBlock", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        var layout = obj.GetComponent<LayoutElement>();
        layout.preferredHeight = preferredHeight;
        if (isSlot) layout.preferredWidth = 80f;

        var tmp = obj.GetComponent<TextMeshProUGUI>();
        if (fontSource != null) tmp.font = fontSource.font;
        tmp.fontSize = fontSize;
        tmp.text = text;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private Image CreateSlotImage(Transform parent, float width = 80f, float height = 80f)
    {
        var obj = new GameObject("SlotImage", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        var layout = obj.GetComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.preferredWidth = width;

        var img = obj.GetComponent<Image>();
        img.color = Color.white;
        img.sprite = slotSprites != null && slotSprites.Length > 0 ? slotSprites[0] : null;
        img.preserveAspect = true;
        return img;
    }

    private Button CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick, float height = 48f, int fontSize = 24)
    {
        var buttonObj = new GameObject("SpinButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObj.transform.SetParent(parent, false);

        var layout = buttonObj.GetComponent<LayoutElement>();
        layout.preferredHeight = height;

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
        if (fontSource != null) label.font = fontSource.font;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.text = text;

        return button;
    }

    private void OnSpinPressed()
    {
        if (slotSprites == null || slotSprites.Length == 0 || GameManager.Instance == null)
        {
            ApplyOutcome(SpinOutcome.Unavailable);
            return;
        }

        SpinOutcome outcome = ResolveSpin();
        ApplyOutcome(outcome);
    }

    private SpinOutcome ResolveSpin()
    {
        if (GameManager.Instance.DollarBalance < spinCost)
            return SpinOutcome.InsufficientFunds;

        GameManager.Instance.SpendDollars(spinCost);

        int[] values = RollSlots();
        return EvaluateOutcome(values);
    }

    private int[] RollSlots()
    {
        int[] values = new int[_slotImages.Length];
        for (int i = 0; i < _slotImages.Length; i++)
        {
            values[i] = Random.Range(0, slotSprites.Length);
            _slotImages[i].sprite = slotSprites[values[i]];
        }

        return values;
    }

    private static SpinOutcome EvaluateOutcome(int[] values)
    {
        if (values[0] == values[1] && values[1] == values[2])
            return SpinOutcome.Jackpot;
        if (values[0] == values[1] || values[1] == values[2] || values[0] == values[2])
            return SpinOutcome.TwoMatch;
        return SpinOutcome.NoMatch;
    }

    private void ApplyOutcome(SpinOutcome outcome)
    {
        switch (outcome)
        {
            case SpinOutcome.Jackpot:
                _resultText.text = jackpotMessage;
                GameManager.Instance?.AddDollars(jackpotAmount);
                break;
            case SpinOutcome.TwoMatch:
                _resultText.text = twoMatchMessage;
                GameManager.Instance?.AddDollars(twoMatchAmount);
                break;
            case SpinOutcome.InsufficientFunds:
                _resultText.text = noFundsMessage;
                break;
            case SpinOutcome.Unavailable:
                _resultText.text = unavailableMessage;
                break;
            default:
                _resultText.text = noMatchMessage;
                break;
        }
    }

    private void ResetSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            _slotImages[i].sprite = slotSprites != null && slotSprites.Length > 0 ? slotSprites[0] : null;
        }
    }
}
