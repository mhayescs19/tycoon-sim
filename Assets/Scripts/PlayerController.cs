using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private TypingInput typingInput;

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (typingInput != null && typingInput.IsActive) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector3 input = Vector3.zero;
        if (keyboard.wKey.isPressed) input += Vector3.forward;
        if (keyboard.sKey.isPressed) input += Vector3.back;
        if (keyboard.dKey.isPressed) input += Vector3.right;
        if (keyboard.aKey.isPressed) input += Vector3.left;

        _rb.MovePosition(_rb.position + input.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
