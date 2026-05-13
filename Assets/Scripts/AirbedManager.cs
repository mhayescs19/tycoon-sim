using System;
using UnityEngine;

public class AirbedManager : MonoBehaviour
{
    public static AirbedManager Instance { get; private set; }

    public const int MaxUnits = 3;

    private static readonly int[] UnitCosts = { 100, 250, 500 };
    private static readonly float[] UnitDollarSec = { 0.50f, 0.35f, 0.20f };

    public int UnitsOwned { get; private set; } = 0;
    public float DollarPerSec { get; private set; } = 0f;

    public int NextUnitCost => UnitsOwned < MaxUnits ? UnitCosts[UnitsOwned] : -1;
    public bool IsMaxed => UnitsOwned >= MaxUnits;

    public event Action OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (DollarPerSec <= 0f) return;
        GameManager.Instance.AddDollars(DollarPerSec * Time.deltaTime);
    }

    public bool TryPurchaseUnit()
    {
        if (IsMaxed) return false;

        int cost = UnitCosts[UnitsOwned];
        if (GameManager.Instance.DollarBalance < cost) return false;

        GameManager.Instance.SpendDollars(cost);
        UnitsOwned++;
        RecalcRate();
        OnStateChanged?.Invoke();
        return true;
    }

    private void RecalcRate()
    {
        float total = 0f;
        for (int i = 0; i < UnitsOwned; i++)
            total += UnitDollarSec[i];
        DollarPerSec = total;
    }
}
