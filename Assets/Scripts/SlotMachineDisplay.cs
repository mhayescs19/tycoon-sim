using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SlotMachineDisplay : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Sprite[] slotSprites; // Assign 5 sprites in Inspector
    [SerializeField] private TextMeshProUGUI fontSource; // Assign the same TMP as ComputerDisplay.codeText
    [SerializeField] private AudioClip slotSound;
    [SerializeField] private float slotStartTime = 2f;
    [SerializeField] private float slotStopTime = 6f;

    private GameObject _root;
    private TextMeshProUGUI _titleText;
    private Image[] _slotImages = new Image[3];
    private Button _spinButton;
    private TextMeshProUGUI _resultText;
    private AudioSource _slotAudioSource;
    private Coroutine _slotSoundStopRoutine;

    private bool _isActive;

    void Start()
    {
        EnsureAudioSource();
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
        float spinCost = 10f;

        // no spin if not enough moneys
        if (GameManager.Instance?.DollarBalance < spinCost) {
            _resultText.text = "<color=red>Back to your 9-5 brokie</color>";
            return;
        }
        
        GameManager.Instance?.SpendDollars(spinCost);
        PlaySlotSoundSlice();

        int[] values = new int[3];
        for (int i = 0; i < 3; i++)
        {
            values[i] = Random.Range(0, slotSprites != null ? slotSprites.Length : 0);
            _slotImages[i].sprite = slotSprites != null && slotSprites.Length > 0 ? slotSprites[values[i]] : null;
        }

        // Payouts
        float jackpotAmount = 110f;
        float twoMatchAmount = 20f;

        if (values[0] == values[1] && values[1] == values[2])
        {
            _resultText.text = "<color=yellow>JACKPOT! You win!</color>";
            GameManager.Instance?.AddDollars(jackpotAmount);
        }
        else if (values[0] == values[1] || values[1] == values[2] || values[0] == values[2])
        {
            _resultText.text = "<color=green>Nice! Two match!</color>";
            GameManager.Instance?.AddDollars(twoMatchAmount);
        }
        else
        {
            _resultText.text = "<color=red>No match. Try again!</color>";
        }
    }

    private void ResetSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            _slotImages[i].sprite = slotSprites != null && slotSprites.Length > 0 ? slotSprites[0] : null;
        }
    }

    private void EnsureAudioSource()
    {
        if (_slotAudioSource != null) return;

        _slotAudioSource = GetComponent<AudioSource>();
        if (_slotAudioSource == null)
            _slotAudioSource = gameObject.AddComponent<AudioSource>();

        _slotAudioSource.playOnAwake = false;
        _slotAudioSource.loop = false;
    }

    private void PlaySlotSoundSlice()
    {
        if (slotSound == null) return;

        EnsureAudioSource();
        if (_slotSoundStopRoutine != null)
            StopCoroutine(_slotSoundStopRoutine);

        float startTime = Mathf.Clamp(slotStartTime, 0f, Mathf.Max(0f, slotSound.length - 0.01f));
        float stopTime = Mathf.Clamp(slotStopTime, startTime, slotSound.length);
        float duration = stopTime - startTime;
        if (duration <= 0f) return;

        _slotAudioSource.Stop();
        _slotAudioSource.clip = slotSound;
        _slotAudioSource.time = startTime;
        _slotAudioSource.Play();
        _slotSoundStopRoutine = StartCoroutine(StopSlotSoundAfter(duration));
    }

    private IEnumerator StopSlotSoundAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_slotAudioSource != null)
            _slotAudioSource.Stop();

        _slotSoundStopRoutine = null;
    }
}