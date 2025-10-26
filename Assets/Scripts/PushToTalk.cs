using UnityEngine;
using Neocortex; // Make sure this matches your SDK's namespace

public class PushToTalk : MonoBehaviour
{
    [Header("Neocortex References")]
    public NeocortexAudioReceiver audioReceiver;
    public NeocortexSmartAgent smartAgent;

    [Header("Settings")]
    public KeyCode pushToTalkKey = KeyCode.T; // change to any key
    public bool logDebug = false;

    private bool isRecording = false;

    void Start()
    {
        // Auto-assign if left empty
        if (audioReceiver == null)
            audioReceiver = FindObjectOfType<NeocortexAudioReceiver>();

        if (smartAgent == null)
            smartAgent = FindObjectOfType<NeocortexSmartAgent>();

        if (audioReceiver == null || smartAgent == null)
            Debug.LogError("⚠️ Missing NeocortexAudioReceiver or NeocortexSmartAgent reference!");

        // Subscribe to when audio is done recording
        audioReceiver.OnAudioRecorded.AddListener(OnAudioRecorded);
    }

    void OnDestroy()
    {
        if (audioReceiver != null)
            audioReceiver.OnAudioRecorded.RemoveListener(OnAudioRecorded);
    }

    void Update()
    {
        // Start recording when key pressed down
        if (Input.GetKeyDown(pushToTalkKey))
        {
            StartRecording();
        }

        // Stop recording and send when key released
        if (Input.GetKeyUp(pushToTalkKey))
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (isRecording) return;
        isRecording = true;

        if (logDebug) Debug.Log("🎙️ Recording started (keyboard).");

        audioReceiver.StartMicrophone();
    }

    private void StopRecording()
    {
        if (!isRecording) return;
        isRecording = false;

        if (logDebug) Debug.Log("🧠 Recording stopped. Sending to Neocortex...");

        audioReceiver.StopMicrophone(); // will trigger OnAudioRecorded
    }

    private void OnAudioRecorded(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("No audio recorded!");
            return;
        }

        if (smartAgent != null)
        {
            smartAgent.AudioToText(clip);
            if (logDebug) Debug.Log("📤 Sent audio to Neocortex.");
        }
        else
        {
            Debug.LogError("NeocortexSmartAgent not assigned.");
        }
    }
}
