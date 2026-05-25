using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Interview/Question Data")]
public class InterviewQuestionData : ScriptableObject
{
    [TextArea(2, 4)]
    public string question;
    public ChoiceData[] choices = new ChoiceData[3];
    public string category; // e.g. "C#", "Unity", "Architecture", "URP"
}

[System.Serializable]
public class ChoiceData
{
    [TextArea(2, 3)]
    public string text;
    [Range(1, 3)]
    public int score = 1;
    [Tooltip("1 = weak, 2 = acceptable, 3 = strong")]
    public string feedbackHint; // used in NPC thought prompt
}