using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float dollarBalance = 47f;
    [SerializeField] private float locToDollarRate = 0.1f;
    [SerializeField] private AudioClip policeSirenClip;
    [SerializeField] private string policeSirenResourceName = "police-siren";
    [SerializeField] private string policeSirenEditorAssetPath = "Assets/Audio/police-siren.mp3";
    [SerializeField] private float policeSirenIntervalSeconds = 30f;
    [SerializeField, Range(0f, 1f)] private float policeSirenVolume = 0.1f;

    private float typingLOCPerSec;
    private float passiveLOCPerSec;
    private float passiveLOCCarry;
    private float policeSirenTimer;
    private AudioSource policeSirenAudioSource;

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
        EnsurePoliceSirenAudioSource();
        LoadPoliceSirenClip();
    }

    void Update()
    {
        UpdatePoliceSirenTimer(Time.deltaTime);

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

    private void UpdatePoliceSirenTimer(float deltaTime)
    {
        if (policeSirenIntervalSeconds <= 0f) return;

        policeSirenTimer += deltaTime;
        if (policeSirenTimer < policeSirenIntervalSeconds) return;

        policeSirenTimer = 0f;
        PlayPoliceSiren();
    }

    private void EnsurePoliceSirenAudioSource()
    {
        if (policeSirenAudioSource != null) return;

        policeSirenAudioSource = GetComponent<AudioSource>();
        if (policeSirenAudioSource == null)
            policeSirenAudioSource = gameObject.AddComponent<AudioSource>();

        policeSirenAudioSource.playOnAwake = false;
        policeSirenAudioSource.loop = false;
    }

    private void LoadPoliceSirenClip()
    {
        if (policeSirenClip != null || string.IsNullOrWhiteSpace(policeSirenResourceName)) return;

        policeSirenClip = Resources.Load<AudioClip>(policeSirenResourceName);
#if UNITY_EDITOR
        if (policeSirenClip == null && !string.IsNullOrWhiteSpace(policeSirenEditorAssetPath))
            policeSirenClip = AssetDatabase.LoadAssetAtPath<AudioClip>(policeSirenEditorAssetPath);
#endif
        if (policeSirenClip == null)
            Debug.LogWarning($"[GameManager] Could not load police siren audio from Resources/{policeSirenResourceName} or {policeSirenEditorAssetPath}.");
    }

    private void PlayPoliceSiren()
    {
        if (policeSirenClip == null)
            LoadPoliceSirenClip();
        if (policeSirenClip == null) return;

        EnsurePoliceSirenAudioSource();
        policeSirenAudioSource.PlayOneShot(policeSirenClip, policeSirenVolume);
    }
}
