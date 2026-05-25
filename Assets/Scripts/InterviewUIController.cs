using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterviewUIController : MonoBehaviour
{
    [Header("Question Panel")]
    [SerializeField] private CanvasGroup questionPanel;
    [SerializeField] private TMP_Text questionCounterText;
    [SerializeField] private TMP_Text questionBodyText;

    [Header("Choice Buttons")]
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceLabels;

    [Header("Progress Dots (10 imgs)")]
    [SerializeField] private Image[] progressDots;
    [SerializeField] private Color colorDone = new Color(0.55f, 0.75f, 1f, 1f);
    [SerializeField] private Color colorCurrent = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color colorPending = new Color(1f, 1f, 1f, 0.18f);

    [Header("Thinking dots")]
    [SerializeField] private GameObject thinkingIndicator;

    [Header("Result Screen")]
    [SerializeField] private CanvasGroup resultPanel;
    [SerializeField] private TMP_Text resultScoreText;
    [SerializeField] private TMP_Text resultGradeText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Fade duration (secs)")]
    [SerializeField] private float fadeDuration = 0.35f;

    private InterviewManager _manager;

    private void Awake()
    {
        _manager = FindFirstObjectByType<InterviewManager>();

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int idx = i;
            choiceButtons[i].onClick.AddListener(() => _manager.OnChoiceSelected(idx));
        }

        if (restartButton) restartButton.onClick.AddListener(_manager.RestartInterview);
        if (quitButton) quitButton.onClick.AddListener(_manager.ExitInterview);

        SetInstant(questionPanel, false);
        SetInstant(resultPanel, false);
        if (thinkingIndicator) thinkingIndicator.SetActive(false);
    }

    public void DisplayQuestion(InterviewQuestionData data, int index, int total)
    {
        questionCounterText.text = $"Quest {index + 1} / {total}";
        questionBodyText.text = data.question;

        for (int i = 0; i < choiceLabels.Length; i++)
        {
            choiceLabels[i].text = data.choices[i].text;
            choiceButtons[i].interactable = true;
        }

        UpdateDots(index, total);
        if (thinkingIndicator) thinkingIndicator.SetActive(false);

        questionPanel.blocksRaycasts = true;
        questionPanel.interactable = true;
        StartCoroutine(FadeTo(questionPanel, 1f));
    }

    public void HideChoices()
    {
        foreach (var btn in choiceButtons) btn.interactable = false;
    }

    public void HideResultPanel()
    {
        SetInstant(resultPanel, false);
    }

    public void ShowThinkingDots(bool show)
    {
        if (thinkingIndicator) thinkingIndicator.SetActive(show);
    }

    public void FadeOutQuestion(Action onComplete)
    {
        StartCoroutine(FadeOutThen(questionPanel, onComplete));
    }

    public void ShowResultScreen(int score, int maxScore)
    {
        SetInstant(questionPanel, false);
        if (thinkingIndicator) thinkingIndicator.SetActive(false);

        float pct = (float)score / maxScore;
        resultScoreText.text = $"{score} / {maxScore} Rating: ";
        resultGradeText.text =
            pct >= 0.8f ? "Ex - It's seem your skill are good to go!" :
            pct >= 0.6f ? "Mid — Your skill are enough although you need to learn the workflow through time!" :
            pct >= 0.4f ? "Fine — Your skill are avarege, we need to train you for a period of time after you join our company!" :
                          "... - You have learn the basics but our position need more than that, thanks for your time!";

        resultPanel.blocksRaycasts = true;
        resultPanel.interactable = true;
        StartCoroutine(FadeTo(resultPanel, 1f));
    }

    private void UpdateDots(int current, int total)
    {
        for (int i = 0; i < progressDots.Length; i++)
            progressDots[i].color =
                i < current ? colorDone :
                i == current ? colorCurrent :
                               colorPending;
    }

    private void SetInstant(CanvasGroup cg, bool visible)
    {
        if (!cg) return;
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    private IEnumerator FadeTo(CanvasGroup cg, float target, Action onDone = null)
    {
        float t = 0f, start = cg.alpha;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        cg.alpha = target;
        onDone?.Invoke();
    }

    private IEnumerator FadeOutThen(CanvasGroup cg, Action onDone)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;
        yield return FadeTo(cg, 0f);
        onDone?.Invoke();
    }
}