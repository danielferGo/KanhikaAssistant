using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.Events;

public class ChatClient : MonoBehaviour
{
    [SerializeField] private string url = "http://localhost:5005/ask";
    public UnityEvent<string> onRequestCompleted;
    public UnityEvent<string> onRequestFailed;

    public void SendRequest(string message)
    {
        if (message.Equals("")) return;
        var requestData = new MessageRequest(message);
        StartCoroutine(SendRequest(requestData));
    }

    private IEnumerator SendRequest(object requestData)
    {
        var json = JsonUtility.ToJson(requestData);
        Debug.Log("Sending request: " + json);
        var request = new UnityWebRequest(url, "POST");
        var jsonToSend = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            onRequestFailed.Invoke(request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            onRequestCompleted.Invoke(request.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class MessageRequest
    {
        public string message;

        public MessageRequest(string message)
        {
            this.message = message;
        }
    }
}