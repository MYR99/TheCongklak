using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent onLanguageChanged;
    public static LanguageManager Instance {get; private set;}
    SystemLanguage currentLanguage = SystemLanguage.Unknown;
    [Header("Database")]
    [SerializeField]
    TranslationTable[] translationTables;
    readonly Dictionary<string, (int, int)> index = new();
    [Header("Interface")]
    [SerializeField]
    private Text localeIndicator;
    [SerializeField]
    private Button localeButton;
    [SerializeField]
    string localeButtonPrefix = "LANG: ";

    // Missing translation report
    readonly Dictionary<string,int> missingKeys = new();
    [Header("Reports")]
    [SerializeField]
    List<MissingTranslationReport> missingTranslationReports = new List<MissingTranslationReport>();

    // Untranslated Report
    [SerializeField]
    List<UntranslatedReport> mainLanguageUntranslatedReports = new List<UntranslatedReport>();
    [SerializeField]
    List<UntranslatedReport> optionalLanguageUntranslatedReports = new List<UntranslatedReport>();

    const string CURRENT_LANG_KEY = "current_language";
    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        else {
            Instance = this;
        }
        Assert.IsNotNull(localeIndicator);
    }
    void GenerateIndex() {
        missingKeys.Clear();
        missingTranslationReports.Clear();

        for (int i = 0; i < translationTables.Length; i++) {
            var table = translationTables[i];
            if (table == null) continue;
            for (int j = 0; j < table.entries.Length; j++) {
                var entry = table.entries[j];
                GenerateIndexEntry(entry, i, j, table);
            }
        }
    }
    /// <summary>
    /// Function extracted from GenerateIndexEntry()
    /// </summary>
    void GenerateIndexEntry(TranslationEntry entry, int i, int j, TranslationTable table) {
        if (index.ContainsKey(entry.sourceText)) {
            Debug.LogError($"Duplicate entry for '{entry.sourceText}' found in {table}[{j}] and {translationTables[index[entry.sourceText].Item1]}[{index[entry.sourceText].Item2}]", this);
            return;
        }
        index[entry.sourceText] = (i,j);
        if (!entry.translationNotRequired) {
            switch (entry.translation.IsFilled()) {
                case TranslatedString.FillStatus.NoPrimaryLanguage:
                    UntranslatedReport report = new() {text = entry.sourceText, index = j, table = table};
                    if (mainLanguageUntranslatedReports.Count == 0) Debug.LogError("Some language doesn't have primary translation, please check LanguageManager for reports.",this);
                    mainLanguageUntranslatedReports.Add(report);
                    break;
                case TranslatedString.FillStatus.OptionalLanguagesIsNotFilled:
                    UntranslatedReport reportx = new() {text = entry.sourceText, index = j, table = table};
                    if (optionalLanguageUntranslatedReports.Count == 0) Debug.LogError("Some language doesn't have optional translation, please check LanguageManager for reports.",this);
                    optionalLanguageUntranslatedReports.Add(reportx);
                    break;
            }
        }
        
    }
    void Start()
    {
        if (localeButton != null) localeButton.onClick.AddListener(SetCurrentLanguageToNext);

        // Load overrided language
        if (PlayerPrefs.HasKey(CURRENT_LANG_KEY)) {
            var config = PlayerPrefs.GetString(CURRENT_LANG_KEY);
            foreach (SystemLanguage lang in TranslatedString.SUPPORTED_LANGUAGES) {
                if (lang.ToString() == config) {
                    currentLanguage = lang;
                    break;
                }
            }
        }
        
        GenerateIndex();

        // Update indicator for the first time.
        UpdateLanguageIndicator();
    }
    /// <summary>
    /// Translate text.
    /// </summary>
    /// <param name="sourceText">Text to translate</param>
    /// <param name="requester">GameObject that request the translation, will be used for reporting missing translation entry.</param>
    /// <returns>Return the translated text. If translated text doesn't exist, it will return sourceText</returns>
    public string GetTranslation(string sourceText, GameObject requester) {
        if (index.ContainsKey(sourceText)) {
            var tup = index[sourceText];
            var translation = translationTables[tup.Item1].entries[tup.Item2].translation.GetString();
            if (translation == String.Empty) return sourceText;
            else return translation;
        }
        else {
            AddMissingReport(sourceText, requester);
            return sourceText;
        }
    }
    public void SetCurrentLanguage(SystemLanguage newLanguage = SystemLanguage.Unknown) {
        if (currentLanguage == newLanguage) return; // language is the same as current
        if (!TranslatedString.SUPPORTED_LANGUAGES.Contains(newLanguage)) return; // language not supported
        currentLanguage = newLanguage;

        if (newLanguage == SystemLanguage.Unknown) PlayerPrefs.DeleteKey(CURRENT_LANG_KEY);
        else PlayerPrefs.SetString(CURRENT_LANG_KEY, newLanguage.ToString());
        
        onLanguageChanged.Invoke();
        UpdateLanguageIndicator();
    }
    public void SetCurrentLanguageToNext() {
        var length = TranslatedString.SUPPORTED_LANGUAGES.Length;
        var languages = TranslatedString.SUPPORTED_LANGUAGES;
        for (int i = 0; i < length; i++) {
            if (currentLanguage == languages[i]) {
                if (length-1 == i) SetCurrentLanguage(languages[0]);
                else SetCurrentLanguage(languages[i+1]);
                break;
            }
        }
    }
    void UpdateLanguageIndicator() {
        if (currentLanguage == SystemLanguage.Unknown) {
            localeIndicator.text = localeButtonPrefix+"Auto("+GetCurrentLanguage().ToString()+")";
        } else {
        localeIndicator.text = localeButtonPrefix+GetCurrentLanguage().ToString();
        }
    }
    public SystemLanguage GetCurrentLanguage() {
        if (currentLanguage == SystemLanguage.Unknown) {
            if (TranslatedString.SUPPORTED_LANGUAGES.Contains(Application.systemLanguage)) return Application.systemLanguage;
            else return TranslatedString.PRIMARY_LANGUAGE;
        }
        else {
            return currentLanguage;
        }
    }
    void AddMissingReport(string text, GameObject requester) {
        if (missingKeys.Count == 0) {
            Debug.LogError("Some TranslationKey are missing, please see LanguageManager instance in the inspector to see report.",this);
        }
        if (!missingKeys.ContainsKey(text)) {
            missingKeys[text] = missingTranslationReports.Count;
            missingTranslationReports.Add(new MissingTranslationReport() {text = text});
        }
        var requesters = missingTranslationReports[missingKeys[text]].requesters;
        requesters.RemoveAll((GameObject obj) => obj == null);
        requesters.Add(requester);
    }
}
[System.Serializable]
/// <summary>
/// To add new language, add the variable to each section marked with roman numerals. Like "I. Language Lists",
/// "II. Language Checker", and "III. Translator". The code should be self explanatory.
/// </summary>
public struct TranslatedString {
    /// <summary>
    /// NoPrimaryLanguage = fallback language string hasn't even been filled.
    /// OptionalLanguagesIsNotFilled = fallback language string is filled, but the rest of the languages is not.
    /// Filled = filled correctly.
    /// </summary>
    public enum FillStatus {NoPrimaryLanguage, OptionalLanguagesIsNotFilled, Filled}
    public const SystemLanguage PRIMARY_LANGUAGE = SystemLanguage.English;
    const int MAX_LINES = 3;

