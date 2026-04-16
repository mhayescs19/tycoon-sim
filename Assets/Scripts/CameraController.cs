using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float panSpeed = 5f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 14f;

    private float _zoom = 10f;
    private float _yRotation = 0f;
    private Vector3 _pivot = new Vector3(0f, 0f, 0f);

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // Pan — WASD relative to current Y rotation
        Vector3 forward = new Vector3(Mathf.Sin(_yRotation * Mathf.Deg2Rad), 0f, Mathf.Cos(_yRotation * Mathf.Deg2Rad));
        Vector3 right = new Vector3(forward.z, 0f, -forward.x);

        if (keyboard.wKey.isPressed) _pivot += forward * panSpeed * Time.deltaTime;
        if (keyboard.sKey.isPressed) _pivot -= forward * panSpeed * Time.deltaTime;
        if (keyboard.dKey.isPressed) _pivot += right * panSpeed * Time.deltaTime;
        if (keyboard.aKey.isPressed) _pivot -= right * panSpeed * Time.deltaTime;

        // Rotate — Q/E
        if (keyboard.qKey.isPressed) _yRotation -= rotateSpeed * Time.deltaTime;
        if (keyboard.eKey.isPressed) _yRotation += rotateSpeed * Time.deltaTime;

        // Zoom — scroll wheel
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
            _zoom = Mathf.Clamp(_zoom - scroll * 0.1f, minZoom, maxZoom);

        ApplyTransform();
    }

    void ApplyTransform()
    {
        float pitch = 40;
        Quaternion rotation = Quaternion.Euler(pitch, _yRotation, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -_zoom);
        transform.position = _pivot + offset;
        transform.rotation = rotation;
    }
}
