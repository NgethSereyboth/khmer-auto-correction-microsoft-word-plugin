using System;
using System.Collections.Generic;
using System.Linq;
using KhmerAutoCorrection.Core;
using KhmerAutoCorrection.SpellChecker;

namespace KhmerAutoCorrection.WordAddIn;

/// <summary>
/// Core spell checking engine that combines segmentation and spell checking.
/// This class is designed to be used by the VSTO add-in to check text and get errors.
/// </summary>
public sealed class SpellCheckEngine : IDisposable
{
    private readonly KhmerDictionary _dictionary;
    private readonly KhmerSegmenter _segmenter;
    private readonly KhmerSpellChecker _spellChecker;

    public SpellCheckEngine(KhmerDictionary dictionary, int maxEditDistance = 3, int maxSuggestions = 10)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _segmenter = new KhmerSegmenter(dictionary);
        _spellChecker = new KhmerSpellChecker(dictionary, maxEditDistance, maxSuggestions);
    }

    /// <summary>
    /// Checks the given text for spelling errors and returns a list of errors with suggestions.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns>List of spelling errors found in the text.</returns>
    public IReadOnlyList<SpellingError> CheckText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<SpellingError>();

        var errors = new List<SpellingError>();
        
        // Segment the text into tokens
        var tokens = _segmenter.Segment(text);
        
        foreach (var token in tokens)
        {
            // Check all Khmer tokens - both known and unknown
            // For known tokens, we skip them (no error)
            // For unknown tokens that are Khmer text, we flag them as errors
            if (IsKhmerText(token.Value) && !token.IsKnown)
            {
                var suggestions = _spellChecker.GetSuggestions(token.Value);
                var suggestionTerms = suggestions.Select(s => s.Term).ToList();
                
                errors.Add(new SpellingError(
                    token.Start,
                    token.End - token.Start,
                    token.Value,
                    suggestionTerms
                ));
            }
        }
        
        return errors;
    }

    /// <summary>
    /// Checks the given text for spelling errors using a more aggressive approach.
    /// This method checks every Khmer word segment, even if it's a valid sub-word.
    /// Useful for catching misspellings that get segmented incorrectly.
    /// </summary>
    public IReadOnlyList<SpellingError> CheckTextAggressive(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<SpellingError>();

        var errors = new List<SpellingError>();
        
        // Segment the text into tokens
        var tokens = _segmenter.Segment(text);
        
        // Group consecutive unknown tokens to check them together
        int i = 0;
        while (i < tokens.Count)
        {
            var token = tokens[i];
            
            if (IsKhmerText(token.Value) && !token.IsKnown)
            {
                // Try to group with next unknown tokens
                int start = token.Start;
                int end = token.End;
                string combinedWord = token.Value;
                int lastIdx = i;
                
                // Look ahead to combine consecutive unknown Khmer tokens
                for (int j = i + 1; j < tokens.Count; j++)
                {
                    var nextToken = tokens[j];
                    if (IsKhmerText(nextToken.Value) && !nextToken.IsKnown)
                    {
                        combinedWord += nextToken.Value;
                        end = nextToken.End;
                        lastIdx = j;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // Check the combined word
                if (!_dictionary.Contains(combinedWord))
                {
                    var suggestions = _spellChecker.GetSuggestions(combinedWord);
                    var suggestionTerms = suggestions.Select(s => s.Term).ToList();
                    
                    // If no suggestions for combined word, try individual tokens
                    if (suggestionTerms.Count == 0 && start != end)
                    {
                        suggestions = _spellChecker.GetSuggestions(token.Value);
                        suggestionTerms = suggestions.Select(s => s.Term).ToList();
                        end = token.End;
                        combinedWord = token.Value;
                        lastIdx = i;
                    }
                    
                    if (suggestionTerms.Count > 0 || !_dictionary.Contains(combinedWord))
                    {
                        errors.Add(new SpellingError(
                            start,
                            end - start,
                            combinedWord,
                            suggestionTerms
                        ));
                    }
                }
                
                i = lastIdx + 1;
            }
            else if (IsKhmerText(token.Value) && token.IsKnown)
            {
                // Even known tokens should be checked in context
                // Look ahead to see if combining with next known token creates an unknown sequence
                int start = token.Start;
                int end = token.End;
                string combinedWord = token.Value;
                int lastIdx = i;
                
                // Look ahead to combine consecutive known Khmer tokens
                for (int j = i + 1; j < tokens.Count; j++)
                {
                    var nextToken = tokens[j];
                    if (IsKhmerText(nextToken.Value) && nextToken.IsKnown)
                    {
                        string testCombined = combinedWord + nextToken.Value;
                        // If the combined word is not in dictionary, we might have found a boundary issue
                        if (!_dictionary.Contains(testCombined))
                        {
                            // Don't extend further, but check if current combination is suspicious
                            break;
                        }
                        combinedWord = testCombined;
                        end = nextToken.End;
                        lastIdx = j;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // Now check if there's an unknown token right after this known sequence
                if (lastIdx + 1 < tokens.Count)
                {
                    var nextToken = tokens[lastIdx + 1];
                    if (IsKhmerText(nextToken.Value) && !nextToken.IsKnown)
                    {
                        // Combine the known sequence with the unknown token
                        string fullCombined = combinedWord + nextToken.Value;
                        if (!_dictionary.Contains(fullCombined))
                        {
                            var suggestions = _spellChecker.GetSuggestions(fullCombined);
                            var suggestionTerms = suggestions.Select(s => s.Term).ToList();
                            
                            if (suggestionTerms.Count > 0)
                            {
                                errors.Add(new SpellingError(
                                    start,
                                    nextToken.End - start,
                                    fullCombined,
                                    suggestionTerms
                                ));
                                i = lastIdx + 2;
                                continue;
                            }
                        }
                    }
                }
                
                i++;
            }
            else
            {
                i++;
            }
        }
        
        return errors;
    }

    /// <summary>
    /// Checks if a word is spelled correctly.
    /// </summary>
    public bool IsWordCorrect(string word)
    {
        return _spellChecker.IsCorrect(word);
    }

    /// <summary>
    /// Gets spelling suggestions for a misspelled word.
    /// </summary>
    public IReadOnlyList<string> GetSuggestions(string word)
    {
        var suggestions = _spellChecker.GetSuggestions(word);
        return suggestions.Select(s => s.Term).ToList();
    }

    /// <summary>
    /// Determines if the given text contains Khmer characters.
    /// Only returns true for actual Khmer words (not punctuation or digits).
    /// </summary>
    private static bool IsKhmerText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
            
        // Check for Khmer Unicode range (U+1780 to U+17FF)
        // Also exclude pure punctuation and digit sequences
        bool hasKhmerLetter = false;
        foreach (char c in text)
        {
            // Khmer letters (consonants, vowels, dependent vowels)
            if ((c >= '\u1780' && c <= '\u17B3') ||  // Consonants
                (c >= '\u17B6' && c <= '\u17C8') ||  // Vowels and signs
                (c >= '\u17CA' && c <= '\u17D3'))    // Various signs
            {
                hasKhmerLetter = true;
            }
            // If we have a non-Khmer, non-punctuation character, might not be Khmer word
            else if (c < '\u1780' || c > '\u17FF')
            {
                // Allow some combining marks but skip pure Latin/digits
            }
        }
        
        // Must have at least one Khmer letter to be considered Khmer text
        return hasKhmerLetter;
    }

    public void Dispose()
    {
        _spellChecker?.Dispose();
    }
}