    // I. Language Lists.
    // Add the language to the supported languages, and then the variables.
    // thats TWO
    public static readonly SystemLanguage[] SUPPORTED_LANGUAGES = {
        SystemLanguage.Unknown,
        SystemLanguage.English,
        SystemLanguage.French,
        SystemLanguage.Italian,
        SystemLanguage.German,
        SystemLanguage.Spanish,
        SystemLanguage.Indonesian,
        SystemLanguage.Portuguese, // This is for Brazilian.
    };
    [TextArea(1,MAX_LINES), SerializeField] string english;
    [TextArea(1,MAX_LINES), SerializeField] string french;
    [TextArea(1,MAX_LINES), SerializeField] string italian;
    [TextArea(1,MAX_LINES), SerializeField] string german;
    [TextArea(1,MAX_LINES), SerializeField] string spanish;
    [TextArea(1,MAX_LINES), SerializeField] string indonesian;
    [TextArea(1,MAX_LINES), SerializeField] string portugese;

    /// <param name="identifier">Identifier for Debug.Log warning and error in case you haven't set string in current language.</param>
    /// <returns>String in current language or fallback if it doesn't exist.</returns>
    public string GetString() {
        string translated = Translate(LanguageManager.Instance.GetCurrentLanguage());
        if (translated == string.Empty) {
            translated = Translate(PRIMARY_LANGUAGE);
        }
        return translated;
    }

    // II. Language Checker
    /// <summary>
    /// Check if developer already filled string for all languages correctly. For testing purposes.
    /// </summary>
    public FillStatus IsFilled() {
        if (english == string.Empty) return FillStatus.NoPrimaryLanguage;
        string[] langs = new string[] { // Add below here
            english, french, italian, german, spanish, indonesian, portugese
        };
        foreach (var lang in langs) {
            if (lang == string.Empty) return FillStatus.OptionalLanguagesIsNotFilled;
        }
        return FillStatus.Filled;
    }

    // III. Translator
    string Translate(SystemLanguage language) {
        return language switch
        {
            SystemLanguage.English => english,
            SystemLanguage.French => french,
            SystemLanguage.Italian => italian,
            SystemLanguage.German => german,
            SystemLanguage.Spanish => spanish,
            SystemLanguage.Indonesian => indonesian,
            SystemLanguage.Portuguese => portugese,
            _ => "",
        };
    }
}

[System.Serializable]
public class MissingTranslationReport {
    public string text;
    public List<GameObject> requesters = new();
}

[System.Serializable]
public class UntranslatedReport {
    public string text;
    public int index;
    public TranslationTable table;
}

