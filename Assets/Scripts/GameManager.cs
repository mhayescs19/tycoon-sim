using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float dollarBalance = 47f;
    [SerializeField] private float locToDollarRate = 0.1f;

    private float typingLOCPerSec;
    private float passiveLOCPerSec;
    private float passiveLOCCarry;

    public float DollarBalance => dollarBalance;
    public float LOCToDollarRate => locToDollarRate;
    public int LOCCount { get; private set; }
    public float LOCPerSec => typingLOCPerSec + passiveLOCPerSec;

    public event Action<float> OnBalanceChanged;
    public event Action<int, float> OnLOCChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (LOCPerSec <= 0f) return;

        bool locChanged = false;
        if (passiveLOCPerSec > 0f)
        {
            passiveLOCCarry += passiveLOCPerSec * Time.deltaTime;
            int wholeLOC = Mathf.FloorToInt(passiveLOCCarry);
            if (wholeLOC > 0)
            {
                LOCCount += wholeLOC;
                passiveLOCCarry -= wholeLOC;
                locChanged = true;
            }
        }

        dollarBalance += LOCPerSec * locToDollarRate * Time.deltaTime;
        OnBalanceChanged?.Invoke(dollarBalance);

        if (locChanged)
        {
            OnLOCChanged?.Invoke(LOCCount, LOCPerSec);
        }
    }

    public void AddLOC(int amount)
    {
        if (amount <= 0) return;
        LOCCount += amount;
        OnLOCChanged?.Invoke(LOCCount, LOCPerSec);
    }

    public void SetLOCPerSec(float value)
    {
        SetTypingLOCPerSec(value);
    }

    public void SetTypingLOCPerSec(float value)
    {
        typingLOCPerSec = Mathf.Max(0f, value);
        OnLOCChanged?.Invoke(LOCCount, LOCPerSec);
    }

    public void SetPassiveLOCPerSec(float value)
    {
        passiveLOCPerSec = Mathf.Max(0f, value);
        OnLOCChanged?.Invoke(LOCCount, LOCPerSec);
    }

    public void SetLOCToDollarRate(float value) {
        locToDollarRate = value;
    }

    public void AddDollars(float amount)
    {
        dollarBalance += amount;
        OnBalanceChanged?.Invoke(dollarBalance);
    }

    public void SpendDollars(float amount)
    {
        dollarBalance = Mathf.Max(0f, dollarBalance - amount);
        OnBalanceChanged?.Invoke(dollarBalance);
    }
}
