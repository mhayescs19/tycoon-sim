using UnityEngine;
using UnityEngine.InputSystem;

public class TypingInput : MonoBehaviour
{
    [SerializeField] private ComputerDisplay computerDisplay;
    [SerializeField] private ComboMultiplier comboMultiplier;
    [SerializeField] private int charsPerKeypress = 3;

    public bool IsActive { get; set; }

    void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += OnTextInput;
            Debug.Log("[TypingInput] Subscribed to keyboard");
        }
        else
        {
            Debug.LogWarning("[TypingInput] Keyboard.current is null on enable");
        }
    }

    void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    void Update()
    {
        comboMultiplier.Tick(Time.deltaTime);

        if (IsActive && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            IsActive = false;
            computerDisplay.Deactivate();
            comboMultiplier.Reset();
            GameManager.Instance.SetLOCPerSec(0f);
        }
    }

    private void OnTextInput(char c)
    {
        Debug.Log($"[TypingInput] Key pressed: '{c}' IsActive={IsActive}");
        if (!IsActive) return;
        int loc = computerDisplay.TypeCharacters(charsPerKeypress);
        if (loc > 0) GameManager.Instance.AddLOC(loc);
        comboMultiplier.RegisterKeypress();
    }
}
