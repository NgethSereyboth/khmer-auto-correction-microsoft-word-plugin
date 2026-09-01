using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace KhmerAutoCorrection.Core;

/// <summary>
/// In-memory Khmer lexicon. Dictionary entries are exact Unicode strings and
/// optional frequencies guide Viterbi segmentation.
/// </summary>
public sealed class KhmerDictionary
{
    private readonly HashSet<string> _words;
    private readonly Dictionary<string, long> _frequencies;

    private KhmerDictionary(HashSet<string> words, Dictionary<string, long> frequencies, long totalFrequency, Trie trie)
    {
        _words = words;
        _frequencies = frequencies;
        TotalFrequency = totalFrequency;
        Trie = trie;
    }

    public Trie Trie { get; }

    public int Count => _words.Count;

    public long TotalFrequency { get; }

    /// <summary>
    /// Gets an enumerable of all words in the dictionary.
    /// </summary>
    public IEnumerable<string> GetWords()
    {
        return _words;
    }

    public bool Contains(string word)
    {
        return word != null && _words.Contains(word);
    }

    public long GetFrequency(string word)
    {
        return word != null && _frequencies.TryGetValue(word, out long frequency) ? frequency : 1L;
    }

    public static KhmerDictionary Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A dictionary path is required.", nameof(path));
        }

        return FromLines(File.ReadLines(path));
    }

    public static KhmerDictionary FromLines(IEnumerable<string> lines)
    {
        if (lines == null)
        {
            throw new ArgumentNullException(nameof(lines));
        }

        var words = new HashSet<string>(StringComparer.Ordinal);
        var frequencies = new Dictionary<string, long>(StringComparer.Ordinal);
        var trie = new Trie();
        long totalFrequency = 0L;

        foreach (string rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            string line = rawLine.Trim();
            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            string[] fields = line.Split(new[] { '\t' }, 2);
            string word = fields[0].Trim();
            if (word.Length == 0 || !words.Add(word))
            {
                continue;
            }

            long frequency = 1L;
            if (fields.Length == 2 &&
                long.TryParse(fields[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsedFrequency) &&
                parsedFrequency > 0)
            {
                frequency = parsedFrequency;
            }

            frequencies[word] = frequency;
            totalFrequency += frequency;
            trie.Insert(word);
        }

        return new KhmerDictionary(words, frequencies, totalFrequency, trie);
    }
}
