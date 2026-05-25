using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NPCThoughtController : MonoBehaviour
{
    [Header("Thought Bubble UI")]
    [SerializeField] private GameObject bubbleRoot;
    [SerializeField] private TMP_Text bubbleText;
    [SerializeField] private GameObject loadingDots;

    [Header("Claude API (leave empty if not used)")]
    [SerializeField] private string apiKey = "";
    private const string API_URL = "https://api.anthropic.com/v1/messages";
    private const string MODEL = "claude-sonnet-4-20250514";

    [Header("Typewriter speed (seconds/character)")]
    [SerializeField] private float typeSpeed = 0.028f;

    [Header("Keep thinking after display (seconds)")]
    [SerializeField] private float holdAfterDisplay = 2.2f;

    private const string SYSTEM =
        "You're senior Unity developer of 0 works experience, 1st time gone to interview. " +
        "When the candidate answers Unity/C# technical questions, express your internal thoughts " +
        "briefly (1-2 sentences, in Vietnamese) about the quality of the answer. " +
        "Do not speak directly to the candidate. Only think silently. " +
        "Maximum 25 words. Start with an opening quotation mark.";

    private void Awake()
    {
        if (bubbleRoot) bubbleRoot.SetActive(false);
    }

    public void ShowThought(string question, string answer,
                            string hint, int score, Action onDone)
    {
        StartCoroutine(ThoughtRoutine(question, answer, hint, score, onDone));
    }

    public void ShowFinalThought(int score, int max)
    {
        float p = (float)score / max;
        string msg = p >= 0.8f ? "\"Excellent candidate, I would recommend hiring immediately.\"" :
                     p >= 0.6f ? "\"Quite good, needs a bit more practical experience.\"" :
                     p >= 0.4f ? "\"Knowledge is still thin, might consider for an intern position.\"" :
                                 "\"Not enough foundation for this position, needs to learn more.\"";
        StartCoroutine(TypewriterShow(msg, null));
    }

    private IEnumerator ThoughtRoutine(string question, string answer,
                                       string hint, int score, Action onDone)
    {
        if (bubbleRoot) bubbleRoot.SetActive(true);
        if (loadingDots) loadingDots.SetActive(true);
        if (bubbleText) bubbleText.text = "";

        string thought = null;

        //if has api, use, if not, fallback after 1s
        if (!string.IsNullOrEmpty(apiKey))
        {
            bool done = false;
            StartCoroutine(CallAPI(
                BuildPrompt(question, answer, hint, score),
                r => { thought = r; done = true; }
            ));

            float elapsed = 0f;
            while (!done && elapsed < 8f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        if (loadingDots) loadingDots.SetActive(false);
        if (string.IsNullOrEmpty(thought)) thought = Fallback(score);

        yield return TypewriterShow(thought, onDone);
    }

    private IEnumerator TypewriterShow(string text, Action onDone)
    {
        if (bubbleRoot) bubbleRoot.SetActive(true);
        if (bubbleText) bubbleText.text = "";

        foreach (char c in text)
        {
            if (bubbleText) bubbleText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(holdAfterDisplay);
        onDone?.Invoke();
    }

    private string BuildPrompt(string q, string a, string hint, int score) =>
        $"Question: \"{q}\"\nCandidate's answer: \"{a}\"\n" +
        $"(Quality {score}/3 — {hint})\nThink silently about your reaction:";

    private IEnumerator CallAPI(string prompt, Action<string> onResult)
    {
        var body = $"{{\"model\":\"{MODEL}\",\"max_tokens\":100," +
                   $"\"system\":{JsonEscape(SYSTEM)}," +
                   $"\"messages\":[{{\"role\":\"user\",\"content\":{JsonEscape(prompt)}}}]}}";

        using var req = new UnityWebRequest(API_URL, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("x-api-key", apiKey);
        req.SetRequestHeader("anthropic-version", "2023-06-01");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string raw = req.downloadHandler.text;
                int ts = raw.IndexOf("\"text\":\"") + 8;
                int te = raw.IndexOf("\"", ts);
                string txt = raw.Substring(ts, te - ts)
                                .Replace("\\n", " ").Replace("\\\"", "\"");
                onResult?.Invoke(txt);
            }
            catch { onResult?.Invoke(Fallback(1)); }
        }
        else
        {
            //Debug.LogWarning($"[NPC API] {req.error}");
            onResult?.Invoke(Fallback(1));
        }
    }

    private static string Fallback(int score) => score switch
    {
        3 => "\"The answer is confident, the candidate understands the technical principles well.\"",
        2 => "\"The answer is acceptable but lacks depth, needs more practical experience.\"",
        _ => "\"The answer is vague, does not grasp the core concepts.\""
    };

    private static string JsonEscape(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\n", "\\n").Replace("\r", "") + "\"";
}