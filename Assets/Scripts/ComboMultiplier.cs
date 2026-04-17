using UnityEngine;

public class ComboMultiplier : MonoBehaviour
{
    [SerializeField] private float bumpPerKeypress = 2f;
    [SerializeField] private float decaySpeed = 5f;
    [SerializeField] private float stopThreshold = 0.3f;

    public float Current { get; private set; } = 0f;

    private float _timeSinceLastKey = 0f;

    public void RegisterKeypress()
    {
        _timeSinceLastKey = 0f;
        Current += bumpPerKeypress;
        Push();
    }

    public void Tick(float deltaTime)
    {
        if (Current <= 0f) return;

        _timeSinceLastKey += deltaTime;
        if (_timeSinceLastKey >= stopThreshold)
        {
            Current = 0f;
            Push();
            return;
        }

        Current = Mathf.Lerp(Current, 0f, decaySpeed * deltaTime);
        if (Current < 0.05f) Current = 0f;
        Push();
    }

    public void Reset()
    {
        Current = 0f;
        Push();
    }

    private void Push()
    {
        GameManager.Instance.SetLOCPerSec(Current);
    }
}
