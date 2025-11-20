using System;
using UnityEngine;
using TMPro;

/// <summary>
/// </summary>
public class TMP_TextAutoTranslate : MonoBehaviour
{
    string initialText;
    TMP_Text textComponent; 

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent == null || LanguageManager.Instance == null)
        {
            if (textComponent == null) Debug.LogWarning($"[TMP_TextAutoTranslate] Tidak ada komponen TMP_Text di {gameObject.name}");
            if (LanguageManager.Instance == null) Debug.LogWarning($"[TMP_TextAutoTranslate] LanguageManager.Instance not found.");
            return;
        }

        // initialText adalah "Kunci" (misal: "PLAY_BUTTON" atau "SETTINGS")
        initialText = textComponent.text; 

        LanguageManager.Instance.onLanguageChanged.AddListener(UpdateText);

        UpdateText();
    }

    /// <summary>
    /// Dipanggil jika Anda ingin mengubah kunci teks secara dinamis
    /// </summary>
    public void SetText(string newTextKey) 
    {
        initialText = newTextKey;
        UpdateText();
    }

    void UpdateText() 
    {
        if (textComponent == null) return;
        if (initialText == String.Empty) return;

        textComponent.text = LanguageManager.Instance.GetTranslation(initialText, gameObject);
    }

    // Pastikan untuk Unsubscribe saat object hancur
    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.onLanguageChanged.RemoveListener(UpdateText);
        }
    }
}