using NUnit.Framework;
using UnityEngine;

public class GameManagerTests
{
    private GameManager _gm;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        _gm = go.AddComponent<GameManager>();
        // Awake doesn't run in EditMode; set Instance manually
        var instanceProp = typeof(GameManager).GetProperty("Instance");
        instanceProp.SetValue(null, _gm);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_gm.gameObject);
    }

    [Test]
    public void AddLOC_IncreasesLOCCount()
    {
        _gm.AddLOC(5);
        Assert.AreEqual(5, _gm.LOCCount);
    }

    [Test]
    public void AddLOC_ConvertsToDollarsAtRate()
    {
        float startBalance = _gm.DollarBalance;
        _gm.AddLOC(10);
        Assert.AreEqual(startBalance + 10 * _gm.LOCToDollarRate, _gm.DollarBalance, 0.001f);
    }

    [Test]
    public void SpendDollars_ReducesBalance()
    {
        float startBalance = _gm.DollarBalance;
        _gm.SpendDollars(10f);
        Assert.AreEqual(startBalance - 10f, _gm.DollarBalance, 0.001f);
    }

    [Test]
    public void SpendDollars_ClampsAtZero()
    {
        _gm.SpendDollars(9999f);
        Assert.AreEqual(0f, _gm.DollarBalance, 0.001f);
    }

    [Test]
    public void OnBalanceChanged_FiresOnBalanceChange()
    {
        bool fired = false;
        _gm.OnBalanceChanged += _ => fired = true;
        _gm.AddDollars(1f);
        Assert.IsTrue(fired);
    }
}
