using System;
using System.Collections.Generic;
using System.Linq;
using KhmerAutoCorrection.Core;

namespace KhmerAutoCorrection.SpellChecker;

/// <summary>
/// Khmer spell checker using a custom implementation with edit distance and frequency-based ranking.
/// This provides suggestions for misspelled Khmer words without external dependencies.
/// </summary>
public sealed class KhmerSpellChecker : IDisposable
{
    private readonly KhmerDictionary _dictionary;
    private readonly int _maxEditDistance;
    private readonly int _maxSuggestions;
    private readonly List<string> _dictionaryWords;

    /// <summary>
    /// Creates a new Khmer spell checker.
    /// </summary>
    /// <param name="dictionary">The Khmer dictionary to use.</param>
    /// <param name="maxEditDistance">Maximum edit distance for suggestions (default: 3).</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return (default: 10).</param>
    public KhmerSpellChecker(KhmerDictionary dictionary, int maxEditDistance = 3, int maxSuggestions = 10)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _maxEditDistance = maxEditDistance;
        _maxSuggestions = maxSuggestions;
        _dictionaryWords = dictionary.GetWords().ToList();
    }

    /// <summary>
    /// Checks if a word is spelled correctly.
    /// </summary>
    public bool IsCorrect(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return true;

        return _dictionary.Contains(word);
    }

    /// <summary>
    /// Gets spelling suggestions for a misspelled word.
    /// Suggestions are ranked using Khmer-specific edit distance and frequency.
    /// </summary>
    /// <param name="misspelledWord">The misspelled word.</param>
    /// <returns>List of suggestions ordered by relevance.</returns>
    public IReadOnlyList<SpellingSuggestion> GetSuggestions(string misspelledWord)
    {
        if (string.IsNullOrWhiteSpace(misspelledWord))
            return Array.Empty<SpellingSuggestion>();

        // If the word is correct, return empty list
        if (IsCorrect(misspelledWord))
            return Array.Empty<SpellingSuggestion>();

        // Find candidates within max edit distance
        var candidates = new List<(string term, int distance, long frequency)>();
        
        foreach (var word in _dictionaryWords)
        {
            // Quick length filter - skip words that are too different in length
            if (Math.Abs(word.Length - misspelledWord.Length) > _maxEditDistance)
                continue;

            int distance = KhmerEditDistance.Compute(misspelledWord, word);
            
            if (distance <= _maxEditDistance)
            {
                long freq = _dictionary.GetFrequency(word);
                candidates.Add((word, distance, freq));
            }
        }

        // Rank by edit distance first, then by frequency
        var suggestions = candidates
            .OrderBy(c => c.distance)
            .ThenByDescending(c => c.frequency)
            .Take(_maxSuggestions)
            .Select(c => new SpellingSuggestion(c.term, c.distance, c.frequency))
            .ToList();

        return suggestions;
    }

    /// <summary>
    /// Gets the best suggestion for a misspelled word.
    /// </summary>
    public SpellingSuggestion? GetBestSuggestion(string misspelledWord)
    {
        var suggestions = GetSuggestions(misspelledWord);
        return suggestions.FirstOrDefault();
    }

    public void Dispose()
    {
        // No unmanaged resources to dispose
    }
}
