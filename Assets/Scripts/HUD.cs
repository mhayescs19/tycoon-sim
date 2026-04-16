using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI locText;
    [SerializeField] private TextMeshProUGUI balanceText;

    void Start()
    {
        GameManager.Instance.OnLOCChanged += UpdateLOC;
        GameManager.Instance.OnBalanceChanged += UpdateBalance;

        UpdateLOC(GameManager.Instance.LOCCount, GameManager.Instance.LOCPerSec);
        UpdateBalance(GameManager.Instance.DollarBalance);
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnLOCChanged -= UpdateLOC;
        GameManager.Instance.OnBalanceChanged -= UpdateBalance;
    }

    void UpdateLOC(int count, float perSec)
    {
        locText.text = $"LOC: {count} ({perSec:F1}/sec)";
    }

    void UpdateBalance(float balance)
    {
        balanceText.text = $"$ {balance:F2}";
    }
}
