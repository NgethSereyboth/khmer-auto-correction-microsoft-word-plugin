using System;
using System.Collections.Generic;

namespace KhmerAutoCorrection.Core;

/// <summary>
/// Dictionary-backed Viterbi segmenter for continuous Khmer text. Unknown Khmer
/// clusters remain on a valid path with a deliberate penalty, allowing callers to
/// underline them without losing their original Word range offsets.
/// </summary>
public sealed class KhmerSegmenter
{
    private const double NegativeInfinity = -1e100;
    private readonly KhmerDictionary _dictionary;

    public KhmerSegmenter(KhmerDictionary dictionary, double unknownClusterPenalty = 20d)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        if (unknownClusterPenalty <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(unknownClusterPenalty));
        }

        UnknownClusterPenalty = unknownClusterPenalty;
    }

    public double UnknownClusterPenalty { get; }

    public IReadOnlyList<SegmentToken> Segment(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var tokens = new List<SegmentToken>();
        int index = 0;
        while (index < text.Length)
        {
            if (!IsKhmer(text[index]))
            {
                index++;
                continue;
            }

            int runStart = index;
            while (index < text.Length && IsKhmer(text[index]))
            {
                index++;
            }

            tokens.AddRange(SegmentKhmerRun(text, runStart, index));
        }

        return tokens;
    }

    private IReadOnlyList<SegmentToken> SegmentKhmerRun(string text, int start, int end)
    {
        int length = end - start;
        var bestScore = new double[length + 1];
        var previous = new int[length + 1];
        var isKnownStep = new bool[length + 1];

        for (int index = 0; index <= length; index++)
        {
            bestScore[index] = NegativeInfinity;
            previous[index] = -1;
        }

        bestScore[0] = 0d;

        for (int relativeStart = 0; relativeStart < length; relativeStart++)
        {
            if (bestScore[relativeStart] == NegativeInfinity)
            {
                continue;
            }

            int absoluteStart = start + relativeStart;
            foreach (int absoluteEnd in _dictionary.Trie.FindWordEnds(text, absoluteStart))
            {
                if (absoluteEnd > end)
                {
                    break;
                }

                int relativeEnd = absoluteEnd - start;
                string word = text.Substring(absoluteStart, absoluteEnd - absoluteStart);
                double score = bestScore[relativeStart] + ScoreWord(word);
                TryUpdate(relativeEnd, relativeStart, true, score, bestScore, previous, isKnownStep);
            }

            int unknownEnd = NextClusterEnd(text, absoluteStart, end);
            int unknownRelativeEnd = unknownEnd - start;
            double unknownScore = bestScore[relativeStart] - UnknownClusterPenalty;
            TryUpdate(unknownRelativeEnd, relativeStart, false, unknownScore, bestScore, previous, isKnownStep);
        }

        var reversed = new List<SegmentToken>();
        int cursor = length;
        while (cursor > 0)
        {
            int before = previous[cursor];
            if (before < 0)
            {
                throw new InvalidOperationException("The segmentation path could not be reconstructed.");
            }

            int tokenStart = start + before;
            int tokenEnd = start + cursor;
            reversed.Add(new SegmentToken(tokenStart, tokenEnd, text.Substring(tokenStart, tokenEnd - tokenStart), isKnownStep[cursor]));
            cursor = before;
        }

        reversed.Reverse();
        return CoalesceUnknownTokens(reversed);
    }

    private static void TryUpdate(
        int target,
        int source,
        bool known,
        double score,
        double[] bestScore,
        int[] previous,
        bool[] isKnownStep)
    {
        if (score > bestScore[target])
        {
            bestScore[target] = score;
            previous[target] = source;
            isKnownStep[target] = known;
        }
    }

    private static IReadOnlyList<SegmentToken> CoalesceUnknownTokens(IReadOnlyList<SegmentToken> tokens)
    {
        var result = new List<SegmentToken>();
        foreach (SegmentToken token in tokens)
        {
            if (!token.IsKnown && result.Count > 0)
            {
                SegmentToken previous = result[result.Count - 1];
                if (!previous.IsKnown && previous.End == token.Start)
                {
                    result[result.Count - 1] = new SegmentToken(
                        previous.Start,
                        token.End,
                        previous.Value + token.Value,
                        false);
                    continue;
                }
            }

            result.Add(token);
        }

        return result;
    }

    private double ScoreWord(string word)
    {
        // Log-probabilities are negative. Maximizing their sum naturally
        // penalizes unnecessary boundaries, unlike raw positive frequencies,
        // which incorrectly favor a series of single-character entries.
        return Math.Log((double)_dictionary.GetFrequency(word) / _dictionary.TotalFrequency);
    }

    private static bool IsKhmer(char character)
    {
        return character >= '\u1780' && character <= '\u17ff';
    }

    private static int NextClusterEnd(string text, int start, int runEnd)
    {
        int index = start + 1;
        while (index < runEnd)
        {
            char character = text[index];
            if (character == '\u17d2' && index + 1 < runEnd)
            {
                index += 2;
                continue;
            }

            if (IsKhmerCombiningMark(character))
            {
                index++;
                continue;
            }

            break;
        }

        return index;
    }

    private static bool IsKhmerCombiningMark(char character)
    {
        return (character >= '\u17b6' && character <= '\u17d3') ||
               (character >= '\u17dd' && character <= '\u17dd');
    }
}
