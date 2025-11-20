using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "TranslationTable", menuName = "Scriptable Objects/TranslationTable")]
public class TranslationTable : ScriptableObject
{
    public TranslationEntry[] entries;
}
[System.Serializable]
public struct TranslationEntry {
    public string sourceText;
    /// <summary>
    /// Used for words that doesn't require translation. This is only for silencing validation. It's completely ignored otherwise.
    /// </summary>
    public bool translationNotRequired;
    public TranslatedString translation;
}
