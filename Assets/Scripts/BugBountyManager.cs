using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BugBountyManager : MonoBehaviour
{
    private const float SkillCheckInputLeniency = 0.015f;
    private enum AlertTone { Default, Failure }

    [Serializable]
    public class BugBountyDefinition
    {
        public string Id;
        public string Title;
        public string Description;
        public int Reward;

        public BugBountyDefinition(string id, string title, int reward, string description = null)
        {
            Id = id;
            Title = string.IsNullOrWhiteSpace(title) ? "Unknown bounty" : title;
            Description = string.IsNullOrWhiteSpace(description) ? Title : description;
            Reward = reward;
        }
    }

    [Serializable]
    public class ActiveBounty
    {
        public BugBountyDefinition Definition;
        public int DifficultyTier;
        public float PenaltyPercent;
        public BugBountyNote Note;
        public string SpawnedAt;
    }

    [Serializable]
    public class BountyResult
    {
        public string Id;
        public string Title;
        public int Amount;
        public string Timestamp;
    }

    private class PuzzleSession
    {
        public bool IsActive;
        public bool AwaitingStart;
        public ActiveBounty Bounty;
        public int RequiredChecks;
        public int CompletedChecks;
        public int AllowedMisses;
        public int Misses;
        public float Speed;
        public float WindowSize;
        public float CheckDuration;
        public float CheckTimer;
        public float MarkerPosition;
        public int Direction = 1;
        public float TargetCenter;
    }

    public static BugBountyManager Instance { get; private set; }

    [SerializeField] private float minSpawnIntervalSeconds = 5f;
    [SerializeField] private float maxSpawnIntervalSeconds = 15f;
    [SerializeField] private int maxActiveBounties = 4;
    [SerializeField] private AudioClip spawnAlertClip;
    [SerializeField] private string spawnAlertResourceName = "bounty-alert";
    [SerializeField] private string spawnAlertEditorAssetPath = "Assets/Audio/bounty-alert.mp3";
    [SerializeField, Range(0f, 1f)] private float spawnAlertVolume = 0.05f;

    private readonly List<BugBountyDefinition> _catalog = new List<BugBountyDefinition>();
    private readonly List<ActiveBounty> _activeBounties = new List<ActiveBounty>();
    private readonly List<BountyResult> _successfulBounties = new List<BountyResult>();
    private readonly List<BountyResult> _failedBounties = new List<BountyResult>();
    private readonly Vector3[] _noteAnchors =
    {
        new Vector3(-2.2f, 1.3f, 3.95f),
        new Vector3(-1.0f, 1.7f, 3.95f),
        new Vector3(0.0f, 1.25f, 3.95f),
        new Vector3(1.0f, 1.65f, 3.95f),
        new Vector3(2.2f, 1.35f, 3.95f)
    };

    private int _nextCatalogIndex;
    private float _spawnTimer;
    private float _nextSpawnDelay;
    private Transform _noteRoot;

    private float _totalEarned;
    private float _totalPenalties;

    private PuzzleSession _puzzle = new PuzzleSession();
    private GameObject _puzzleOverlay;
    private TextMeshProUGUI _puzzleTitle;
    private GameObject _puzzleRewardChipRoot;
    private TextMeshProUGUI _puzzleRewardChipText;
    private TextMeshProUGUI _puzzleStatus;
    private TextMeshProUGUI _puzzleProgress;
    private RectTransform _puzzleBarRect;
    private RectTransform _puzzleZoneRect;
    private RectTransform _puzzleMarkerRect;
    private Button _puzzleStartButton;
    private Button _puzzleRejectButton;

    private GameObject _spawnAlertRoot;
    private TextMeshProUGUI _spawnAlertText;
    private Image _spawnAlertBackground;
    private float _spawnAlertTimer;
    private AudioSource _spawnAlertAudioSource;

    public IReadOnlyList<ActiveBounty> ActiveBounties => _activeBounties;
    public IReadOnlyList<BountyResult> SuccessfulBounties => _successfulBounties;
    public IReadOnlyList<BountyResult> FailedBounties => _failedBounties;
    public float TotalEarned => _totalEarned;
    public float TotalPenalties => _totalPenalties;
    public float CurrentSpawnInterval => _nextSpawnDelay;
    public float SpawnTimerRemaining => Mathf.Max(0f, _nextSpawnDelay - _spawnTimer);

    public event Action OnBountyDataChanged;
    public event Action<string> OnBountySpawned;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EnsureSpawnAlertAudioSource();
        LoadSpawnAlertClipFromResources();

        BuildCatalog();
        _nextSpawnDelay = RollSpawnDelay();

        var notesRootObject = new GameObject("BugBountyNotes");
        _noteRoot = notesRootObject.transform;

        CreateSpawnAlertUI();
        CreatePuzzleOverlay();
    }

    void Update()
    {
        if (_puzzle.IsActive)
        {
            if (_puzzle.AwaitingStart)
            {
                // Keyboard shortcut parity for the Start button.
                if (Keyboard.current != null &&
                    (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    StartPuzzleAttempt();
                }
                else if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    DeferPuzzleAttempt();
                }
            }
            else
            {
                UpdatePuzzleSession(Time.deltaTime);
            }
        }
        else
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _nextSpawnDelay && _activeBounties.Count < maxActiveBounties)
                SpawnNextBounty();
        }

        UpdateSpawnAlert(Time.deltaTime);
    }

    public void TryClaimBounty(string bountyId)
    {
        if (_puzzle.IsActive) return;
        BugBountyNote.HideClaimPromptGlobal();

        ActiveBounty bounty = _activeBounties.Find(b => b.Definition.Id == bountyId);
        if (bounty == null) return;

        if (bounty.Note != null)
            Destroy(bounty.Note.gameObject);

        _activeBounties.Remove(bounty);
        OnBountyDataChanged?.Invoke();
        BeginPuzzleForBounty(bounty);
    }

    private void SpawnNextBounty()
    {
        BugBountyDefinition definition = _catalog[_nextCatalogIndex];
        _nextCatalogIndex = (_nextCatalogIndex + 1) % _catalog.Count;

        int tier = ResolveTier(definition.Reward);
        float penaltyPercent = RollPenaltyPercent(tier);

        ActiveBounty active = new ActiveBounty
        {
            Definition = definition,
            DifficultyTier = tier,
            PenaltyPercent = penaltyPercent,
            SpawnedAt = DateTime.Now.ToString("HH:mm:ss")
        };

        active.Note = SpawnWallNote(active, _activeBounties.Count);
        _activeBounties.Add(active);

        _spawnTimer = 0f;
        _nextSpawnDelay = RollSpawnDelay();

        string message = $"New bug bounty spawned: {SafeTitle(definition)} (${definition.Reward})";
        ShowSpawnAlert(message);
        PlaySpawnAlertSound();
        OnBountySpawned?.Invoke(message);
        OnBountyDataChanged?.Invoke();
    }

    private void EnsureSpawnAlertAudioSource()
    {
        if (_spawnAlertAudioSource != null) return;

        _spawnAlertAudioSource = GetComponent<AudioSource>();
        if (_spawnAlertAudioSource == null)
            _spawnAlertAudioSource = gameObject.AddComponent<AudioSource>();

        _spawnAlertAudioSource.playOnAwake = false;
        _spawnAlertAudioSource.loop = false;
    }

    private void LoadSpawnAlertClipFromResources()
    {
        if (spawnAlertClip != null || string.IsNullOrWhiteSpace(spawnAlertResourceName)) return;

        spawnAlertClip = Resources.Load<AudioClip>(spawnAlertResourceName);
#if UNITY_EDITOR
        if (spawnAlertClip == null && !string.IsNullOrWhiteSpace(spawnAlertEditorAssetPath))
            spawnAlertClip = AssetDatabase.LoadAssetAtPath<AudioClip>(spawnAlertEditorAssetPath);
#endif
        if (spawnAlertClip == null)
            Debug.LogWarning($"[BugBountyManager] Could not load alert audio from Resources/{spawnAlertResourceName} or {spawnAlertEditorAssetPath}.");
    }

    private void PlaySpawnAlertSound()
    {
        if (spawnAlertClip == null)
            LoadSpawnAlertClipFromResources();
        if (spawnAlertClip == null) return;

        EnsureSpawnAlertAudioSource();
        _spawnAlertAudioSource.PlayOneShot(spawnAlertClip, spawnAlertVolume);
    }

    private BugBountyNote SpawnWallNote(ActiveBounty active, int anchorIndex)
    {
        int index = Mathf.Clamp(anchorIndex, 0, _noteAnchors.Length - 1);
        Vector3 position = _noteAnchors[index];

        GameObject noteObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        noteObj.transform.SetParent(_noteRoot, false);
        noteObj.transform.position = position;
        noteObj.transform.localScale = new Vector3(0.42f, 0.3f, 0.05f);

        Renderer renderer = noteObj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = new Color(0.95f, 0.87f, 0.55f, 1f);

        BoxCollider collider = noteObj.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        Rigidbody rb = noteObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        BugBountyNote note = noteObj.AddComponent<BugBountyNote>();
        note.Initialize(this, active.Definition.Id, SafeTitle(active.Definition), active.Definition.Reward);
        return note;
    }

    private void BeginPuzzleForBounty(ActiveBounty bounty)
    {
        if (bounty == null || bounty.Definition == null)
        {
            Debug.LogWarning("[BugBounty] Cannot start puzzle, bounty definition missing.");
            return;
        }

        if (_puzzleOverlay == null)
            CreatePuzzleOverlay();

        if (_puzzleOverlay == null)
        {
            Debug.LogWarning("[BugBounty] Cannot start puzzle, puzzle overlay not initialized.");
            return;
        }

        _puzzle.IsActive = true;
        _puzzle.Bounty = bounty;
        _puzzle.RequiredChecks = 2 + bounty.DifficultyTier;
        _puzzle.CompletedChecks = 0;
        _puzzle.AllowedMisses = Mathf.Max(1, 4 - ((bounty.DifficultyTier + 1) / 2));
        _puzzle.Misses = 0;
        _puzzle.Speed = Mathf.Lerp(0.55f, 1.4f, (bounty.DifficultyTier - 1) / 4f);
        _puzzle.WindowSize = Mathf.Lerp(0.34f, 0.12f, (bounty.DifficultyTier - 1) / 4f) * 0.5f;
        _puzzle.CheckDuration = Mathf.Lerp(2.6f, 1.2f, (bounty.DifficultyTier - 1) / 4f);
        _puzzle.MarkerPosition = 0.5f;
        _puzzle.Direction = 1;
        _puzzle.AwaitingStart = true;

        _puzzleOverlay.SetActive(true);
        UpdatePuzzleUI();

        if (_puzzleStartButton != null)
            _puzzleStartButton.gameObject.SetActive(true);
        if (_puzzleRejectButton != null)
            _puzzleRejectButton.gameObject.SetActive(true);
        if (EventSystem.current != null && _puzzleStartButton != null)
            EventSystem.current.SetSelectedGameObject(_puzzleStartButton.gameObject);
    }

    private void StartNextSkillCheck()
    {
        _puzzle.CheckTimer = 0f;
        _puzzle.TargetCenter = UnityEngine.Random.Range(0.2f, 0.8f);
    }

    private void UpdatePuzzleSession(float deltaTime)
    {
        _puzzle.CheckTimer += deltaTime;

        _puzzle.MarkerPosition += _puzzle.Direction * _puzzle.Speed * deltaTime;
        if (_puzzle.MarkerPosition >= 1f)
        {
            _puzzle.MarkerPosition = 1f;
            _puzzle.Direction = -1;
        }
        else if (_puzzle.MarkerPosition <= 0f)
        {
            _puzzle.MarkerPosition = 0f;
            _puzzle.Direction = 1;
        }

        UpdatePuzzleVisuals();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            bool success = Mathf.Abs(_puzzle.MarkerPosition - _puzzle.TargetCenter) <= ((_puzzle.WindowSize * 0.5f) + SkillCheckInputLeniency);
            if (success)
            {
                _puzzle.CompletedChecks++;
                if (_puzzle.CompletedChecks >= _puzzle.RequiredChecks)
                {
                    CompletePuzzleSuccess();
                    return;
                }

                StartNextSkillCheck();
                UpdatePuzzleUI();
            }
            else
            {
                RegisterPuzzleMiss();
            }
        }

        if (_puzzle.CheckTimer >= _puzzle.CheckDuration)
            RegisterPuzzleMiss();
    }

    private void RegisterPuzzleMiss()
    {
        _puzzle.Misses++;
        if (_puzzle.Misses >= _puzzle.AllowedMisses)
        {
            CompletePuzzleFailure();
            return;
        }

        StartNextSkillCheck();
        UpdatePuzzleUI();
    }

    private void StartPuzzleAttempt()
    {
        if (!_puzzle.IsActive) return;
        if (!_puzzle.AwaitingStart) return;

        _puzzle.AwaitingStart = false;
        _puzzle.CompletedChecks = 0;
        _puzzle.Misses = 0;
        StartNextSkillCheck();

        if (_puzzleStartButton != null) _puzzleStartButton.gameObject.SetActive(false);
        if (_puzzleRejectButton != null) _puzzleRejectButton.gameObject.SetActive(false);
        UpdatePuzzleUI();
    }

    private void DeferPuzzleAttempt()
    {
        if (!_puzzle.IsActive) return;
        ActiveBounty bounty = _puzzle.Bounty;
        if (bounty != null)
        {
            // Return the bounty note to active state if the player backs out before starting.
            bounty.Note = SpawnWallNote(bounty, _activeBounties.Count);
            _activeBounties.Add(bounty);
            OnBountyDataChanged?.Invoke();
        }

        ShowSpawnAlert("Bounty deferred.");
        EndPuzzle();
    }

    private void RejectPuzzleAttempt()
    {
        if (!_puzzle.IsActive) return;

        ActiveBounty bounty = _puzzle.Bounty;
        if (bounty != null)
            ShowSpawnAlert($"Bounty rejected: {SafeTitle(bounty.Definition)}");

        // Reject means delete/discard this bounty attempt for now.
        OnBountyDataChanged?.Invoke();
        EndPuzzle();
    }

    private void CompletePuzzleSuccess()
    {
        ActiveBounty bounty = _puzzle.Bounty;
        int reward = bounty.Definition.Reward;

        GameManager.Instance.AddDollars(reward);
        _totalEarned += reward;
        _successfulBounties.Insert(0, new BountyResult
        {
            Id = bounty.Definition.Id,
            Title = bounty.Definition.Title,
            Amount = reward,
            Timestamp = DateTime.Now.ToString("HH:mm:ss")
        });

        ShowSpawnAlert($"Bounty complete: +${reward}");
        EndPuzzle();
        OnBountyDataChanged?.Invoke();
    }

    private void CompletePuzzleFailure()
    {
        ActiveBounty bounty = _puzzle.Bounty;
        int penalty = Mathf.Max(1, Mathf.RoundToInt(bounty.Definition.Reward * (bounty.PenaltyPercent / 100f)));

        GameManager.Instance.SpendDollars(penalty);
        _totalPenalties += penalty;
        _failedBounties.Insert(0, new BountyResult
        {
            Id = bounty.Definition.Id,
            Title = bounty.Definition.Title,
            Amount = penalty,
            Timestamp = DateTime.Now.ToString("HH:mm:ss")
        });

        ShowSpawnAlert($"Bounty failed: -${penalty}", AlertTone.Failure);
        EndPuzzle();
        OnBountyDataChanged?.Invoke();
    }

    private void EndPuzzle()
    {
        _puzzle.IsActive = false;
        _puzzle.AwaitingStart = false;
        if (_puzzleOverlay != null)
            _puzzleOverlay.SetActive(false);
        _spawnTimer = 0f;
        _nextSpawnDelay = RollSpawnDelay();
    }

    private int ResolveTier(int reward)
    {
        if (reward <= 50) return 1;
        if (reward <= 120) return 2;
        if (reward <= 220) return 3;
        if (reward <= 350) return 4;
        return 5;
    }

    private float RollPenaltyPercent(int tier)
    {
        return UnityEngine.Random.Range(40f, 60f);
    }

    private float RollSpawnDelay()
    {
        return UnityEngine.Random.Range(minSpawnIntervalSeconds, maxSpawnIntervalSeconds);
    }

    private void CreateSpawnAlertUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        _spawnAlertRoot = new GameObject("BugBountySpawnAlert", typeof(RectTransform), typeof(Image));
        _spawnAlertRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = (RectTransform)_spawnAlertRoot.transform;
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -12f);
        rootRect.sizeDelta = new Vector2(620f, 42f);

        _spawnAlertBackground = _spawnAlertRoot.GetComponent<Image>();
        _spawnAlertBackground.color = new Color(0.15f, 0.24f, 0.16f, 0.92f);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(_spawnAlertRoot.transform, false);
        RectTransform textRect = (RectTransform)textObj.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 4f);
        textRect.offsetMax = new Vector2(-10f, -4f);

        _spawnAlertText = textObj.GetComponent<TextMeshProUGUI>();
        _spawnAlertText.text = string.Empty;
        _spawnAlertText.fontSize = 20;
        _spawnAlertText.alignment = TextAlignmentOptions.Center;
        _spawnAlertText.color = Color.white;

        _spawnAlertRoot.SetActive(false);
    }

    private void ShowSpawnAlert(string message, AlertTone tone = AlertTone.Default)
    {
        if (_spawnAlertRoot == null) return;
        _spawnAlertText.text = message;
        if (_spawnAlertBackground != null)
        {
            _spawnAlertBackground.color = tone == AlertTone.Failure
                ? new Color(0.42f, 0.12f, 0.12f, 0.94f)
                : new Color(0.15f, 0.24f, 0.16f, 0.92f);
        }
        _spawnAlertRoot.SetActive(true);
        _spawnAlertTimer = 3f;
    }

    private void UpdateSpawnAlert(float deltaTime)
    {
        if (_spawnAlertRoot == null || !_spawnAlertRoot.activeSelf) return;
        _spawnAlertTimer -= deltaTime;
        if (_spawnAlertTimer <= 0f)
            _spawnAlertRoot.SetActive(false);
    }

    private void CreatePuzzleOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        _puzzleOverlay = new GameObject("BugBountyPuzzleOverlay", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        _puzzleOverlay.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = (RectTransform)_puzzleOverlay.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(560f, 280f);

        Image rootImage = _puzzleOverlay.GetComponent<Image>();
        rootImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

        VerticalLayoutGroup layout = _puzzleOverlay.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;

        _puzzleTitle = CreateOverlayText(_puzzleOverlay.transform, "Title", 28, 40f);
        _puzzleProgress = CreateOverlayText(_puzzleOverlay.transform, "Progress", 20, 28f);
        _puzzleStatus = CreateOverlayText(_puzzleOverlay.transform, "Status", 18, 48f);
        CreateRewardChip();

        GameObject barRoot = new GameObject("SkillBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        barRoot.transform.SetParent(_puzzleOverlay.transform, false);
        LayoutElement barLayout = barRoot.GetComponent<LayoutElement>();
        barLayout.preferredHeight = 28f;

        _puzzleBarRect = (RectTransform)barRoot.transform;
        _puzzleBarRect.sizeDelta = new Vector2(460f, 28f);
        Image barImage = barRoot.GetComponent<Image>();
        barImage.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        GameObject zone = new GameObject("Zone", typeof(RectTransform), typeof(Image));
        zone.transform.SetParent(_puzzleBarRect, false);
        _puzzleZoneRect = (RectTransform)zone.transform;
        _puzzleZoneRect.anchorMin = new Vector2(0f, 0f);
        _puzzleZoneRect.anchorMax = new Vector2(0f, 1f);
        _puzzleZoneRect.pivot = new Vector2(0.5f, 0.5f);
        Image zoneImage = zone.GetComponent<Image>();
        zoneImage.color = new Color(0.24f, 0.55f, 0.22f, 0.85f);

        GameObject marker = new GameObject("Marker", typeof(RectTransform), typeof(Image));
        marker.transform.SetParent(_puzzleBarRect, false);
        _puzzleMarkerRect = (RectTransform)marker.transform;
        _puzzleMarkerRect.anchorMin = new Vector2(0f, 0f);
        _puzzleMarkerRect.anchorMax = new Vector2(0f, 1f);
        _puzzleMarkerRect.pivot = new Vector2(0.5f, 0.5f);
        _puzzleMarkerRect.sizeDelta = new Vector2(8f, 0f);
        Image markerImage = marker.GetComponent<Image>();
        markerImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        TextMeshProUGUI instruction = CreateOverlayText(_puzzleOverlay.transform, "Instruction", 16, 28f);
        instruction.text = "Press SPACE during checks when marker is in the green zone.";

        _puzzleStartButton = CreateOverlayButton(_puzzleOverlay.transform, "StartBountyButton", "Start Bounty", StartPuzzleAttempt);
        _puzzleRejectButton = CreateOverlayButton(_puzzleOverlay.transform, "RejectBountyButton", "Reject Bounty", RejectPuzzleAttempt);
        _puzzleStartButton.gameObject.SetActive(false);
        _puzzleRejectButton.gameObject.SetActive(false);

        _puzzleOverlay.SetActive(false);
    }

    private TextMeshProUGUI CreateOverlayText(Transform parent, string name, int size, float preferredHeight)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        LayoutElement layout = obj.GetComponent<LayoutElement>();
        layout.preferredHeight = preferredHeight;

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        return text;
    }

    private Button CreateOverlayButton(Transform parent, string name, string label, Action onClick)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);

        LayoutElement layout = obj.GetComponent<LayoutElement>();
        layout.preferredHeight = 34f;

        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.22f, 0.29f, 0.44f, 1f);

        Button button = obj.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.onClick.AddListener(() => onClick());
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.31f, 0.42f, 0.61f, 1f);
        colors.selectedColor = new Color(0.48f, 0.42f, 0.18f, 1f);
        colors.pressedColor = new Color(0.18f, 0.24f, 0.35f, 1f);
        button.colors = colors;

        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(obj.transform, false);
        RectTransform textRect = (RectTransform)textObj.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.text = label;
        return button;
    }

    private void CreateRewardChip()
    {
        _puzzleRewardChipRoot = new GameObject("RewardChip", typeof(RectTransform), typeof(Image));
        _puzzleRewardChipRoot.transform.SetParent(_puzzleOverlay.transform, false);

        var chipRect = (RectTransform)_puzzleRewardChipRoot.transform;
        chipRect.anchorMin = new Vector2(1f, 1f);
        chipRect.anchorMax = new Vector2(1f, 1f);
        chipRect.pivot = new Vector2(1f, 1f);
        chipRect.anchoredPosition = new Vector2(-20f, -20f);
        chipRect.sizeDelta = new Vector2(138f, 40f);

        var chipImage = _puzzleRewardChipRoot.GetComponent<Image>();
        chipImage.color = new Color(0.2f, 0.46f, 0.2f, 0.95f);

        var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(_puzzleRewardChipRoot.transform, false);

        var textRect = (RectTransform)textObj.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _puzzleRewardChipText = textObj.GetComponent<TextMeshProUGUI>();
        _puzzleRewardChipText.fontSize = 24f;
        _puzzleRewardChipText.alignment = TextAlignmentOptions.Center;
        _puzzleRewardChipText.color = Color.white;
        _puzzleRewardChipText.text = "$0";
    }

    private void UpdatePuzzleUI()
    {
        if (_puzzle.Bounty == null || _puzzle.Bounty.Definition == null) return;
        if (_puzzleTitle == null || _puzzleProgress == null || _puzzleStatus == null) return;

        string title = SafeTitle(_puzzle.Bounty.Definition);
        string description = SafeDescription(_puzzle.Bounty.Definition);
        int reward = _puzzle.Bounty.Definition.Reward;

        _puzzleTitle.text = title;
        _puzzleProgress.text = $"Checks {_puzzle.CompletedChecks}/{_puzzle.RequiredChecks}  |  Misses {_puzzle.Misses}/{_puzzle.AllowedMisses}";
        bool hasDistinctDescription = !string.IsNullOrWhiteSpace(description) && !string.Equals(description, title, StringComparison.OrdinalIgnoreCase);
        string details = hasDistinctDescription
            ? $"{description}\nFail penalty: {_puzzle.Bounty.PenaltyPercent:F1}%"
            : $"Fail penalty: {_puzzle.Bounty.PenaltyPercent:F1}%";
        _puzzleStatus.text = _puzzle.AwaitingStart
            ? $"{details}\nStart to begin, Reject to delete, Esc to defer."
            : details;
        if (_puzzleRewardChipText != null)
            _puzzleRewardChipText.text = $"<b>${reward}</b>";
        UpdatePuzzleVisuals();
    }

    private void UpdatePuzzleVisuals()
    {
        if (_puzzleBarRect == null || _puzzleZoneRect == null || _puzzleMarkerRect == null) return;

        float width = _puzzleBarRect.rect.width;
        if (width <= 0f) width = 460f;

        float zoneWidth = width * Mathf.Clamp01(_puzzle.WindowSize);
        float halfZone = zoneWidth * 0.5f;
        float unclampedCenter = Mathf.Clamp01(_puzzle.TargetCenter) * width;
        float zoneCenterX = Mathf.Clamp(unclampedCenter, halfZone, width - halfZone);
        _puzzleZoneRect.sizeDelta = new Vector2(zoneWidth, 0f);
        _puzzleZoneRect.anchoredPosition = new Vector2(zoneCenterX, 0f);

        float markerX = Mathf.Clamp01(_puzzle.MarkerPosition) * width;
        _puzzleMarkerRect.anchoredPosition = new Vector2(markerX, 0f);
    }

    private void BuildCatalog()
    {
        _catalog.Clear();

        _catalog.Add(new BugBountyDefinition("BB-001", "NullRef in settings modal", 15));
        _catalog.Add(new BugBountyDefinition("BB-002", "Save button double-submit", 20));
        _catalog.Add(new BugBountyDefinition("BB-003", "Tooltip overflow on low res", 25));
        _catalog.Add(new BugBountyDefinition("BB-004", "Broken tab order in auth form", 30));
        _catalog.Add(new BugBountyDefinition("BB-005", "Ghost click on modal close", 35));
        _catalog.Add(new BugBountyDefinition("BB-006", "Incorrect loading spinner state", 40));
        _catalog.Add(new BugBountyDefinition("BB-007", "Missing icon fallback", 45));
        _catalog.Add(new BugBountyDefinition("BB-008", "Toast appears behind panel", 50));
        _catalog.Add(new BugBountyDefinition("BB-009", "Profile image crop mismatch", 18));
        _catalog.Add(new BugBountyDefinition("BB-010", "Stale cache banner persists", 22));
        _catalog.Add(new BugBountyDefinition("BB-011", "Off-by-one in pagination", 55));
        _catalog.Add(new BugBountyDefinition("BB-012", "Search clears filters unexpectedly", 60));
        _catalog.Add(new BugBountyDefinition("BB-013", "Retry action misses last request", 65));
        _catalog.Add(new BugBountyDefinition("BB-014", "Session timer drift issue", 70));
        _catalog.Add(new BugBountyDefinition("BB-015", "Keyboard shortcut conflict", 75));
        _catalog.Add(new BugBountyDefinition("BB-016", "Notification dedupe bug", 80));
        _catalog.Add(new BugBountyDefinition("BB-017", "Form validation race condition", 85));
        _catalog.Add(new BugBountyDefinition("BB-018", "Scroll reset after refresh", 90));
        _catalog.Add(new BugBountyDefinition("BB-019", "Undo stack loses one step", 100));
        _catalog.Add(new BugBountyDefinition("BB-020", "Filter chip removal desync", 110));
        _catalog.Add(new BugBountyDefinition("BB-021", "Permission badge stale after role change", 120));
        _catalog.Add(new BugBountyDefinition("BB-022", "Export csv truncates unicode", 130));
        _catalog.Add(new BugBountyDefinition("BB-023", "Date parser timezone mismatch", 140));
        _catalog.Add(new BugBountyDefinition("BB-024", "Audit row sorting unstable", 150));
        _catalog.Add(new BugBountyDefinition("BB-025", "Draft autosave deadlock", 160));
        _catalog.Add(new BugBountyDefinition("BB-026", "Webhook retry jitter bug", 170));
        _catalog.Add(new BugBountyDefinition("BB-027", "Queue item duplication", 180));
        _catalog.Add(new BugBountyDefinition("BB-028", "Background job status phantom complete", 190));
        _catalog.Add(new BugBountyDefinition("BB-029", "Reconnect logic misses first packet", 200));
        _catalog.Add(new BugBountyDefinition("BB-030", "Billing preview rounds wrong", 210));
        _catalog.Add(new BugBountyDefinition("BB-031", "Leaderboard rank tie-break wrong", 220));
        _catalog.Add(new BugBountyDefinition("BB-032", "Chat mention parser false positives", 58));
        _catalog.Add(new BugBountyDefinition("BB-033", "Sticky header overlaps dropdown", 62));
        _catalog.Add(new BugBountyDefinition("BB-034", "Attachment preview orientation bug", 68));
        _catalog.Add(new BugBountyDefinition("BB-035", "Mobile nav flicker", 74));
        _catalog.Add(new BugBountyDefinition("BB-036", "Focus trap escapes modal", 82));
        _catalog.Add(new BugBountyDefinition("BB-037", "Theme switch one-frame flash", 88));
        _catalog.Add(new BugBountyDefinition("BB-038", "Keyboard repeat floods action", 96));
        _catalog.Add(new BugBountyDefinition("BB-039", "Replay timeline marker drift", 104));
        _catalog.Add(new BugBountyDefinition("BB-040", "Chart tooltip stale point", 112));
        _catalog.Add(new BugBountyDefinition("BB-041", "AB flag mismatch in edge region", 125));
        _catalog.Add(new BugBountyDefinition("BB-042", "Websocket backoff reset bug", 135));
        _catalog.Add(new BugBountyDefinition("BB-043", "Report generator memory leak", 145));
        _catalog.Add(new BugBountyDefinition("BB-044", "Parser ignores escaped delimiter", 155));
        _catalog.Add(new BugBountyDefinition("BB-045", "Sync conflict picker wrong default", 165));
        _catalog.Add(new BugBountyDefinition("BB-046", "Merge resolver drops blank lines", 175));
        _catalog.Add(new BugBountyDefinition("BB-047", "Partial update overwrites null", 185));
        _catalog.Add(new BugBountyDefinition("BB-048", "Queue priority inversion", 195));
        _catalog.Add(new BugBountyDefinition("BB-049", "Batch action timeout regression", 205));
        _catalog.Add(new BugBountyDefinition("BB-050", "Tenant boundary cache bleed", 220));
        _catalog.Add(new BugBountyDefinition("BB-051", "Inventory count eventually wrong", 230));
        _catalog.Add(new BugBountyDefinition("BB-052", "Refund idempotency edge case", 240));
        _catalog.Add(new BugBountyDefinition("BB-053", "Retry budget not respected", 250));
        _catalog.Add(new BugBountyDefinition("BB-054", "Circuit breaker half-open flaps", 260));
        _catalog.Add(new BugBountyDefinition("BB-055", "Search index stale shard", 270));
        _catalog.Add(new BugBountyDefinition("BB-056", "Leader election split-brain window", 280));
        _catalog.Add(new BugBountyDefinition("BB-057", "Snapshot restore ordering bug", 290));
        _catalog.Add(new BugBountyDefinition("BB-058", "Event replay duplicates side-effect", 300));
        _catalog.Add(new BugBountyDefinition("BB-059", "Rule engine precedence mismatch", 315));
        _catalog.Add(new BugBountyDefinition("BB-060", "Delayed job orphan handling", 330));
        _catalog.Add(new BugBountyDefinition("BB-061", "Multi-region failover warmup bug", 345));
        _catalog.Add(new BugBountyDefinition("BB-062", "Dependency graph cycle false negative", 350));
        _catalog.Add(new BugBountyDefinition("BB-063", "CSS grid overflow in RTL", 28));
        _catalog.Add(new BugBountyDefinition("BB-064", "Copy to clipboard strips newline", 34));
        _catalog.Add(new BugBountyDefinition("BB-065", "Markdown code block language loss", 42));
        _catalog.Add(new BugBountyDefinition("BB-066", "Sidebar collapse state not persisted", 48));
        _catalog.Add(new BugBountyDefinition("BB-067", "Input debounce too aggressive", 57));
        _catalog.Add(new BugBountyDefinition("BB-068", "Filename sanitizer misses emoji", 66));
        _catalog.Add(new BugBountyDefinition("BB-069", "Token refresh duplicate request", 76));
        _catalog.Add(new BugBountyDefinition("BB-070", "Avatar color hash inconsistency", 86));
        _catalog.Add(new BugBountyDefinition("BB-071", "Presence indicator stale after sleep", 98));
        _catalog.Add(new BugBountyDefinition("BB-072", "Draft recover banner loops", 108));
        _catalog.Add(new BugBountyDefinition("BB-073", "Feature gate ignores org override", 118));
        _catalog.Add(new BugBountyDefinition("BB-074", "Currency formatter locale fallback", 128));
        _catalog.Add(new BugBountyDefinition("BB-075", "Command palette score ranking bug", 138));
        _catalog.Add(new BugBountyDefinition("BB-076", "Query planner wrong join path", 225));
        _catalog.Add(new BugBountyDefinition("BB-077", "Deadletter queue redrive mismatch", 245));
        _catalog.Add(new BugBountyDefinition("BB-078", "Priority scheduler starvation", 265));
        _catalog.Add(new BugBountyDefinition("BB-079", "Segment compaction corruption guard", 285));
        _catalog.Add(new BugBountyDefinition("BB-080", "Replica lag alarm false positive", 305));
        _catalog.Add(new BugBountyDefinition("BB-081", "Metrics cardinality explosion path", 325));
        _catalog.Add(new BugBountyDefinition("BB-082", "Lock contention hotspot regression", 340));
        _catalog.Add(new BugBountyDefinition("BB-083", "Schema migration rollback fault", 360));
        _catalog.Add(new BugBountyDefinition("BB-084", "Distributed trace parent loss", 375));
        _catalog.Add(new BugBountyDefinition("BB-085", "Cache stampede on cold key", 390));
        _catalog.Add(new BugBountyDefinition("BB-086", "Cross-shard transaction leak", 405));
        _catalog.Add(new BugBountyDefinition("BB-087", "Message ordering guarantee breach", 420));
        _catalog.Add(new BugBountyDefinition("BB-088", "Blob compactor partial write", 435));
        _catalog.Add(new BugBountyDefinition("BB-089", "Sandbox escape in parser edge", 450));
        _catalog.Add(new BugBountyDefinition("BB-090", "Replay attack nonce reuse", 465));
        _catalog.Add(new BugBountyDefinition("BB-091", "Checkpoint restore data skew", 480));
        _catalog.Add(new BugBountyDefinition("BB-092", "Quorum write stale-read window", 495));
        _catalog.Add(new BugBountyDefinition("BB-093", "Wallet reconciliation drift", 500));
        _catalog.Add(new BugBountyDefinition("BB-094", "Geo routing asymmetry fault", 355));
        _catalog.Add(new BugBountyDefinition("BB-095", "Burst load admission collapse", 370));
        _catalog.Add(new BugBountyDefinition("BB-096", "Async cancellation token leak", 385));
        _catalog.Add(new BugBountyDefinition("BB-097", "Rollup aggregator double-count", 410));
        _catalog.Add(new BugBountyDefinition("BB-098", "Secret rotation race", 440));
        _catalog.Add(new BugBountyDefinition("BB-099", "Compensating transaction miss", 470));
        _catalog.Add(new BugBountyDefinition("BB-100", "Cross-tenant ACL bypass edge", 490));
    }

    private static string SafeTitle(BugBountyDefinition definition)
    {
        if (definition == null) return "Unknown bounty";
        return string.IsNullOrWhiteSpace(definition.Title) ? "Unknown bounty" : definition.Title;
    }

    private static string SafeDescription(BugBountyDefinition definition)
    {
        if (definition == null) return "No description provided.";
        return string.IsNullOrWhiteSpace(definition.Description) ? "No description provided." : definition.Description;
    }
}
