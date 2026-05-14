using System;
using System.Collections.Generic;
using UnityEngine;

public class OpenclawAgent
{
    public bool IsPurchased;
    public bool[] SkillsPurchased = new bool[4];
    public float PassiveLOC; // will change as skills get purchased
}

public class OpenclawManager : MonoBehaviour
{
    public static OpenclawManager Instance { get; private set; }

    [SerializeField] private AudioClip level1PurchaseClip;
    [SerializeField] private AudioClip level2PurchaseClip;
    [SerializeField] private AudioClip level3PurchaseClip;

    public bool DeskActive = false;
    
    public OpenclawAgent[] Agents { get; private set; }

    public int[] AgentCosts = { 200, 500, 1200, 3000 };
    public int[] SkillCosts = { 50, 100, 250, 500 };

    public int AgentsPurchased = 0;

    public float LOCPerSec { get; private set; } = 0f;

    private GameObject deskObject;
    private GameObject[] computerObjects = new GameObject[4];
    private AudioSource _purchaseAudioSource;

    public event Action OnStateChanged;

    void Awake()
    {
        Debug.Log("[OpenclawManager] Awake() called.");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[OpenclawManager] Another instance was found and is being destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsurePurchaseAudioSource();

        Agents = new OpenclawAgent[4];
        for (int i = 0; i < 4; i++)
        {
            Agents[i] = new OpenclawAgent();
        }
        Agents[3] = new OpenclawAgent { IsPurchased = false, SkillsPurchased = new bool[4] };

        deskObject = GameObject.Find("MacMiniDesk");
        if (deskObject != null)
        {
            deskObject.SetActive(false);
            Debug.Log("[OpenclawManager] Found and hid MacMiniDesk.");
        }
        else
        {
            Debug.LogWarning("[OpenclawManager] Could not find MacMiniDesk in the scene.");
        }

        for (int i = 0; i < computerObjects.Length; i++)
        {
            computerObjects[i] = GameObject.Find($"MacMini_{i + 1}");
            if (computerObjects[i] != null)
            {
                computerObjects[i].SetActive(false);
                Debug.Log($"[OpenclawManager] Found and hid MacMini_{i + 1}.");
            }
            else
            {
                Debug.LogWarning($"[OpenclawManager] Could not find MacMini_{i + 1} in the scene.");
            }
        }
    }

    void Start()
    {
        Debug.Log("[OpenclawManager] Start() called.");
        RecalcRate();
    }

    public int GetAgentCost(int agentIndex)
    {
        if (agentIndex < 0 || agentIndex >= AgentCosts.Length) return 0;
        return AgentCosts[agentIndex];
    }

    public int GetSkillCost(int agentIndex, int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= SkillCosts.Length) return 0;
        return SkillCosts[skillIndex] * (agentIndex + 1);
    }

    public bool TryPurchaseAgent(int agentIndex)
    {
        if (agentIndex < 0 || agentIndex >= Agents.Length) return false;
        if (Agents[agentIndex].IsPurchased) return false;

        int cost = GetAgentCost(agentIndex);
        
        if (GameManager.Instance.DollarBalance < cost) return false;
        GameManager.Instance.SpendDollars(cost);

        Agents[agentIndex].IsPurchased = true;

        if (!DeskActive && deskObject != null)
        {
            deskObject.SetActive(true);
            DeskActive = true;
        }

        if (agentIndex < computerObjects.Length && computerObjects[agentIndex] != null)
        {
            computerObjects[agentIndex].SetActive(true);
        }

        Agents[agentIndex].PassiveLOC = 2;

        AgentsPurchased += 1;
        PlayAgentPurchaseClip(agentIndex);
        RecalcRate();
        OnStateChanged?.Invoke();
        return true;
    }

    public bool TryPurchaseSkill(int agentIndex, int skillIndex)
    {
        if (agentIndex < 0 || agentIndex >= Agents.Length) return false;
        if (skillIndex < 0 || skillIndex >= 4) return false;
        
        if (!Agents[agentIndex].IsPurchased) return false;
        if (Agents[agentIndex].SkillsPurchased[skillIndex]) return false;

        int cost = GetSkillCost(agentIndex, skillIndex);

        if (GameManager.Instance.DollarBalance < cost) return false;
        GameManager.Instance.SpendDollars(cost);

        OpenclawAgent agent = Agents[agentIndex];
        agent.SkillsPurchased[skillIndex] = true;

        agent.PassiveLOC *= 2;

        float currentRate = GameManager.Instance.LOCToDollarRate;
        GameManager.Instance.SetLOCToDollarRate(currentRate + (float)0.5);

        RecalcRate();
        OnStateChanged?.Invoke();
        return true;
    }

    public int GetTotalAgentsPurchased()
    {
        if (Agents == null) return 0;
        int count = 0;
        foreach (var agent in Agents)
        {
            if (agent.IsPurchased) count++;
        }
        return count;
    }

    private void RecalcRate() {
        float total = 0f;
        foreach (var agent in Agents) {
            total += agent.PassiveLOC;
        }
        LOCPerSec = total;

        GameManager.Instance.SetPassiveLOCPerSec(LOCPerSec);
    }

    private void EnsurePurchaseAudioSource()
    {
        if (_purchaseAudioSource != null) return;

        _purchaseAudioSource = GetComponent<AudioSource>();
        if (_purchaseAudioSource == null)
            _purchaseAudioSource = gameObject.AddComponent<AudioSource>();

        _purchaseAudioSource.playOnAwake = false;
        _purchaseAudioSource.loop = false;
    }

    private void PlayAgentPurchaseClip(int agentIndex)
    {
        AudioClip clip = agentIndex switch
        {
            0 => level1PurchaseClip,
            1 => level2PurchaseClip,
            2 => level3PurchaseClip,
            _ => null
        };

        if (clip == null) return;

        EnsurePurchaseAudioSource();
        _purchaseAudioSource.PlayOneShot(clip);
    }
}
