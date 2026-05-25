using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InterviewManager : MonoBehaviour
{
    [Header("Question Pool")]
    [SerializeField] private List<InterviewQuestionData> questionPool;
    [SerializeField] private int questionsPerSession = 10;

    [Header("References (scene)")]
    [SerializeField] private InterviewUIController uiController;
    [SerializeField] private NPCThoughtController npcThought;
    [SerializeField] private CreditScreenController creditScreen;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [HideInInspector] public UnityEvent OnInterviewStarted = new UnityEvent();
    [HideInInspector] public UnityEvent<int, int> OnInterviewFinished = new UnityEvent<int, int>();

    private List<InterviewQuestionData> _session;
    private int _currentIndex;
    private int _totalScore;
    private bool _isWaiting;

    private void Start()
    {
        if (creditScreen != null)
            creditScreen.OnCreditFinished.AddListener(EnablePlayer);
        else
            EnablePlayer();
    }

    private void EnablePlayer()
    {
        if (playerMovement != null) playerMovement.SetInterviewMode(false);
    }

    public void StartInterview()
    {
        _session = Shuffle(questionPool)
                    .GetRange(0, Mathf.Min(questionsPerSession, questionPool.Count));
        _currentIndex = 0;
        _totalScore = 0;
        _isWaiting = false;

        if (playerMovement != null) playerMovement.SetInterviewMode(true);

        OnInterviewStarted?.Invoke();
        ShowCurrentQuestion();
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        if (_isWaiting) return;
        if (_currentIndex >= _session.Count) return;

        _isWaiting = true;
        var q = _session[_currentIndex];
        var chosen = q.choices[choiceIndex];
        _totalScore += chosen.score;

        uiController.HideChoices();
        uiController.ShowThinkingDots(true);

        npcThought.ShowThought(
            q.question,
            chosen.text,
            chosen.feedbackHint,
            chosen.score,
            onDone: () => StartCoroutine(NextQuestionDelay())
        );
    }

    public void RestartInterview()
    {
        if (creditScreen != null) creditScreen.HideImmediate();

        uiController.HideResultPanel();

        StartInterview();
    }

    public void ExitInterview()
    {
        if (playerMovement != null) playerMovement.SetInterviewMode(false);
        Application.Quit();
    }

    private void ShowCurrentQuestion()
    {
        if (_currentIndex >= _session.Count) { FinishInterview(); return; }
        uiController.DisplayQuestion(_session[_currentIndex], _currentIndex, _session.Count);
    }

    private IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(0.8f);
        _currentIndex++;
        uiController.FadeOutQuestion(() =>
        {
            _isWaiting = false;
            ShowCurrentQuestion();
        });
    }

    private void FinishInterview()
    {
        int maxScore = _session.Count * 3;
        uiController.ShowResultScreen(_totalScore, maxScore);
        npcThought.ShowFinalThought(_totalScore, maxScore);
        OnInterviewFinished?.Invoke(_totalScore, maxScore);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private static List<T> Shuffle<T>(List<T> src)
    {
        var list = new List<T>(src);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}