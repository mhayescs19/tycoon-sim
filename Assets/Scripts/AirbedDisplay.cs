using UnityEngine;

/// <summary>
/// Procedurally builds up to 3 blow-up mattress + pillow models from primitives
/// and shows/hides them based on AirbedManager.UnitsOwned.
/// 
/// Attach this component to any persistent GameObject in the scene (e.g. GameManager).
/// The beds are spawned automatically at three hardcoded floor positions.
/// </summary>
public class AirbedDisplay : MonoBehaviour
{
    // ── Hardcoded bed floor positions (world space) ───────────────────────────
    // Adjust these Vector3s to fit your scene layout.
    private static readonly Vector3[] BedPositions =
    {
        new Vector3( 2.0f, 0f,  1.0f),
        new Vector3( 2.0f, 0f, -0.8f),
        new Vector3( 2.0f, 0f, -2.6f),
    };

    // Mattress colours (light blue inflatable look)
    private static readonly Color MattressColor = new Color(0.45f, 0.72f, 0.90f, 1f);
    private static readonly Color PillowColor   = new Color(0.95f, 0.95f, 0.95f, 1f);

    private GameObject[] _bedRoots; // one root per unit slot

    // ── Unity ─────────────────────────────────────────────────────────────────
    void Start()
    {
        BuildBeds();
        RefreshVisibility();
        AirbedManager.Instance.OnStateChanged += RefreshVisibility;
    }

    void OnDestroy()
    {
        if (AirbedManager.Instance != null)
            AirbedManager.Instance.OnStateChanged -= RefreshVisibility;
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    private void BuildBeds()
    {
        _bedRoots = new GameObject[AirbedManager.MaxUnits];

        for (int i = 0; i < AirbedManager.MaxUnits; i++)
        {
            GameObject root = new GameObject($"AirbedUnit_{i + 1}");
            root.transform.position = BedPositions[i];

            // ── Mattress ──────────────────────────────────────────────────────
            // Main body: wide, low, slightly rounded-looking cube
            GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mattress.name = "Mattress";
            mattress.transform.SetParent(root.transform, false);
            mattress.transform.localPosition = new Vector3(0f, 0.10f, 0f);
            mattress.transform.localScale    = new Vector3(1.90f, 0.20f, 0.90f);
            SetColor(mattress, MattressColor);
            DisableCollider(mattress);

            // Slightly puffed top layer to give inflatable look
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Cube);
            puff.name = "MattressPuff";
            puff.transform.SetParent(root.transform, false);
            puff.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            puff.transform.localScale    = new Vector3(1.82f, 0.06f, 0.82f);
            SetColor(puff, new Color(0.52f, 0.78f, 0.94f, 1f));
            DisableCollider(puff);

            // ── Pillow ────────────────────────────────────────────────────────
            GameObject pillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillow.name = "Pillow";
            pillow.transform.SetParent(root.transform, false);
            // Place at the head end (east, +X side) of the mattress
            pillow.transform.localPosition = new Vector3(0.72f, 0.28f, 0f);
            pillow.transform.localScale    = new Vector3(0.38f, 0.10f, 0.68f);
            SetColor(pillow, PillowColor);
            DisableCollider(pillow);

            // Hide by default; RefreshVisibility will show as needed
            root.SetActive(false);
            _bedRoots[i] = root;
        }
    }

    // ── Visibility ────────────────────────────────────────────────────────────
    private void RefreshVisibility()
    {
        if (_bedRoots == null) return;
        int owned = AirbedManager.Instance.UnitsOwned;
        for (int i = 0; i < _bedRoots.Length; i++)
        {
            if (_bedRoots[i] != null)
                _bedRoots[i].SetActive(i < owned);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static void SetColor(GameObject obj, Color color)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            // Create a new material instance so beds don't share the default material
            r.material = new Material(r.sharedMaterial);
            r.material.color = color;
        }
    }

    private static void DisableCollider(GameObject obj)
    {
        Collider c = obj.GetComponent<Collider>();
        if (c != null) c.enabled = false;
    }
}
