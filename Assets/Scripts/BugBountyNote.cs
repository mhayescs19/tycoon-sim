using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BugBountyNote : MonoBehaviour
{
    private BugBountyManager _manager;
    private string _bountyId;
    private bool _playerInRange;
    private static GameObject _claimPromptRoot;
    private static TextMeshProUGUI _claimPromptText;
    private static BugBountyNote _currentPromptOwner;

    public void Initialize(BugBountyManager manager, string bountyId, string title, int reward)
    {
        _manager = manager;
        _bountyId = bountyId;
        name = $"BugBountyNote_{bountyId}";

        EnsureClaimPromptUi();
    }

    void Update()
    {
        if (!_playerInRange) return;
        if (Keyboard.current == null) return;
        if (_manager == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            _manager.TryClaimBounty(_bountyId);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("CameraPivot")) return;
        _playerInRange = true;
        ShowClaimPrompt();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("CameraPivot")) return;
        _playerInRange = false;
        HideClaimPromptIfOwner();
    }

    void OnDisable()
    {
        HideClaimPromptIfOwner();
    }

    private void EnsureClaimPromptUi()
    {
        if (_claimPromptRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        _claimPromptRoot = new GameObject("BugBountyClaimPrompt", typeof(RectTransform), typeof(Image));
        _claimPromptRoot.transform.SetParent(canvas.transform, false);

        var rootRect = (RectTransform)_claimPromptRoot.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = new Vector2(0f, 82f);
        rootRect.sizeDelta = new Vector2(560f, 42f);

        var rootImage = _claimPromptRoot.GetComponent<Image>();
        rootImage.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(_claimPromptRoot.transform, false);

        var textRect = (RectTransform)textObj.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        _claimPromptText = textObj.GetComponent<TextMeshProUGUI>();
        _claimPromptText.text = "Press E to claim bug bounty";
        _claimPromptText.fontSize = 24f;
        _claimPromptText.alignment = TextAlignmentOptions.Center;
        _claimPromptText.color = Color.white;

        _claimPromptRoot.SetActive(false);
    }

    private void ShowClaimPrompt()
    {
        if (_claimPromptRoot == null) return;
        _currentPromptOwner = this;
        _claimPromptText.text = "Press E to claim bug bounty";
        _claimPromptRoot.SetActive(true);
    }

    private void HideClaimPromptIfOwner()
    {
        if (_claimPromptRoot == null) return;
        if (_currentPromptOwner != this) return;
        _currentPromptOwner = null;
        _claimPromptRoot.SetActive(false);
    }

    public static void HideClaimPromptGlobal()
    {
        if (_claimPromptRoot == null) return;
        _currentPromptOwner = null;
        _claimPromptRoot.SetActive(false);
    }
}
