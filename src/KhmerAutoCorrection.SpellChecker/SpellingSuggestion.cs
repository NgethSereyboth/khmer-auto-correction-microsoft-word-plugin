using System;
using System.Collections.Generic;

namespace KhmerAutoCorrection.SpellChecker;

/// <summary>
/// Represents a spelling suggestion with its distance score and frequency.
/// </summary>
public sealed class SpellingSuggestion
{
    public SpellingSuggestion(string term, int distance, long frequency = 0)
    {
        Term = term ?? throw new ArgumentNullException(nameof(term));
        Distance = distance;
        Frequency = frequency;
    }

    /// <summary>
    /// The suggested correction term.
    /// </summary>
    public string Term { get; }

    /// <summary>
    /// The edit distance from the original misspelled word.
    /// </summary>
    public int Distance { get; }

    /// <summary>
    /// The frequency of the suggestion in the dictionary (if available).
    /// </summary>
    public long Frequency { get; }
}
