using UnityEngine;
using UnityEngine.Events;

public class AssistantManager : MonoBehaviour
{
    [SerializeField] private ChatClient chatClient;
    [SerializeField] private VoiceManager voiceManager;
    [SerializeField] private AudioSource waitingAudioSource;

    public UnityEvent<TranslationResponse> _onTranslationReceived;
    public UnityEvent<string> _onResponseReceived;
    public UnityEvent<string> _onResponseFailed;

    private void Start()
    {
        voiceManager.onResponseReceived.AddListener(GenerateMessage);
        chatClient.onRequestCompleted.AddListener(OnRequestCompleted);
        chatClient.onRequestFailed.AddListener(OnRequestFailed);
    }

    private void GenerateMessage(string message)
    {
        waitingAudioSource.Play();
        chatClient.SendRequest(message);
    }

    private void OnRequestCompleted(string arg0)
    {
        waitingAudioSource.Stop();
        var translationResult = JsonUtility.FromJson<TranslationResponse>(arg0);
        Debug.Log("Teacher's message: " + translationResult.teacher_message);
        Debug.Log("Pronunciation: " + translationResult.pronunciation);
        Debug.Log("Is Translation: " + translationResult.isTranslation);
        Debug.Log("Kanji: " + translationResult.kanji);
        Debug.Log("Hiragana: " + translationResult.hiragana);
        Debug.Log("Katakana: " + translationResult.katakana);
        voiceManager.Speak(translationResult.teacher_message);
        if (translationResult.isTranslation)
        {
            _onTranslationReceived.Invoke(translationResult);
            return;
        }
        _onResponseReceived.Invoke(translationResult.teacher_message);
    }

    private void OnRequestFailed(string arg0)
    {
        waitingAudioSource.Stop();
        voiceManager.Speak("Looks there was an error, please try again.");
        _onResponseFailed.Invoke("Looks there was an error, please try again.");
    }
}