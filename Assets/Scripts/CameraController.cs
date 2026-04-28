using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 14f;
    [SerializeField] private float followSmoothing = 4f;
    [SerializeField] private Transform target;

    private float _zoom = 10f;
    private float _yRotation = 0f;
    private Vector3 _pivot = new Vector3(0f, 0f, 0f);

    void Update()
    {
        // Drift pivot toward player
        if (target != null)
            _pivot = Vector3.Lerp(_pivot, target.position, followSmoothing * Time.deltaTime);

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
