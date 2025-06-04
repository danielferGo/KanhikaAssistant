using TMPro;
using UnityEngine;

public class TranslationManager : MonoBehaviour
{
    [SerializeField] private AssistantManager assistantManager;
    [SerializeField] private TextMeshProUGUI translationText;
    [SerializeField] private TextMeshProUGUI kanjiText;
    [SerializeField] private TextMeshProUGUI hiraganaText;
    [SerializeField] private TextMeshProUGUI katakanaText;

    [SerializeField] private GameObject hiraganaCanvas;
    [SerializeField] private GameObject katakanaCanvas;
    [SerializeField] private GameObject kanjiCanvas;

    private void Start()
    {
        assistantManager._onTranslationReceived.AddListener(UpdateTranslation);
        // var translationResponse = new TranslationResponse();
        // translationResponse.hiragana = "ひらがな";
        // translationResponse.katakana = "カタカナ";
        // translationResponse.kanji = "漢字";
        // translationResponse.pronunciation = "Konichiwsa";
        // translationResponse.teacher_message = "This is a test message.";
        // translationResponse.isTranslation = true;
        // UpdateTranslation(translationResponse);
        ChangeCanvasVisibility(0);
    }

    private void UpdateTranslation(TranslationResponse response)
    {
        translationText.text = "HIRAGANA: " + response.hiragana + "\n" +
                               "KATAKANA: " + response.katakana + "\n" +
                               "KANJI: " + response.kanji + "\n" +
                               "PRONUNCIATION: " + response.pronunciation;
        kanjiText.text = response.kanji;
        hiraganaText.text = response.hiragana;
        katakanaText.text = response.katakana;
    }

    public void ChangeCanvasVisibility(int canvasVisibility)
    {
        switch (canvasVisibility)
        {
            case 0:
                hiraganaCanvas.gameObject.SetActive(true);
                katakanaCanvas.gameObject.SetActive(false);
                kanjiCanvas.gameObject.SetActive(false);
                break;
            case 1:
                hiraganaCanvas.gameObject.SetActive(false);
                katakanaCanvas.gameObject.SetActive(true);
                kanjiCanvas.gameObject.SetActive(false);
                break;
            case 2:
                hiraganaCanvas.gameObject.SetActive(false);
                katakanaCanvas.gameObject.SetActive(false);
                kanjiCanvas.gameObject.SetActive(true);
                break;
        }
    }
}