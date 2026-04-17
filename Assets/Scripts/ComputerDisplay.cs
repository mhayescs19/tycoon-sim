using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerDisplay : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private ScrollRect scrollRect;

    private string _corpus;
    private int _charIndex;
    private string _buffer = "";

    void Awake()
    {
        TextAsset asset = Resources.Load<TextAsset>("GStackSourceCode");
        if (asset != null)
        {
            _corpus = asset.text;
        }
        else
        {
            _corpus = "// loading...\n";
            Debug.LogError("GStackSourceCode.txt not found in Resources/");
        }

        panel.SetActive(false);
    }

    public int TypeCharacters(int count)
    {
        int newlines = 0;
        for (int i = 0; i < count; i++)
        {
            char c = _corpus[_charIndex];
            _buffer += c;
            if (c == '\n') newlines++;
            _charIndex = (_charIndex + 1) % _corpus.Length;
        }

        codeText.text = _buffer;
        StartCoroutine(ScrollToBottom());
        return newlines;
    }

    public void Activate()
    {
        _buffer = "";
        _charIndex = 0;
        codeText.text = "";
        panel.SetActive(true);
        StartCoroutine(ScrollToTop());
    }

    public void Deactivate()
    {
        panel.SetActive(false);
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private IEnumerator ScrollToTop()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
