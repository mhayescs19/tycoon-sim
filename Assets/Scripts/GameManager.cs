using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float dollarBalance = 47f;
    [SerializeField] private float locToDollarRate = 0.1f;

    public float DollarBalance => dollarBalance;
    public float LOCToDollarRate => locToDollarRate;
    public int LOCCount { get; private set; }
    public float LOCPerSec { get; private set; }

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

    public void AddLOC(int amount)
    {
        if (amount <= 0) return;
        LOCCount += amount;
        dollarBalance += amount * locToDollarRate;
        OnLOCChanged?.Invoke(LOCCount, LOCPerSec);
        OnBalanceChanged?.Invoke(dollarBalance);
    }

    public void SetLOCPerSec(float value)
    {
        LOCPerSec = value;
        OnLOCChanged?.Invoke(LOCCount, LOCPerSec);
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
