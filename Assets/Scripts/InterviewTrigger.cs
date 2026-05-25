using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class InterviewTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InterviewManager interviewManager;

    [Header("Prompt UI — text 'Press E to start'")]
    [SerializeField] private GameObject promptUI;

    private bool _playerInRange = false;
    private bool _hasStarted = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (promptUI) promptUI.SetActive(false);

        interviewManager.OnInterviewFinished.AddListener((s, m) => _hasStarted = false);
    }

    private void Update()
    {
        if (_playerInRange && !_hasStarted && Input.GetKeyDown(KeyCode.E))
        {
            _hasStarted = true;
            if (promptUI) promptUI.SetActive(false);
            interviewManager.StartInterview();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        if (promptUI && !_hasStarted) promptUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        if (promptUI) promptUI.SetActive(false);
    }
}