using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class SlotMachineProximity : MonoBehaviour
{
    [SerializeField] private SlotMachineDisplay slotMachineDisplay;
    [SerializeField] private string interactionPromptText = "Press E to gamble!";

    private bool _isActive = false;
    private bool _playerInRange;
    private GameObject _promptRoot;
    private TextMeshProUGUI _promptText;
    private const string PlayerTag = "CameraPivot";

    void Start()
    {
        CreatePromptUi();
        if (slotMachineDisplay == null) Debug.LogError("[Proximity] SlotMachineDisplay not assigned!");

    }

    void Update()
    {
        if (!_playerInRange || _isActive) return;
        if (Keyboard.current == null) return;
        
        if (Keyboard.current.eKey.wasPressedThisFrame) {
            Activate();
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if (!IsPlayerCollider(other)) return;
        _playerInRange = true;
        ShowPrompt();
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsPlayerCollider(other)) return;
        _playerInRange = true;
        if (!_isActive) ShowPrompt();
    }

    private void Activate()
    {
        _isActive = true;
        slotMachineDisplay.Activate();
        HidePrompt();
    }

    void OnTriggerExit(Collider other) {
        if (!IsPlayerCollider(other)) return;
        _playerInRange = false;
        HidePrompt();

        if (!_isActive) return;
        _isActive = false;
        slotMachineDisplay.Deactivate();
    }

    private void CreatePromptUi()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        _promptRoot = new GameObject("SlotMachineClaimPrompt", typeof(RectTransform), typeof(Image));
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
        _promptText.text = interactionPromptText;
        _promptText.fontSize = 24f;
        _promptText.alignment = TextAlignmentOptions.Center;
        _promptText.color = Color.white;

        _promptRoot.SetActive(false);
    }

    private void ShowPrompt()
    {
        if (_promptRoot == null) return;
        _promptText.text = interactionPromptText;
        _promptRoot.SetActive(true);
    }

    private void HidePrompt()
    {
        if (_promptRoot == null) return;
        _promptRoot.SetActive(false);
    }

    private static bool IsPlayerCollider(Collider other)
    {
        return other.CompareTag(PlayerTag);
    }


}
