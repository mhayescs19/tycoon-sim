using UnityEngine;

public class ComboMultiplier : MonoBehaviour
{
    [SerializeField] private float baseLocPerSec = 5f;
    [SerializeField] private float decayDelay = 1.5f;
    [SerializeField] private float multiplierStep = 0.2f;
    [SerializeField] private float maxMultiplier = 3f;

    public float Current { get; private set; } = 1f;

    private float _timeSinceLastKeypress = 0f;
    private bool _active = false;

    public void RegisterKeypress()
    {
        _timeSinceLastKeypress = 0f;
        _active = true;
        Current = Mathf.Min(Current + multiplierStep, maxMultiplier);
        Push();
    }

    public void Tick(float deltaTime)
    {
        if (!_active) return;

        _timeSinceLastKeypress += deltaTime;
        if (_timeSinceLastKeypress >= decayDelay)
        {
            Current = Mathf.Max(1f, Current - multiplierStep * deltaTime * 3f);
            Push();

            if (Mathf.Approximately(Current, 1f))
                _active = false;
        }
    }

    public void Reset()
    {
        Current = 1f;
        _timeSinceLastKeypress = 0f;
        _active = false;
        Push();
    }

    private void Push()
    {
        GameManager.Instance.SetLOCPerSec(baseLocPerSec * Current);
    }
}
