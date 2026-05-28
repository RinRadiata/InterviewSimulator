using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CreditScreenController : MonoBehaviour
{
    [Header("Text references")]
    [SerializeField] private TMP_Text line1Text;
    [SerializeField] private TMP_Text line2Text;
    [SerializeField] private TMP_Text line3Text;
    [SerializeField] private TMP_Text skipHintText;

    [Header("CreditScreen Canvas group")]
    [SerializeField] private CanvasGroup creditCanvasGroup;

    [Header("Player camera")]
    [SerializeField] private GameObject playerCameraObject;

    [Header("Timing")]
    [SerializeField] private float typewriterDelay = 0.045f;
    [SerializeField] private float holdDuration = 5f;
    [SerializeField] private float fadeOutDuration = 1.2f;

    [Header("Content")]
    [SerializeField] private string line1Content = "";
    [SerializeField] private string line2Content = "";
    [SerializeField] private string line3Content = "";

    public UnityEvent OnCreditFinished = new UnityEvent();

    private bool _skipped;

    private void Start()
    {
        Play();
    }

    public void Play()
    {
        _skipped = false;
        gameObject.SetActive(true);
        creditCanvasGroup.alpha = 1f;
        creditCanvasGroup.blocksRaycasts = true;
        creditCanvasGroup.interactable = true;

        if (playerCameraObject != null) playerCameraObject.SetActive(false);
        StartCoroutine(PlayCredit());
    }

    public void HideImmediate()
    {
        StopAllCoroutines();
        creditCanvasGroup.alpha = 0f;
        creditCanvasGroup.blocksRaycasts = false;
        creditCanvasGroup.interactable = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            _skipped = true;
    }

    private IEnumerator PlayCredit()
    {
        line1Text.text = "";
        line2Text.text = "";
        line3Text.text = "";
        skipHintText.text = "(Press Space to skip)";

        yield return TypeWrite(line1Text, line1Content);
        yield return new WaitForSeconds(0.4f);
        yield return TypeWrite(line2Text, line2Content);
        yield return new WaitForSeconds(0.4f);
        yield return TypeWrite(line3Text, line3Content);

        float elapsed = 0f;
        while (elapsed < holdDuration && !_skipped)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

            float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            creditCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        creditCanvasGroup.alpha = 0f;
        creditCanvasGroup.blocksRaycasts = false;
        creditCanvasGroup.interactable = false;
        gameObject.SetActive(false);

        if (playerCameraObject != null) playerCameraObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnCreditFinished?.Invoke();
    }

    private IEnumerator TypeWrite(TMP_Text target, string content)
    {
        target.text = "";
        foreach (char c in content)
        {
            if (_skipped) { target.text = content; yield break; }
            target.text += c;
            yield return new WaitForSeconds(typewriterDelay);
        }
    }
}