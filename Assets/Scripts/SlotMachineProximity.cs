using UnityEngine;

public class SlotMachineProximity : MonoBehaviour
{
    [SerializeField] private SlotMachineDisplay slotMachineDisplay;

    private bool _isActive = false;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("CameraPivot")) return;
        Activate();
    }

    void OnTriggerStay(Collider other)
    {
        if (_isActive) return;
        if (!other.CompareTag("CameraPivot")) return;
        Activate();
    }

    private void Activate()
    {
        _isActive = true;
        slotMachineDisplay.Activate();
        Debug.Log("[Proximity] Activated slot machine display");
    }

    void OnTriggerExit(Collider other) {
        if (!other.CompareTag("CameraPivot")) return;
        _isActive = false;
        slotMachineDisplay.Deactivate();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
