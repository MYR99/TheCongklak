using UnityEngine;
using TMPro;

public class LanguagePanelController : MonoBehaviour
{
    [SerializeField] private GameObject languagePanel;
    [SerializeField] private TMP_Text currentLanguageText; // optional

    private void Start()
    {
        UpdateCurrentLanguageText();
    }

    public void ToggleLanguagePanel()
    {
        languagePanel.SetActive(!languagePanel.activeSelf);
    }

    public void SetEnglish()
    {
        LocalizationManager.SetLanguage(LocalizationManager.Language.English);
        AfterLanguageSet();
    }

    public void SetIndonesian()
    {
        LocalizationManager.SetLanguage(LocalizationManager.Language.Indonesian);
        AfterLanguageSet();
    }

    private void AfterLanguageSet()
    {
        UpdateCurrentLanguageText();
        languagePanel.SetActive(false);
    }

    private void UpdateCurrentLanguageText()
    {
        if (currentLanguageText == null) return;

        var lang = LocalizationManager.GetLanguage();
        currentLanguageText.text = lang.ToString();
    }
}