using UnityEngine;

public class ComputerProximity : MonoBehaviour
{
    [SerializeField] private TypingInput typingInput;
    [SerializeField] private ComputerDisplay computerDisplay;
    [SerializeField] private ComboMultiplier comboMultiplier;

    private bool _isActive = false;

    void Start()
    {
        var col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError("[Proximity] No Collider found on Desk!");
        else
            Debug.Log($"[Proximity] Desk collider: {col.GetType().Name} isTrigger={col.isTrigger}");

        if (typingInput == null) Debug.LogError("[Proximity] TypingInput not assigned!");
        if (computerDisplay == null) Debug.LogError("[Proximity] ComputerDisplay not assigned!");
        if (comboMultiplier == null) Debug.LogError("[Proximity] ComboMultiplier not assigned!");
    }

    void Update()
    {
        if (Physics.CheckSphere(transform.position, 2f))
            Debug.Log($"[Proximity] Desk position={transform.position}, Player nearby check passed");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Proximity] TriggerEnter: {other.gameObject.name} tag={other.tag}");
        if (!other.CompareTag("CameraPivot")) return;
        Activate();
    }

    void OnTriggerStay(Collider other)
    {
        if (_isActive) return;
        if (!other.CompareTag("CameraPivot")) return;
        Debug.Log("[Proximity] TriggerStay caught missed enter");
        Activate();
    }

    private void Activate()
    {
        _isActive = true;
        typingInput.IsActive = true;
        computerDisplay.Activate();
        Debug.Log("[Proximity] Activated computer display");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Proximity] TriggerExit: {other.gameObject.name} tag={other.tag}");
        if (!other.CompareTag("CameraPivot")) return;
        _isActive = false;
        typingInput.IsActive = false;
        computerDisplay.Deactivate();
        comboMultiplier.Reset();
        GameManager.Instance.SetLOCPerSec(0f);
    }
}
