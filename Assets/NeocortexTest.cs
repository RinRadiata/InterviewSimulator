using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NeocortexTest : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField userInput;
    public TMP_Text aiResponseText;
    public Button sendButton;

    // Replace with your actual API endpoint and key
    private string apiUrl = "https://api.neocortex.ai/v1/chat";
    private string apiKey = "YOUR_API_KEY_HERE"; 

    private HttpClient httpClient;

    void Start()
    {
        // Initialize HTTP client
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        aiResponseText.text = "AI: Hello! I'm your interviewer. Ready to begin?";
        sendButton.onClick.AddListener(OnSendButtonPressed);
    }

    public async void OnSendButtonPressed()
    {
        string playerMessage = userInput.text.Trim();

        if (string.IsNullOrEmpty(playerMessage))
            return;

        aiResponseText.text += $"\n\nYou: {playerMessage}";
        userInput.text = "";
        aiResponseText.text += "\nAI: (thinking...)";

        string response = await GetAIResponse(playerMessage);

        // Replace placeholder "(thinking...)" with AI reply
        aiResponseText.text = aiResponseText.text.Replace("(thinking...)", response);
    }

    private async Task<string> GetAIResponse(string userMessage)
    {
        try
        {
            var jsonData = new
            {
                prompt = userMessage,
                max_tokens = 200
            };

            string jsonBody = JsonUtility.ToJson(jsonData);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                // You may need to adjust parsing based on actual Neocortex API JSON format
                return ParseNeocortexResponse(json);
            }
            else
            {
                return $"(Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()})";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            return "(Network error: Unable to reach Neocortex API)";
        }
    }

    private string ParseNeocortexResponse(string json)
    {
        // 🔧 Example parsing – adjust this if Neocortex returns a different format
        // Example expected JSON: { "reply": "Hello there!" }
        try
        {
            NeocortexResponse data = JsonUtility.FromJson<NeocortexResponse>(json);
            return data.reply ?? "(No response)";
        }
        catch
        {
            return "(Invalid response format)";
        }
    }

    [Serializable]
    private class NeocortexResponse
    {
        public string reply;
    }
}
