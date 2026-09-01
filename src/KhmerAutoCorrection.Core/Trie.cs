using System;
using System.Collections.Generic;

namespace KhmerAutoCorrection.Core;

/// <summary>
/// A Unicode trie used for dictionary lookups and efficient prefix traversal.
/// Khmer characters are in the Basic Multilingual Plane, so .NET UTF-16 offsets
/// remain compatible with Microsoft Word range offsets.
/// </summary>
public sealed class Trie
{
    private readonly TrieNode _root = new TrieNode();

    public void Insert(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ArgumentException("A dictionary word cannot be empty.", nameof(word));
        }

        TrieNode node = _root;
        foreach (char character in word)
        {
            if (!node.Children.TryGetValue(character, out TrieNode? child))
            {
                child = new TrieNode();
                node.Children.Add(character, child);
            }

            node = child;
        }

        node.IsWord = true;
    }

    public bool Contains(string word)
    {
        TrieNode? node = FindNode(word);
        return node != null && node.IsWord;
    }

    public bool StartsWith(string prefix)
    {
        return FindNode(prefix) != null;
    }

    /// <summary>
    /// Returns exclusive text indexes for every dictionary word that begins at
    /// <paramref name="start"/>. The trie prevents scanning beyond a failed prefix.
    /// </summary>
    public IReadOnlyList<int> FindWordEnds(string text, int start)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (start < 0 || start > text.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        var matches = new List<int>();
        TrieNode node = _root;

        for (int index = start; index < text.Length; index++)
        {
            if (!node.Children.TryGetValue(text[index], out TrieNode? child))
            {
                break;
            }

            node = child;
            if (node.IsWord)
            {
                matches.Add(index + 1);
            }
        }

        return matches;
    }

    private TrieNode? FindNode(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        TrieNode node = _root;
        foreach (char character in value)
        {
            if (!node.Children.TryGetValue(character, out TrieNode? child))
            {
                return null;
            }

            node = child;
        }

        return node;
    }

    private sealed class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();

        public bool IsWord { get; set; }
    }
}
