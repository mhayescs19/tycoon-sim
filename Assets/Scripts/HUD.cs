using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI locText;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private float locSecUpdateInterval = 0.5f;

    private float _displayedLocPerSec;
    private float _locSecTimer;

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

    void Update()
    {
        _locSecTimer += Time.deltaTime;
        if (_locSecTimer >= locSecUpdateInterval)
        {
            _locSecTimer = 0f;
            _displayedLocPerSec = Mathf.Round(GameManager.Instance.LOCPerSec * 10f) / 10f;
            locText.text = $"LOC: {GameManager.Instance.LOCCount} ({_displayedLocPerSec:F1}/sec)";
        }
    }

    void UpdateLOC(int count, float perSec)
    {
        // LOC count updates immediately; LOC/sec is throttled via Update
        locText.text = $"LOC: {count} ({_displayedLocPerSec:F1}/sec)";
    }

    void UpdateBalance(float balance)
    {
        balanceText.text = $"$ {balance:F2}";
    }
}
