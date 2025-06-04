using System.Collections;
using System.Linq;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class VoiceManager : MonoBehaviour
{
    [Header("Voice Settings")] [SerializeField]
    private AppVoiceExperience appVoiceExperience;

    [SerializeField] private WitResponseMatcher witResponseMatcher;
    [SerializeField] private TextMeshProUGUI transcriptText;
    [SerializeField] private TTSSpeaker ttsSpeaker;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float conversationActiveTimeout;

    private bool _isWakeWordDetected;
    private string[] _actualWakeWords;
    private Coroutine _conversationCoroutine;

    public UnityEvent onWakeWordDetected;
    public UnityEvent<string> onResponseReceived;

    private void Start()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoiceExperience);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscript);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscript);
        ttsSpeaker.Events.OnPlaybackQueueComplete.AddListener(ReactivateConversation);
        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(witResponseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            Debug.Log("Listening to wake word events");
            onMultiValueEvent.AddListener(WakeWordDetected);
        }

        appVoiceExperience.Activate();
        Debug.Log("Activating voice experience");
    }

    private void WakeWordDetected(string[] wakeWords)
    {
        Debug.Log("Wake word detected: " + string.Join(", ", wakeWords));
        if (wakeWords.Length == 0) return;
        _isWakeWordDetected = true;
        _actualWakeWords = wakeWords;
        audioSource.Play();
        onWakeWordDetected?.Invoke();
    }

    private void OnPartialTranscript(string transcript)
    {
        if (!_isWakeWordDetected) return;
        transcriptText.text = transcript;
    }

    private void OnFullTranscript(string arg0)
    {
        if (!_isWakeWordDetected || _actualWakeWords.Contains(arg0)) return;
        _isWakeWordDetected = false;
        onResponseReceived?.Invoke(arg0);
    }

    private void ReactivateConversation()
    {
        Debug.Log("Reactivating voice experience with grace period");
        _isWakeWordDetected = true;
        if (_conversationCoroutine != null) StopCoroutine(_conversationCoroutine);
        _conversationCoroutine = StartCoroutine(ActivateConversationCountdown());
    }

    private IEnumerator ActivateConversationCountdown()
    {
        yield return new WaitForSeconds(conversationActiveTimeout);
        _isWakeWordDetected = false;
        Debug.Log("Conversartion ended; wake word now required again.");
    }
    
    private void ReactivateVoiceExperience() => appVoiceExperience.Activate();

    public void Speak(string text)
    {
        ttsSpeaker.SpeakQueued(text);
    }
}