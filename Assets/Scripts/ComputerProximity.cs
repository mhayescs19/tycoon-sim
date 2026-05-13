using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class ComputerProximity : MonoBehaviour
{
    [SerializeField] private TypingInput typingInput;
    [SerializeField] private ComputerDisplay computerDisplay;
    [SerializeField] private ComboMultiplier comboMultiplier;

    private bool _isActive;
    private bool _playerInRange;
    private GameObject _promptRoot;
    private TextMeshProUGUI _promptText;

    void Start()
    {
        CreatePromptUi();

        if (typingInput == null) Debug.LogError("[Proximity] TypingInput not assigned!");
        if (computerDisplay == null) Debug.LogError("[Proximity] ComputerDisplay not assigned!");
        if (comboMultiplier == null) Debug.LogError("[Proximity] ComboMultiplier not assigned!");
    }

    void Update()
    {
        if (!_playerInRange || _isActive) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            Activate();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("CameraPivot")) return;
        _playerInRange = true;
        ShowPrompt();
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("CameraPivot")) return;
        _playerInRange = true;
        if (!_isActive) ShowPrompt();
    }

    private void Activate()
    {
        _isActive = true;
        typingInput.IsActive = false;
        computerDisplay.Activate();
        HidePrompt();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("CameraPivot")) return;
        _playerInRange = false;
        HidePrompt();

        if (!_isActive) return;
        _isActive = false;
        typingInput.IsActive = false;
        computerDisplay.Deactivate();
        comboMultiplier.Reset();
        GameManager.Instance.SetTypingLOCPerSec(0f);
    }

    private void CreatePromptUi()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        _promptRoot = new GameObject("ComputerClaimPrompt", typeof(RectTransform), typeof(Image));
        _promptRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = (RectTransform)_promptRoot.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = new Vector2(0f, 132f);
        rootRect.sizeDelta = new Vector2(340f, 42f);

        Image rootImage = _promptRoot.GetComponent<Image>();
        rootImage.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(_promptRoot.transform, false);

        RectTransform textRect = (RectTransform)textObj.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        _promptText = textObj.GetComponent<TextMeshProUGUI>();
        _promptText.text = "Press E to use computer";
        _promptText.fontSize = 24f;
        _promptText.alignment = TextAlignmentOptions.Center;
        _promptText.color = Color.white;

        _promptRoot.SetActive(false);
    }

    private void ShowPrompt()
    {
        if (_promptRoot == null) return;
        _promptText.text = "Press E to use computer";
        _promptRoot.SetActive(true);
    }

    private void HidePrompt()
    {
        if (_promptRoot == null) return;
        _promptRoot.SetActive(false);
    }
}
