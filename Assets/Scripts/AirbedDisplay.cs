using UnityEngine;

public class AirbedDisplay : MonoBehaviour
{
    private static readonly Vector3[] BedPositions =
    {
        new Vector3( 2.0f, 0f,  1.0f),
        new Vector3( 2.0f, 0f, -0.8f),
        new Vector3( 2.0f, 0f, -2.6f),
    };

    private static readonly Color MattressColor = new Color(0.45f, 0.72f, 0.90f, 1f);
    private static readonly Color PillowColor   = new Color(0.95f, 0.95f, 0.95f, 1f);

    private GameObject[] _bedRoots;

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

    private void BuildBeds()
    {
        _bedRoots = new GameObject[AirbedManager.MaxUnits];

        for (int i = 0; i < AirbedManager.MaxUnits; i++)
        {
            GameObject root = new GameObject($"AirbedUnit_{i + 1}");
            root.transform.position = BedPositions[i];

            GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mattress.name = "Mattress";
            mattress.transform.SetParent(root.transform, false);
            mattress.transform.localPosition = new Vector3(0f, 0.10f, 0f);
            mattress.transform.localScale    = new Vector3(1.90f, 0.20f, 0.90f);
            SetColor(mattress, MattressColor);
            DisableCollider(mattress);

            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Cube);
            puff.name = "MattressPuff";
            puff.transform.SetParent(root.transform, false);
            puff.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            puff.transform.localScale    = new Vector3(1.82f, 0.06f, 0.82f);
            SetColor(puff, new Color(0.52f, 0.78f, 0.94f, 1f));
            DisableCollider(puff);

            GameObject pillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillow.name = "Pillow";
            pillow.transform.SetParent(root.transform, false);
            pillow.transform.localPosition = new Vector3(0.72f, 0.28f, 0f);
            pillow.transform.localScale    = new Vector3(0.38f, 0.10f, 0.68f);
            SetColor(pillow, PillowColor);
            DisableCollider(pillow);

            root.SetActive(false);
            _bedRoots[i] = root;
        }
    }

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

    private static void SetColor(GameObject obj, Color color)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
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
