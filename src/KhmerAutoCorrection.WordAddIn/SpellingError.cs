using System;
using System.Collections.Generic;
using KhmerAutoCorrection.Core;

namespace KhmerAutoCorrection.WordAddIn;

/// <summary>
/// Represents a spelling error found in the document with its location and suggestions.
/// </summary>
public sealed class SpellingError
{
    public SpellingError(int start, int length, string word, IReadOnlyList<string> suggestions)
    {
        Start = start;
        Length = length;
        Word = word ?? throw new ArgumentNullException(nameof(word));
        Suggestions = suggestions ?? new List<string>();
    }

    /// <summary>
    /// The start index of the misspelled word (relative to the paragraph or checked range).
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// The length of the misspelled word.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The misspelled word text.
    /// </summary>
    public string Word { get; }

    /// <summary>
    /// List of suggested corrections.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; }
}
